using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using MvcLib.Common;

namespace MvcLib.PluginLoader
{
    public static class PluginStorage
    {
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        internal static void Register(string fileName)
        {
            if (Assemblies.ContainsKey(fileName))
                Assemblies.Remove(fileName);

            try
            {
                var loadingAssembly = Assembly.LoadFile(fileName);
                Register(loadingAssembly);
            }
            catch (Exception ex)
            {
                var msg = "ERRO LOADING ASSEMBLY: {0}: {1}".Fmt(fileName, ex);
                Trace.TraceError(msg);
            }
        }

        internal static void Register(Assembly assembly)
        {
            if (Assemblies.ContainsKey(assembly.FullName))
            {
                return;
            }

            Assemblies.Add(assembly.FullName, assembly);
            BuildManager.AddReferencedAssembly(assembly);

            Trace.TraceInformation("[PluginLoader]:Plugin registered: {0}", assembly.FullName);
        }

        internal static Assembly FindAssembly(string fullName)
        {
            return Assemblies.ContainsKey(fullName)
                ? Assemblies[fullName]
                : null;
        }

        public static IEnumerable<string> GetPluginNames()
        {
            return Assemblies.Values.Select(item => item.GetName().Name);
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            return Assemblies.Select(pair => pair.Value);
        }
    }
}