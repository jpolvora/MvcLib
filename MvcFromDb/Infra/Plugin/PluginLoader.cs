using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Xml.Linq;
using MvcFromDb.Infra.Entities;
using MvcFromDb.Infra.Misc;

namespace MvcFromDb.Infra.Plugin
{
    /*
     * http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust
     */

    internal class PluginLoader
    {
        public const string CompiledAssemblyName = "compiledassembly.dll";

        public static readonly DirectoryInfo PluginFolder;

        static PluginLoader()
        {
            //determinar probingPath

            var privatePath = "~/_plugins";

            try
            {
                var configFile = XElement.Load(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                var probingElement = configFile.Descendants("runtime")
                    .SelectMany(runtime => runtime.Elements(XName.Get("probing")))
                    .FirstOrDefault();

                if (probingElement != null)
                {
                    privatePath = probingElement.Attribute("privatePath").Value;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Error reading probing privatePath in web.config. {0}", ex.Message);
            }

            PluginFolder = new DirectoryInfo(HostingEnvironment.MapPath(privatePath));
        }

        public static void Initialize()
        {
            var assemblies = new Dictionary<string, byte[]>();
            using (var ctx = new DbFileContext())
            {
                var files = ctx.DbFiles
                    .Where(x => !x.IsHidden && !x.IsDirectory && x.IsBinary && x.Extension.Equals(".dll"))
                    .ToList();
                foreach (var s in files)
                {
                    assemblies.Add(string.Format("{0}{1}", s.Name, s.Extension), s.Bytes);
                }
            }

            if (!assemblies.ContainsKey(CompiledAssemblyName))
            {
                try
                {
                    //compile dbfiles and get assembly to load
                    var dict = Kompiler.LoadSourceCodeFromDb();
                    if (dict.Count > 0) //somente se houver arquivos .cs
                    {
                        byte[] buffer;
                        string result = Kompiler.CreateSolutionAndCompile(dict, out buffer);
                        if (string.IsNullOrEmpty(result))
                        {
                            assemblies.Add(CompiledAssemblyName, buffer);
                            Kompiler.SaveAssemblyToDataBase(CompiledAssemblyName, buffer);
                        }
                        else
                        {
                            Trace.TraceError("Erro durante a compilação do projeto no banco de dados. \r\n" + result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message, ex);
                }
            }

            LoadPlugins(assemblies);
        }

        public static void LoadPlugins(IEnumerable<KeyValuePair<string, byte[]>> assemblies)
        {
            try
            {
                if (!PluginFolder.Exists)
                {
                    PluginFolder.Create();
                }

                foreach (var fileInfo in PluginFolder.EnumerateFiles("*.dll"))
                {
                    fileInfo.Delete();
                }

                foreach (var assembly in assemblies)
                {
                    var fullFileName = Path.Combine(PluginFolder.FullName, assembly.Key);

                    File.WriteAllBytes(fullFileName, assembly.Value);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }


            foreach (var file in PluginFolder.GetFiles("*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    var loadingAssembly = Assembly.LoadFile(file.FullName);
                }
                catch (Exception ex)
                {
                    var msg = "ERRO LOADING ASSEMBLY: {0}: {1}".Fmt(file.FullName, ex);
                    Trace.TraceError(msg);
                }
            }
        }

        //public static void RecursiveDeleteDirectory(DirectoryInfo baseDir, bool self, params string[] extensions)
        //{
        //    if (!baseDir.Exists)
        //        return;

        //    foreach (var directoryInfo in baseDir.EnumerateDirectories())
        //    {
        //        RecursiveDeleteDirectory(directoryInfo, true, extensions);
        //    }

        //    if (self && baseDir.Exists)
        //        baseDir.Delete(true);
        //}

        //public static bool IsFileLocked(string path)
        //{
        //    if (!File.Exists(path))
        //        return false;

        //    FileStream file = null;
        //    try
        //    {
        //        file = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        //    }
        //    catch (Exception)
        //    {
        //        return true;
        //    }
        //    finally
        //    {
        //        if (file != null)
        //            file.Close();
        //    }

        //    return false;
        //}
    }
}