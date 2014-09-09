using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Microsoft.CSharp;
using MvcLib.Common;

namespace MvcLib.Kompiler
{
    public class CodeDomWrapper : IKompiler
    {
        static CodeDomWrapper()
        {
            if (!AppDomain.CurrentDomain.IsFullyTrusted)
            {
                Trace.TraceWarning("CodeDom works only in full trust!");
            }
        }

        public string CompileFromSource(Dictionary<string, string> files, out byte[] buffer)
        {
            if (files == null)
                throw new ArgumentNullException("files");

            var output = Path.Combine(Path.GetTempPath(), KompilerEntryPoint.CompiledAssemblyName + ".dll");

            CodeDomProvider codeDomProvider = new CSharpCodeProvider();
            var compilerParameters = new CompilerParameters
            {
                OutputAssembly = output,
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = false,
            };

            foreach (var codeDomReference in KompilerEntryPoint.ReferencePaths)
            {
                compilerParameters.ReferencedAssemblies.Add(codeDomReference);
            }

            var src = files.Select(s => s.Value).ToArray();

            CompilerResults result = codeDomProvider.CompileAssemblyFromSource(compilerParameters, src);

            buffer = new byte[0];

            if (result.Errors.HasErrors)
            {
                var sb = new StringBuilder();
                foreach (CompilerError error in result.Errors)
                {
                    sb.AppendFormat("Erro {0}, {1}", error.FileName, error.ErrorText).AppendLine();
                }
                return sb.ToString();
            }

            var file = result.PathToAssembly;
            buffer = File.ReadAllBytes(file);

            return string.Empty;
        }

        public string CompileFromFolder(string folder, out byte[] buffer)
        {
            var dirInfo = new DirectoryInfo(HostingEnvironment.MapPath(folder));
            if (!dirInfo.Exists)
            {
                buffer = new byte[0];
                return "Pasta {0} não encontada".Fmt(folder);
            }

            Dictionary<string, string> source = new Dictionary<string, string>();

            foreach (var file in dirInfo.EnumerateFileSystemInfos("*.cs", SearchOption.AllDirectories))
            {
                var src = File.ReadAllText(file.FullName);
                source.Add(file.Name, src);
            }

            return CompileFromSource(source, out buffer);
        }

        public string CompileString(string text, out byte[] buffer)
        {
            var dict = new Dictionary<string, string>()
            {
                {Guid.NewGuid().ToString("N"), text}
            };

            return CompileFromSource(dict, out buffer);
        }
    }
}