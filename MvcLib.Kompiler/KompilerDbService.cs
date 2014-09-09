using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using MvcLib.DbFileSystem;

namespace MvcLib.Kompiler
{
    public class KompilerDbService
    {
        public static string CompileAndSave(IKompiler compiler)
        {
            if (compiler == null)
                throw new ArgumentNullException("compiler");

            var src = LoadSourceCodeFromDb();
            byte[] buffer;
            var result = compiler.CompileFromSource(src, out buffer);

            if (string.IsNullOrEmpty(result))
            {
                SaveCompiledCustomAssembly(buffer);
            }

            return result;
        }

        public static Dictionary<string, string> LoadSourceCodeFromDb()
        {
            var dict = new Dictionary<string, string>();

            //procurar por todos os arquivos CS no DbFileSystem
            using (var ctx = new DbFileContext())
            {
                var csharpFiles = ctx.DbFiles
                    .Where(x => !x.IsHidden && !x.IsDirectory && x.Extension.Equals(".cs", StringComparison.InvariantCultureIgnoreCase))
                    .Select(s => new { s.VirtualPath, s.Texto })
                    .ToList();

                foreach (var dbFile in csharpFiles)
                {
                    if (string.IsNullOrWhiteSpace(dbFile.Texto))
                        continue;

                    dict.Add(dbFile.VirtualPath, dbFile.Texto);
                }
            }

            return dict;
        }

        public static bool ExistsCompiledAssembly()
        {
            using (var ctx = new DbFileContext())
            {
                var path = "/" + KompilerEntryPoint.CompiledAssemblyName + ".dll";
                return ctx.DbFiles.Any(x => x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public static void RemoveExistingCompiledAssemblyFromDb()
        {
            using (var ctx = new DbFileContext())
            {
                var path = "/" + KompilerEntryPoint.CompiledAssemblyName + ".dll";
                var existingFile = ctx.DbFiles.FirstOrDefault(x => x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase));
                if (existingFile != null)
                {
                    ctx.DbFiles.Remove(existingFile);
                    ctx.SaveChanges();
                    Trace.TraceInformation("[Kompiler]: Compiled Assembly Found and removed.");
                }
            }
        }

        public static void SaveCompiledCustomAssembly(byte[] buffer)
        {
            RemoveExistingCompiledAssemblyFromDb();

            var path = "/" + KompilerEntryPoint.CompiledAssemblyName + ".dll";

            using (var ctx = new DbFileContext())
            {
                var root = ctx.DbFiles.Include(x => x.Children).First(x => x.IsDirectory && x.ParentId == null && x.Name == null && x.VirtualPath.Equals("/", StringComparison.InvariantCultureIgnoreCase));

                var file = new DbFile
                {
                    ParentId = root.Id,
                    IsDirectory = false,
                    Name = KompilerEntryPoint.CompiledAssemblyName,
                    Extension = ".dll",
                    IsBinary = true,
                    VirtualPath = path,
                    Bytes = buffer
                };

                ctx.DbFiles.Add(file);
                ctx.SaveChanges();
            }
        }
    }
}
