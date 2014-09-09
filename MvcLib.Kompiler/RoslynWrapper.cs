using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;
using MvcLib.Common;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace MvcLib.Kompiler
{
    public class RoslynWrapper : IKompiler
    {
        static IEnumerable<MetadataReference> GetMetadataReferences()
        {
            foreach (var reference in KompilerEntryPoint.ReferencePaths)
            {
                yield return new MetadataFileReference(reference);
            }
        }

        static IProject CreateProject()
        {
            IProject project = Solution.Create(SolutionId.CreateNewId())
                .AddCSharpProject(KompilerEntryPoint.CompiledAssemblyName, KompilerEntryPoint.CompiledAssemblyName + ".dll")
                .Solution.Projects.Single()
                .UpdateParseOptions(new ParseOptions().WithLanguageVersion(LanguageVersion.CSharp5))
                .AddMetadataReferences(GetMetadataReferences())
                .UpdateCompilationOptions(new CompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            return project;
        }

        static string Compile(IProject project, out byte[] buffer)
        {
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

        static String TryCompile(string myCode, string assemblyName, out MemoryStream stream, OutputKind kind = OutputKind.ConsoleApplication)
        {

            // The MyClassInAString is where your code goes
            var syntaxTree = SyntaxTree.ParseText(myCode);

            // Use Roslyn to compile the code into a DLL
            var compiledCode = Compilation.Create(assemblyName,
                new CompilationOptions(kind),
                new[] { syntaxTree },
                GetMetadataReferences()
                );

            //return buffer;
            stream = new MemoryStream();


            StringBuilder sb = new StringBuilder();

            var compileResult = compiledCode.Emit(stream);
            if (!compileResult.Success)
            {
                foreach (var diagnostic in compileResult.Diagnostics)
                {
                    sb.AppendLine(diagnostic.Info.GetMessage());
                }
            }
            stream.Flush();
            return sb.ToString();
        }

        public string CompileFromSource(Dictionary<string, string> files, out byte[] buffer)
        {
            var project = CreateProject();

            foreach (var file in files)
            {
                var path = VirtualPathUtility.GetDirectory(file.Key);
                var p = VirtualPathUtility.ToAbsolute(path);
                var folders = p.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                var csDoc = project.AddDocument(file.Key, file.Value, folders);
                project = csDoc.Project;
            }

            return Compile(project, out buffer);
        }

        public string CompileFromFolder(string folder, out byte[] buffer)
        {
            var dirInfo = new DirectoryInfo(HostingEnvironment.MapPath(folder));
            if (!dirInfo.Exists)
            {
                buffer = new byte[0];
                return "Pasta {0} não encontada".Fmt(folder);
            }

            var project = CreateProject();

            foreach (var file in dirInfo.EnumerateFileSystemInfos("*.cs", SearchOption.AllDirectories))
            {
                var folders = file.FullName.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                var csDoc = project.AddDocument(file.FullName, File.ReadAllText(file.FullName), folders);
                project = csDoc.Project;
            }

            return Compile(project, out buffer);
        }

        public string CompileString(string text, out byte[] buffer)
        {
            MemoryStream ms;
            string result = TryCompile(text, KompilerEntryPoint.CompiledAssemblyName, out ms, OutputKind.DynamicallyLinkedLibrary);
            buffer = ms.GetBuffer();
            return result;
        }
    }
}