using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using MvcLib.Common.Configuration;
using MvcLib.PluginLoader;
using Roslyn.Compilers;

namespace MvcLib.Kompiler
{
    public class KompilerEntryPoint
    {
        internal readonly static string CompiledAssemblyName = BootstrapperSection.Instance.Kompiler.AssemblyName;

        private static bool _initialized;

        public static void Execute()
        {
            if (_initialized)
                throw new InvalidOperationException("Compilador só pode ser executado no Pre-Start!");

            _initialized = true;

            //plugin loader 
            PluginLoaderEntryPoint.Initialize();

            if (BootstrapperSection.Instance.Kompiler.ForceRecompilation)
            {
                //se forçar a recompilação, remove o assembly existente.
                KompilerDbService.RemoveExistingCompiledAssemblyFromDb();
            }

            AddReferences(PluginStorage.GetAssemblies().ToArray());

            byte[] buffer = new byte[0];
            string msg;

            try
            {
                //todo: usar depdendency injection
                IKompiler kompiler;

                if (BootstrapperSection.Instance.Kompiler.Roslyn)
                {
                    kompiler = new RoslynWrapper();
                }
                else
                {
                    kompiler = new CodeDomWrapper();
                }

                if (BootstrapperSection.Instance.Kompiler.LoadFromDb)
                {
                    Trace.TraceInformation("Compiling from DB...");
                    var source = KompilerDbService.LoadSourceCodeFromDb();
                    msg = kompiler.CompileFromSource(source, out buffer);
                }
                else
                {
                    var localRootFolder = BootstrapperSection.Instance.DumpToLocal.Folder;
                    Trace.TraceInformation("Compiling from Local File System: {0}", localRootFolder);
                    msg = kompiler.CompileFromFolder(localRootFolder, out buffer);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                Trace.TraceInformation("Erro durante a compilação do projeto no banco de dados. \r\n" + ex.Message);
            }

            if (string.IsNullOrWhiteSpace(msg) && buffer.Length > 0)
            {
                Trace.TraceInformation("[Kompiler]: DB Compilation Result: SUCCESS");

                if (!BootstrapperSection.Instance.Kompiler.ForceRecompilation)
                {
                    //só salva no banco se compilação forçada for False
                    KompilerDbService.SaveCompiledCustomAssembly(buffer);
                }

                PluginLoaderEntryPoint.SaveAndLoadAssembly(CompiledAssemblyName + ".dll", buffer);
            }
            else
            {
                Trace.TraceInformation("[Kompiler]: DB Compilation Result: Bytes:{0}, Msg:{1}",
                    buffer.Length, msg);
            }
        }


        public static void AddReferences(params Type[] types)
        {
            if (_initialized)
                throw new InvalidOperationException("Compilador só pode ser executado no Pre-Start!");

            foreach (var type in types)
            {
                if (!ReferencePaths.Contains(type.Assembly.Location))
                    ReferencePaths.Add(type.Assembly.Location);
            }
        }

        public static void AddReferences(params Assembly[] assemblies)
        {

            foreach (var assembly in assemblies)
            {
                if (!ReferencePaths.Contains(assembly.Location))
                    ReferencePaths.Add(assembly.Location);
            }
        }

        //todo: passar para a classe correta
        internal static List<string> ReferencePaths = new List<string>()
        {
                    typeof (object).Assembly.Location, //System
                    typeof (Enumerable).Assembly.Location, //System.Core.dll
                    typeof(System.Data.DbType).Assembly.Location, //System.Data         
                    typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location, //Microsoft.CSharp
                    typeof(System.Web.HttpApplication).Assembly.Location,
                    typeof(System.ComponentModel.DataAnnotations.DataType).Assembly.Location,
                    typeof(DbContext).Assembly.Location,
                    typeof(CodeDomWrapper).Assembly.Location
        };
    }
}