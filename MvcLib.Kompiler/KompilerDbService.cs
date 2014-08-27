using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MvcLib.DbFileSystem;

namespace MvcLib.PluginCompiler
{
    class KompilerDbService
    {
        public static string TryCreateAndSaveAssemblyFromDbFiles(string assName, out byte[] buffer)
        {
            string result = "";
            try
            {
                var dict = LoadSourceCodeFromDb();
                result = Kompiler.CreateSolutionAndCompile(dict, out buffer);

                if (!String.IsNullOrEmpty(result)) return result;

                SaveCompiledCustomAssembly(assName, buffer);

                return result;
            }
            catch (Exception ex)
            {
                result = ex.Message;
                Trace.TraceError("Erro durante a compilação do projeto no banco de dados. \r\n" + ex.Message);
            }

            buffer = new byte[0];
            return result;
        }

        public static Dictionary<string, byte[]> LoadSourceCodeFromDb()
        {
            var dict = new Dictionary<string, byte[]>();

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

                    dict.Add(dbFile.VirtualPath, Encoding.UTF8.GetBytes(dbFile.Texto));
                }
            }

            return dict;
        }

        public static void SaveCompiledCustomAssembly(string assName, byte[] buffer)
        {
            using (var ctx = new DbFileContext())
            {
                var root = ctx.DbFiles.Include(x => x.Children).First(x => x.IsDirectory && x.ParentId == null && x.Name == null && x.VirtualPath.Equals("/", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory);
                var existingFile = root.Children.FirstOrDefault(x => x.VirtualPath.Equals("/" + Kompiler.CompiledAssemblyName + ".dll", StringComparison.InvariantCultureIgnoreCase));
                if (existingFile != null)
                {
                    ctx.DbFiles.Remove(existingFile);
                    ctx.SaveChanges();
                }

                var file = new DbFile
                {
                    ParentId = root.Id,
                    IsDirectory = false,
                    Name = assName,
                    Extension = ".dll",
                    IsBinary = true
                };
                file.VirtualPath = "/" + file.Name + ".dll";
                file.Bytes = buffer;

                ctx.DbFiles.Add(file);
                ctx.SaveChanges();
            }
        }
    }
}
