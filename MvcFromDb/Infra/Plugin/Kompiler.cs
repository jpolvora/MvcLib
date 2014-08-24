using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using MvcFromDb.Infra.Entities;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace MvcFromDb.Infra.Plugin
{
    public class Kompiler
    {
        public static string CreateSolutionAndCompile(Dictionary<string, byte[]> files, out byte[] buffer)
        {

            IProject project = Solution.Create(SolutionId.CreateNewId())
                .AddCSharpProject(PluginLoader.CompiledAssemblyName, "CustomAssembly")
                .Solution.Projects.Single()
                .AddMetadataReferences(RoslynWrapper.DefaultReferences)
                .UpdateCompilationOptions(new CompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            foreach (var file in files)
            {
                var path = VirtualPathUtility.GetDirectory(file.Key);
                var p = VirtualPathUtility.ToAbsolute(path);
                var folders = p.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                var csDoc = project.AddDocument(file.Key, Encoding.UTF8.GetString(file.Value), folders);
                project = csDoc.Project;
            }

            buffer = new byte[0];

            try
            {
                using (var stream = new MemoryStream())
                {
                    var comp = project.GetCompilation();

                    var result = comp.Emit(stream);

                    if (!result.Success)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var diagnostic in result.Diagnostics)
                        {
                            sb.AppendLine(diagnostic.Info.GetMessage());
                        }

                        return sb.ToString();
                    }

                    buffer = stream.ToArray();

                    return String.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string CreateAndSaveAssemblyFromDbFiles(string assName)
        {
            Byte[] buffer;
            var dict = LoadSourceCodeFromDb();
            var result = CreateSolutionAndCompile(dict, out buffer);

            if (!String.IsNullOrEmpty(result)) return result;

            SaveAssemblyToDataBase(assName, buffer);

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
                    dict.Add(dbFile.VirtualPath, Encoding.UTF8.GetBytes(dbFile.Texto));
                }
            }

            return dict;
        }

        public static void SaveAssemblyToDataBase(string assName, byte[] buffer)
        {
            using (var ctx = new DbFileContext())
            {
                var root = ctx.DbFiles.Include(x => x.Children).First(x => x.IsDirectory && x.ParentId == null && x.Name == null && x.VirtualPath.Equals("/", StringComparison.InvariantCultureIgnoreCase) && x.IsDirectory);
                var existingFile = root.Children.FirstOrDefault(x => x.VirtualPath.Equals("/" + PluginLoader.CompiledAssemblyName, StringComparison.InvariantCultureIgnoreCase));
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