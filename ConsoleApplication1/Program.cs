using System;
using System.IO;
using System.Linq;
using System.Text;
using MvcLib.Common;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Digite 1 para ler do banco ou 2 para gravar no banco.");
                var option = Console.ReadKey();
                bool ok = false;
                switch (option.Key)
                {
                    case ConsoleKey.D1:
                        {
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

                                    var setDirStr = Config.ValueOrDefault("dumpDir", "..\\..\\..\\MvcFromDb");

                                    Directory.SetCurrentDirectory(setDirStr);
                                    var root = new DirectoryInfo(Directory.GetCurrentDirectory());

                                    if (!root.Exists)
                                    {
                                        throw new Exception("Invalid directory " + root.FullName);
                                    }
                                    WriteFilesToDatabase(db, new Uri(root.FullName), root, null);

                                    db.Database.ExecuteSqlCommand(@"EXEC sp_msforeachtable ""ALTER TABLE ? CHECK CONSTRAINT all""");
                                    ok = true;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
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
            Console.ReadLine();
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

        private static readonly string[] IgnoredDirectories = { "/bin", "/App_", "/obj", "/properties", "/_", "/content", "/scripts", "/fonts" };
        private static readonly string[] IgnoredExtensions = { ".csproj", ".user", ".dll", ".config", ".log" };
        private static readonly string[] IgnoredFiles = { "global.asax", "global.asax.cs" };
        private static readonly string[] TextExtensions = { ".txt", ".xml", ".cshtml", ".js", ".html", ".css", ".cs", ".csx" };
        private static bool IsTextFile(string extension)
        {
            return TextExtensions.Any(extension.StartsWith); //remove the dot "."
        }
    }
}
