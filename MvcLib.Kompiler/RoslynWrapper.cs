﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;

namespace MvcLib.Kompiler
{
    public class RoslynWrapper
    {
        public static string CreateSolutionAndCompile(Dictionary<string, byte[]> files, out byte[] buffer)
        {
            IProject project = Solution.Create(SolutionId.CreateNewId())
                .AddCSharpProject(EntryPoint.CompiledAssemblyName, EntryPoint.CompiledAssemblyName + ".dll")
                .Solution.Projects.Single()
                .UpdateParseOptions(new ParseOptions().WithLanguageVersion(LanguageVersion.CSharp5))
                .AddMetadataReferences(EntryPoint.DefaultReferences)
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

        public static String TryCompile(string myCode, string assemblyName, out MemoryStream stream, OutputKind kind = OutputKind.ConsoleApplication)
        {


            // The MyClassInAString is where your code goes
            var syntaxTree = SyntaxTree.ParseText(myCode);

            // Use Roslyn to compile the code into a DLL
            var compiledCode = Compilation.Create(assemblyName,
                new CompilationOptions(kind),
                new[] { syntaxTree },
                EntryPoint.DefaultReferences
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

    }
}