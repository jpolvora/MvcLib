using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Razor;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.WebPages;
using System.Web.WebPages.Razor;
using Microsoft.CSharp;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace MvcLib.Kompiler
{
    public class RazorCompiler
    {
        public static string GenereateCode(string sourceRazor, string virtualPath, bool throws = false)
        {
            try
            {
                var extension = VirtualPathUtility.GetExtension(virtualPath);
                if (extension.IsEmpty() || extension != ".cshtml")
                {
                    return string.Empty;
                }

                Trace.TraceInformation("Parsing razor... {0}", virtualPath);
                var host = WebRazorHostFactory.CreateHostFromConfig(virtualPath);
                string code = GenerateCodeFromRazorString(host, sourceRazor, virtualPath);

                return code;
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("Error:... {0}", ex.Message);
                if (throws)
                    throw;

                return ex.Message;
            }
        }

        private static string GenerateCodeFromRazorTemplate(WebPageRazorHost host, string virtualPath)
        {

            // Create Razor engine and use it to generate a CodeCompileUnit
            var engine = new RazorTemplateEngine(host);
            GeneratorResults results = null;
            VirtualFile file = HostingEnvironment.VirtualPathProvider.GetFile(virtualPath);
            using (var stream = file.Open())
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    results = engine.GenerateCode(reader, className: null, rootNamespace: null, sourceFileName: host.PhysicalPath);
                }
            }

            if (!results.Success)
            {
                throw CreateExceptionFromParserError(results.ParserErrors.Last(), virtualPath);
            }

            // Use CodeDom to generate source code from the CodeCompileUnit
            var codeDomProvider = new CSharpCodeProvider();
            var srcFileWriter = new StringWriter();
            codeDomProvider.GenerateCodeFromCompileUnit(results.GeneratedCode, srcFileWriter, new CodeGeneratorOptions());

            return srcFileWriter.ToString();
        }

        private static string GenerateCodeFromRazorString(WebPageRazorHost host, string razorString, string virtualPath)
        {

            // Create Razor engine and use it to generate a CodeCompileUnit
            var engine = new RazorTemplateEngine(host);
            GeneratorResults results = null;

            using (StringReader reader = new StringReader(razorString))
            {
                results = engine.GenerateCode(reader, className: null, rootNamespace: null,
                    sourceFileName: host.PhysicalPath);
            }

            if (!results.Success)
            {
                throw CreateExceptionFromParserError(results.ParserErrors.Last(), virtualPath);
            }

            // Use CodeDom to generate source code from the CodeCompileUnit
            using (var codeDomProvider = new CSharpCodeProvider())
            {
                using (var srcFileWriter = new StringWriter())
                {
                    codeDomProvider.GenerateCodeFromCompileUnit(results.GeneratedCode, srcFileWriter,
                        new CodeGeneratorOptions());

                    return srcFileWriter.ToString();
                }
            }
        }


        private static HttpParseException CreateExceptionFromParserError(RazorError error, string virtualPath)
        {
            return new HttpParseException(error.Message + Environment.NewLine, null, virtualPath, null, error.Location.LineIndex + 1);
        }

        private static Assembly CompileCodeIntoAssembly(string code, string virtualPath)
        {
            // Parse the source file using Roslyn
            SyntaxTree syntaxTree = SyntaxTree.ParseText(code);

            // Add all the references we need for the compilation
            var references = new List<MetadataReference>();
            foreach (Assembly referencedAssembly in BuildManager.GetReferencedAssemblies())
            {
                references.Add(new MetadataFileReference(referencedAssembly.Location));
            }

            var compilationOptions = new CompilationOptions(outputKind: OutputKind.DynamicallyLinkedLibrary);

            // Note: using a fixed assembly name, which doesn't matter as long as we don't expect cross references of generated assemblies
            var compilation = Compilation.Create("SomeAssemblyName", compilationOptions, new[] { syntaxTree }, references);

            // Generate the assembly into a memory stream
            var memStream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(memStream);

            if (!emitResult.Success)
            {
                Diagnostic diagnostic = emitResult.Diagnostics.First();
                string message = diagnostic.Info.ToString();
                LinePosition linePosition = diagnostic.Location.GetLineSpan(usePreprocessorDirectives: true).StartLinePosition;

                throw new HttpParseException(message, null, virtualPath, null, linePosition.Line + 1);
            }

            return Assembly.Load(memStream.GetBuffer());
        }
    }
}