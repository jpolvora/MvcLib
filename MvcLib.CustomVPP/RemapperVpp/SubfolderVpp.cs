using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using MvcLib.Common.Configuration;

namespace MvcLib.CustomVPP.RemapperVpp
{
    public class SubfolderVpp : VirtualPathProvider
    {
        private readonly string _relativePath;
        private readonly string _absolutePath;

        public static void SelfRegister()
        {
            var instance = new SubfolderVpp();

            Trace.TraceInformation("[SubFolderVpp]: Self registration: {0}", instance);
            HostingEnvironment.RegisterVirtualPathProvider(instance);
        }

        public SubfolderVpp()
            : this(BootstrapperSection.Instance.DumpToLocal.Folder)
        {
        }

        public SubfolderVpp(string cfg)
        {
            _relativePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAppRelative(cfg));
            _absolutePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAbsolute(cfg));

            string subfolder = HostingEnvironment.MapPath(_relativePath);
            if (!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);
        }

        public override bool DirectoryExists(string virtualDir)
        {
            if (base.DirectoryExists(virtualDir))
                return true;

            return IsVirtualPath(virtualDir, true, false);
        }

        public override bool FileExists(string virtualPath)
        {
            if (base.FileExists(virtualPath))
                return true;

            return IsVirtualPath(virtualPath, false, true);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (IsVirtualPath(virtualPath, false, true))
            {
                var rem = new RemappedFile(virtualPath, GetRemappedFullPath(virtualPath));
                if (rem.Exists)
                {
                    Trace.TraceInformation("[SubfolderVpp]: remapping FILE {0} to {1}", virtualPath, rem.FullPath);
                    return rem;
                }
            }

            return base.GetFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            if (IsVirtualPath(virtualDir, true, false))
            {
                var rem = new RemappedDir(virtualDir, GetRemappedFullPath(virtualDir));
                if (rem.Exists)
                {
                    Trace.TraceInformation("[SubfolderVpp]: remapping DIR {0} to {1}", virtualDir, rem.FullPath.FullName);
                    return rem;
                }
            }

            return base.GetDirectory(virtualDir);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsVirtualPath(virtualPath, true, true))
                return null;

            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            if (IsVirtualPath(virtualPath, true, true))
            {
                var path = GetRemappedFullPath(virtualPath);
                string hash;

                if (IsFile(path))
                {
                    var fInfo = new FileInfo(path);
                    hash = fInfo.LastWriteTimeUtc.ToString("u");
                }
                else
                {
                    var dInfo = new DirectoryInfo(path);
                    hash = dInfo.LastWriteTimeUtc.ToString("u");
                }

                Trace.TraceInformation("[SubfolderVpp]: hash for '{0}' is '{1}'", virtualPath, hash);
                return hash;
            }

            return base.GetFileHash(virtualPath, virtualPathDependencies);
        }

        private bool IsVirtualPath(string virtualPath, bool dir, bool file)
        {
            var path = GetRemappedFullPath(virtualPath);

            if (dir && IsDirectory(path))
                return true;

            if (file && IsFile(path))
                return true;

            return false;
        }

        string GetRemappedFullPath(string virtualPath)
        {
            //se o path iniciar com ~/dbfiles / cfg (app_data)
            if (virtualPath.StartsWith(_absolutePath) || virtualPath.StartsWith(_relativePath))
                return HostingEnvironment.MapPath(virtualPath);

            return HostingEnvironment.MapPath(VirtualPathUtility.IsAbsolute(virtualPath)
                ? string.Format("{0}{1}", _absolutePath, virtualPath.Substring(1))
                : string.Format("{0}/{1}", _relativePath, virtualPath.Substring(2)));
        }

        private static bool IsFile(string fullPath)
        {
            return File.Exists(fullPath);

            //var attr = File.GetAttributes(fullPath);
            //return !attr.HasFlag(FileAttributes.Directory);

        }

        private static bool IsDirectory(string fullPath)
        {
            return Directory.Exists(fullPath);

            //var attr = File.GetAttributes(fullPath);
            //return !attr.HasFlag(FileAttributes.Directory);

        }
    }
}
