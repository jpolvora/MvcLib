using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Xml.Linq;
using MvcLib.Common;
using MvcLib.DbFileSystem;

namespace MvcLib.PluginCompiler
{
    /*
     * http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust
     */

    public class PluginLoader
    {
        public const string CompiledAssemblyName = "compiledassembly";

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

        public static void Initialize(bool forceRecompile = false)
        {
            var assemblies = new Dictionary<string, byte[]>();
            using (var ctx = new DbFileContext())
            {
                var files = ctx.DbFiles
                    .Where(x => !x.IsHidden && !x.IsDirectory && x.IsBinary && x.Extension.Equals(".dll"))
                    .ToList();
                foreach (var s in files)
                {
                    if (forceRecompile && s.Name.Equals(CompiledAssemblyName))
                        continue;

                    Trace.TraceInformation("[PluginLoader]: Found assembly from Database: {0}", s.VirtualPath);
                    assemblies.Add(s.Name, s.Bytes);
                }
            }

            if (!assemblies.ContainsKey(CompiledAssemblyName))
            {
                using (DisposableTimer.StartNew("Assembly Compilation"))
                {
                    byte[] buffer;
                    var msg = KompilerDbService.TryCreateAndSaveAssemblyFromDbFiles(CompiledAssemblyName, out buffer);
                    if (string.IsNullOrWhiteSpace(msg) && buffer.Length > 0)
                    {
                        Trace.TraceInformation("[PluginLoader]: DB Compilation Result: SUCCESS");
                        assemblies.Add(CompiledAssemblyName, buffer);
                    }
                    else
                    {
                        Trace.TraceInformation("[PluginLoader]: DB Compilation Result: Bytes:{0}, Msg:{1}",
                            buffer.Length, msg);
                    }
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
                    var fullFileName = Path.Combine(PluginFolder.FullName, assembly.Key + ".dll");

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