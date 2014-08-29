﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Hosting;
using MvcLib.Common;
using MvcLib.DbFileSystem;

namespace MvcLib.FsDump
{
    public class DbToLocal
    {
        static void RecursiveDelete(DirectoryInfo fsInfo, params  string[] extensions)
        {
            foreach (var info in fsInfo.EnumerateFileSystemInfos())
            {
                if (info is DirectoryInfo)
                    RecursiveDelete((DirectoryInfo)info, extensions);

                info.Delete();
            }
        }

        public static void Execute()
        {
            Trace.TraceInformation("[DbToLocal]: Starting...");

            var path = Config.ValueOrDefault("DumpToLocalFolder", "~/dbfiles");

            var appCode = HostingEnvironment.MapPath("~/App_Code/");

            //var appCodeInfo = new DirectoryInfo(appCode);

            var root = Path.GetFullPath(HostingEnvironment.MapPath(path));
            var dirInfo = new DirectoryInfo(root);
            if (!dirInfo.Exists)
                dirInfo.Create();
            else
                RecursiveDelete(dirInfo);

            using (var ctx = new DbFileContext())
            {
                var dbFiles = ctx.DbFiles
                    .Where(x => !x.IsHidden && !x.IsDirectory && x.Extension != ".dll")
                    .ToList();

                foreach (var dbFile in dbFiles)
                {
                    string localpath;
                    if (dbFile.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        //copia p/ app_code
                        localpath = appCode + dbFile.VirtualPath.Replace("/", "\\");
                    }
                    else
                    {
                        localpath = dirInfo.FullName + dbFile.VirtualPath.Replace("/", "\\");
                    }
                    var dir = Path.GetDirectoryName(localpath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if (File.Exists(localpath))
                    {
                        var fi = new FileInfo(localpath);

                        if (fi.LastWriteTimeUtc > dbFile.LastWriteUtc)
                            continue;
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

                    Trace.TraceInformation("[DbToLocal]:Copiando arquivo: '{0}'", dbFile.VirtualPath);
                    try
                    {
                        DbFile file = dbFile;
                        ThreadPool.QueueUserWorkItem(x =>
                        {
                            if (file.IsBinary && file.Bytes.Length > 0)
                                File.WriteAllBytes(localpath, file.Bytes);
                            else File.WriteAllText(localpath, file.Texto);
                        });
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
            }
        }
    }
}