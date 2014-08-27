using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using MvcLib.Common;
using MvcLib.Common.Cache;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace MvcLib.PluginCompiler
{
    public class Kompiler
    {
        public const string CompiledAssemblyName = "db-compiled-assembly";

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
            //new MetadataFileReference(typeof (Roslyn.Services.Solution).Assembly.Location), //self
            //new MetadataFileReference(typeof (Roslyn.Compilers.CSharp.Compilation).Assembly.Location), //self
            //new MetadataFileReference(typeof (Roslyn.Compilers.Common.CommonCompilation).Assembly.Location), //self
            //new MetadataFileReference(typeof (Roslyn.Scripting.Session).Assembly.Location), //self
            new MetadataFileReference(typeof (RoslynWrapper).Assembly.Location), //self            
            new MetadataFileReference(typeof (DbContext).Assembly.Location), //ef    
            //new MetadataFileReference(typeof (WebCacheWrapper).Assembly.Location), //ef    
            //new MetadataFileReference(typeof (ViewRenderer).Assembly.Location), //ef    
            //new MetadataFileReference(typeof (DbToLocal).Assembly.Location), //ef    
        };

        public static void AddReferences(params Type[] types)
        {
            foreach (var type in types)
            {
                DefaultReferences.Add(new MetadataFileReference(type.Assembly.Location));
            }
        }

        public static void AddReferences(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                DefaultReferences.Add(new MetadataFileReference(assembly.Location));
            }
        }

        public static void Initialize()
        {
            using (DisposableTimer.StartNew("Assembly Compilation"))
            {
                byte[] buffer;
                var msg = KompilerDbService.TryCreateAndSaveAssemblyFromDbFiles(CompiledAssemblyName, out buffer);
                if (string.IsNullOrWhiteSpace(msg) && buffer.Length > 0)
                {
                    Trace.TraceInformation("[PluginLoader]: DB Compilation Result: SUCCESS");

                    PluginLoader.PluginLoader.LoadPlugin(CompiledAssemblyName + ".dll", buffer);
                }
                else
                {
                    Trace.TraceInformation("[PluginLoader]: DB Compilation Result: Bytes:{0}, Msg:{1}",
                        buffer.Length, msg);
                }
            }
        }


        public static string CreateSolutionAndCompile(Dictionary<string, byte[]> files, out byte[] buffer)
        {
            IProject project = Solution.Create(SolutionId.CreateNewId())
                .AddCSharpProject(CompiledAssemblyName, CompiledAssemblyName + ".dll")
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