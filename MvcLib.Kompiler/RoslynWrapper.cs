using System;
using System.IO;
using System.Text;
using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace MvcLib.PluginCompiler
{
    public class RoslynWrapper
    {
      

        public static String TryCompile(string myCode, string assemblyName, out MemoryStream stream, OutputKind kind = OutputKind.ConsoleApplication)
        {


            // The MyClassInAString is where your code goes
            var syntaxTree = SyntaxTree.ParseText(myCode);

            // Use Roslyn to compile the code into a DLL
            var compiledCode = Compilation.Create(assemblyName,
                new CompilationOptions(kind),
                new[] { syntaxTree },
                Kompiler.DefaultReferences
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