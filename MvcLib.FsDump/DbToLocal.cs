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

            var root = Path.GetFullPath(HostingEnvironment.MapPath("~/"));
            var dirInfo = new DirectoryInfo(root);
            if (!dirInfo.Exists)
                dirInfo.Create();

            //procurar por todos os arquivos CS no DbFileSystem
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
                        localpath = Path.Combine(dirInfo.FullName, string.Format("App_Code{0}", dbFile.VirtualPath.Replace("/","\\")));
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
                        var mod = dbFile.Modified.HasValue ? dbFile.Modified.Value : dbFile.Created;
                        if (fi.LastWriteTime > mod)
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