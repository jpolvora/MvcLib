using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using MvcLib.DbFileSystem;

namespace MvcLib.FsDump
{
    public class DbToLocal
    {
        public static void Execute()
        {
            Trace.TraceInformation("[DbToLocal]: Starting...");

            var _path = HostingEnvironment.MapPath("~/");
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);

            //procurar por todos os arquivos CS no DbFileSystem
            using (var ctx = new DbFileContext())
            {
                var dbFiles = ctx.DbFiles
                    .Where(x => !x.IsHidden && !x.IsDirectory)
                    .ToList();

                foreach (var dbFile in dbFiles)
                {
                    var localpath = _path + dbFile.VirtualPath.Replace("/", "\\");
                    var dir = Path.GetDirectoryName(localpath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if (File.Exists(localpath))
                    {
                        var fi = new FileInfo(localpath);
                        var mod = dbFile.Modified.HasValue ? dbFile.Modified.Value : dbFile.Created;
                        if (fi.LastWriteTime >= mod)
                            continue;
                        Trace.TraceWarning("[DbToLocal]:Arquivo será excluído: {0}", fi.FullName);
                        try
                        {
                            File.Delete(localpath);
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError(ex.Message);
                        }
                    }

                    Trace.TraceInformation("[DbToLocal]:Copiando arquivo: {0}", localpath);
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
            }
        }
    }
}
