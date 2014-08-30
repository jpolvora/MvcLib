using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MvcLib.Common;
using MvcLib.DbFileSystem;

namespace ConsoleApplication1
{
    class Program
    {
        private static DirectoryInfo DirInfo;

        static void Main(string[] args)
        {
            var setDirStr = Config.ValueOrDefault("dumpDir", "..\\..\\..\\HostWebApp\\dbfiles");

            Directory.SetCurrentDirectory(setDirStr);
            DirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());

            if (!DirInfo.Exists)
            {
                throw new InvalidOperationException("Invalid directory " + DirInfo.FullName);
            }

            while (true)
            {
                Console.WriteLine("1: Db To Local");
                Console.WriteLine("2: Local To Db");

                var option = Console.ReadKey();
                Console.WriteLine("Opção escolhida: {0} ", option);
                Console.WriteLine();
                bool ok = false;
                switch (option.Key)
                {
                    case ConsoleKey.D1:
                        {
                            using (var ctx = new DbFileContext())
                            {
                                try
                                {
                                    var dbFiles = ctx.DbFiles
                                        .Where(x => !x.IsHidden && !x.IsDirectory)
                                        .ToList();

                                    foreach (var dbFile in dbFiles)
                                    {
                                        WriteToDisk(dbFile, false);
                                    }

                                    ok = true;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    Console.Beep(880, 500);
                                }
                            }
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            using (var db = new DbFileContext())
                            {
                                try
                                {
                                    db.Database.ExecuteSqlCommand(@"EXEC sp_msforeachtable ""ALTER TABLE ? NOCHECK CONSTRAINT all""");
                                    db.Database.ExecuteSqlCommand("DELETE FROM DbFiles");
                                    db.Database.ExecuteSqlCommand(@"EXEC sp_executesql ""DBCC CHECKIDENT('DbFiles', RESEED, 0)"" ");

                                    WriteFilesToDatabase(db, new Uri(DirInfo.FullName), DirInfo, null);

                                    db.Database.ExecuteSqlCommand(@"EXEC sp_msforeachtable ""ALTER TABLE ? CHECK CONSTRAINT all""");
                                    ok = true;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                    Console.Beep(880, 500);
                                }

                                break;
                            }
                        }
                    default:
                        {
                            Console.Clear();
                            break;
                        }

                }

                if (ok)
                    break;
            }

            Console.WriteLine("");
            Console.WriteLine("Fim. Pression qualquer tecla ...");
            Console.Beep(440, 1000);
            Console.ReadLine();
        }

        static string GetLocalPath(DbFile dbFile)
        {
            string localpath = DirInfo.FullName + dbFile.VirtualPath.Replace("/", "\\");
            return localpath;
        }

        public static void WriteToDisk(DbFile dbFile, bool force)
        {
            Trace.TraceInformation("[DbToLocal]:Copiando arquivo: '{0}'", dbFile.VirtualPath);

            var localpath = GetLocalPath(dbFile);

            if (File.Exists(localpath))
            {
                var fi = new FileInfo(localpath);

                if (fi.LastWriteTimeUtc > dbFile.LastWriteUtc && !force)
                    return;

                Trace.TraceWarning("[DbToLocal]:Arquivo será excluído: {0}/{1}", fi.FullName, fi.LastAccessTimeUtc);
                try
                {
                    File.Delete(localpath);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                }
            }

            var dir = Path.GetDirectoryName(localpath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            try
            {
                if (dbFile.IsBinary && dbFile.Bytes.Length > 0)
                    File.WriteAllBytes(localpath, dbFile.Bytes);
                else File.WriteAllText(localpath, dbFile.Texto);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }

        }

        public static void WriteFilesToDatabase(DbFileContext ctx, Uri initialUri, DirectoryInfo root, int? id)
        {
            string virtualPath;
            string dirName;
            if (id == null)
            {
                virtualPath = "/";
                dirName = null;
            }
            else
            {
                var currentUri = new Uri(root.FullName);
                var tempRelative = initialUri.MakeRelativeUri(currentUri).ToString();
                var iof = tempRelative.IndexOf('/');
                virtualPath = tempRelative.Substring(iof);

                dirName = root.Name;
            }

            foreach (var ignoredDirectory in IgnoredDirectories)
            {
                if (virtualPath.StartsWith(ignoredDirectory, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            var dbFile = new DbFile
            {
                IsDirectory = true,
                Name = dirName,
                VirtualPath = virtualPath,
                ParentId = id
            };

            ctx.DbFiles.Add(dbFile);
            ctx.SaveChanges();

            foreach (var fi in root.EnumerateFiles())
            {
                bool ignore = IgnoredExtensions.Any(ignoredExtension => fi.Extension.StartsWith(ignoredExtension))
                              || IgnoredFiles.Any(x => x.Equals(fi.Name, StringComparison.OrdinalIgnoreCase));

                if (ignore)
                    continue;

                Console.WriteLine(fi.FullName);

                var dbFileFolder = new DbFile
                {
                    IsDirectory = false,
                    Name = Path.GetFileNameWithoutExtension(fi.Name),
                    Extension = fi.Extension,
                    VirtualPath = Path.Combine(virtualPath, fi.Name).Replace('\\', '/'),
                    ParentId = dbFile.Id,
                };

                if (IsTextFile(fi.Extension))
                {
                    var text = File.ReadAllText(fi.FullName, Encoding.UTF8);
                    dbFileFolder.Texto = text;
                }
                else
                {
                    var bytes = File.ReadAllBytes(fi.FullName);
                    dbFileFolder.Bytes = bytes;
                    dbFileFolder.IsBinary = true;
                }

                ctx.DbFiles.Add(dbFileFolder);
                ctx.SaveChanges();
            }

            foreach (var di in root.EnumerateDirectories())
            {
                WriteFilesToDatabase(ctx, initialUri, di, dbFile.Id);
            }
        }

        private static readonly string[] IgnoredDirectories = { "/bin", "/App_", "/obj", "/properties", "/_", "/fonts" };
        private static readonly string[] IgnoredExtensions = { ".csproj", ".user", ".dll", ".config", ".log" };
        private static readonly string[] IgnoredFiles = { "global.asax", "global.asax.cs" };
        private static readonly string[] TextExtensions = { ".txt", ".xml", ".cshtml", ".js", ".html", ".css", ".cs", ".csx" };
        private static bool IsTextFile(string extension)
        {
            return TextExtensions.Any(extension.StartsWith); //remove the dot "."
        }
    }
}
