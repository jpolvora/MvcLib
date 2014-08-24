using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace MvcFromDb.Infra.Plugin
{
    public class RoslynWrapper
    {
        public static List<MetadataReference> DefaultReferences = new List<MetadataReference>
        {
            MetadataReference. CreateAssemblyReference("mscorlib"),
            MetadataReference.CreateAssemblyReference("System"),
            MetadataReference.CreateAssemblyReference("System.Core"),
            MetadataReference.CreateAssemblyReference("System.Data"),
            MetadataReference.CreateAssemblyReference("System.Linq"),
            MetadataReference.CreateAssemblyReference("Microsoft.CSharp"),
            new MetadataFileReference(typeof (HttpContext).Assembly.Location), //self
            new MetadataFileReference(typeof (RoslynWrapper).Assembly.Location), //self
            new MetadataFileReference(typeof (Controller).Assembly.Location),
            new MetadataFileReference(typeof (WebPage).Assembly.Location),
            new MetadataFileReference(typeof (DbContext).Assembly.Location), //ef    
            
        };


        public static String TryCompile(string myCode, string assemblyName, out MemoryStream stream, OutputKind kind = OutputKind.ConsoleApplication)
        {


            // The MyClassInAString is where your code goes
            var syntaxTree = SyntaxTree.ParseText(myCode);

            // Use Roslyn to compile the code into a DLL
            var compiledCode = Compilation.Create(assemblyName,
                new CompilationOptions(kind),
                new[] { syntaxTree },
                DefaultReferences
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