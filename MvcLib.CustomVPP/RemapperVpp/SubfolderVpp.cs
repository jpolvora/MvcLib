using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using MvcLib.Common;

namespace MvcLib.CustomVPP.RemapperVpp
{
    public class SubfolderVpp : VirtualPathProvider
    {
        private readonly string _relativePath;
        private readonly string _absolutePath;

        public SubfolderVpp()
        {
            var cfg = Config.ValueOrDefault("DumpToLocalFolder", "~/dbfiles");

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

            return IsVirtualPath(virtualDir);
        }

        public override bool FileExists(string virtualPath)
        {
            if (base.FileExists(virtualPath))
                return true;

            return IsVirtualPath(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var rem = new RemappedFile(virtualPath, GetFullPath(virtualPath));
            if (rem.Exists)
            {
                Trace.TraceInformation("[SubfolderVpp]: remapping FILE {0} to {1}", virtualPath, rem.FullPath);
                return rem;
            }

            return base.GetFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {

            var rem = new RemappedDir(virtualDir, GetFullPath(virtualDir));
            if (rem.Exists)
            {
                Trace.TraceInformation("[SubfolderVpp]: remapping DIR {0} to {1}", virtualDir, rem.FullPath.FullName);
                return rem;
            }

            return base.GetDirectory(virtualDir);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsVirtualPath(virtualPath))
                return null;

            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            if (IsVirtualPath(virtualPath))
            {
                var path = GetFullPath(virtualPath);
                if (IsFile(path))
                    return new FileInfo(virtualPath).LastAccessTimeUtc.ToString("T");
                return new DirectoryInfo(virtualPath).LastAccessTimeUtc.ToString("T");
            }

            return base.GetFileHash(virtualPath, virtualPathDependencies);
        }

        private bool IsVirtualPath(string virtualPath)
        {
            var path = GetFullPath(virtualPath);

            if (IsFile(path))
                return File.Exists(path);

            return Directory.Exists(path);
        }

        string GetFullPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(VirtualPathUtility.IsAbsolute(virtualPath)
                ? string.Format("{0}{1}", _absolutePath, virtualPath.Substring(1))
                : string.Format("{0}/{1}", _relativePath, virtualPath.Substring(2)));
        }

        private static bool IsFile(string fullPath)
        {
            return Path.HasExtension(fullPath);
        }
    }
}
