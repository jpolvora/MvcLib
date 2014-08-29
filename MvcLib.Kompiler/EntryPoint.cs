using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Reflection;
using MvcLib.Common;
using Roslyn.Compilers;

namespace MvcLib.Kompiler
{
    public class EntryPoint
    {
        internal const string CompiledAssemblyName = "db-compiled-assembly";

        private static bool _initialized;

        internal static List<MetadataReference> DefaultReferences = new List<MetadataReference>
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
            new MetadataFileReference(typeof (DbContext).Assembly.Location), //ef    
        };

        public static void AddReferences(params Type[] types)
        {
            if (_initialized)
                throw new InvalidOperationException("Compilador só pode ser executado no Pre-Start!");

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

        public static void Execute()
        {
            if (_initialized)
                throw new InvalidOperationException("Compilador só pode ser executado no Pre-Start!");

            _initialized = true;

            using (DisposableTimer.StartNew("Assembly Compilation from dbfiles"))
            {
                byte[] buffer;
                string msg;
                if (Config.ValueOrDefault("DumpToLocal", false))
                {
                    msg = RoslynWrapper.CreateSolutionAndCompile(
                        Config.ValueOrDefault("DumpToLocalFolder", "~/dbfiles"), out buffer);
                }
                else
                {
                    msg = KompilerDbService.TryCreateAndSaveAssemblyFromDbFiles(CompiledAssemblyName, out buffer);
                }

                if (string.IsNullOrWhiteSpace(msg) && buffer.Length > 0)
                {
                    Trace.TraceInformation("[PluginLoader]: DB Compilation Result: SUCCESS");

                    PluginLoader.EntryPoint.LoadPlugin(CompiledAssemblyName + ".dll", buffer);
                }
                else
                {
                    Trace.TraceInformation("[PluginLoader]: DB Compilation Result: Bytes:{0}, Msg:{1}",
                        buffer.Length, msg);
                }
            }
        }
    }
}