using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;

namespace MvcLib.PluginCompiler
{
    public static class PluginStorage
    {
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        public static void Register(Assembly assembly)
        {
            if (!Assemblies.ContainsKey(assembly.FullName))
            {
                Assemblies.Add(assembly.FullName, assembly);
                BuildManager.AddReferencedAssembly(assembly);
            }
        }

        public static Assembly FindAssembly(string fullName)
        {
            if (Assemblies.ContainsKey(fullName))
                return Assemblies[fullName];

            return null;
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