using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using MvcLib.Common;
using MvcLib.Common.Mvc;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace MvcLib.PluginCompiler
{
    public class Kompiler
    {
        public static List<MetadataReference> DefaultReferences = new List<MetadataReference>
        {
            MetadataReference. CreateAssemblyReference("mscorlib"),
            MetadataReference.CreateAssemblyReference("System"),
            MetadataReference.CreateAssemblyReference("System.Core"),
            MetadataReference.CreateAssemblyReference("System.Data"),
            MetadataReference.CreateAssemblyReference("System.Linq"),
            MetadataReference.CreateAssemblyReference("Microsoft.CSharp"),
            MetadataReference.CreateAssemblyReference("System.Web"),
            MetadataReference.CreateAssemblyReference("System.ComponentModel.DataAnnotations"),
            new MetadataFileReference(typeof (Roslyn.Services.Solution).Assembly.Location), //self
            new MetadataFileReference(typeof (Roslyn.Compilers.CSharp.Compilation).Assembly.Location), //self
            new MetadataFileReference(typeof (Roslyn.Compilers.Common.CommonCompilation).Assembly.Location), //self
            new MetadataFileReference(typeof (Roslyn.Scripting.Session).Assembly.Location), //self
            new MetadataFileReference(typeof (RoslynWrapper).Assembly.Location), //self
            new MetadataFileReference(typeof (Controller).Assembly.Location),
            new MetadataFileReference(typeof (WebPage).Assembly.Location),
            new MetadataFileReference(typeof (DbContext).Assembly.Location), //ef    
            new MetadataFileReference(typeof (CacheWrapper).Assembly.Location), //ef    
            new MetadataFileReference(typeof (ViewRenderer).Assembly.Location), //ef    
        };


        public static string CreateSolutionAndCompile(Dictionary<string, byte[]> files, out byte[] buffer)
        {
            IProject project = Solution.Create(SolutionId.CreateNewId())
                .AddCSharpProject(PluginLoader.CompiledAssemblyName, PluginLoader.CompiledAssemblyName + ".dll")
                .Solution.Projects.Single()
                .UpdateParseOptions(new ParseOptions().WithLanguageVersion(LanguageVersion.CSharp5))
                .AddMetadataReferences(DefaultReferences)
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
                            sb.AppendFormat("{0} - {1}", diagnostic.Info.Severity, diagnostic.Info.GetMessage())
                                .AppendLine();
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
    }
}