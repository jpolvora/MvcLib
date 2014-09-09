using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public abstract class AbstractFileSystemProvider : IFileSystemProvider
    {
        public virtual void Initialize()
        {
            Trace.TraceInformation("{0} Initialized ", this);
        }

        public readonly string[] _allowedExtensions = { ".cshtml", ".js", ".css", ".xml", ".config" };
        public readonly string[] _ignoredFiles = { "precompiledapp.config" };
        public readonly string[] _ignoredDirectories = { "/bundles", "/app_localresources", "/app_browsers" };

        protected static string NormalizeFilePath(string virtualPath)
        {
            var absolute = VirtualPathUtility.ToAbsolute(virtualPath);

            var result = VirtualPathUtility.RemoveTrailingSlash(absolute);

            return result.ToLowerInvariant();
        }

        public virtual bool IsVirtualDir(string virtualPath)
        {
            /*
           * Se o path possuir extension, ignora.            
           * Se o path for reservado, ignora.
           */
            try
            {
                var extension = VirtualPathUtility.GetExtension(virtualPath);
                if (!string.IsNullOrEmpty(extension))
                    return false;

                var path = VirtualPathUtility
                    .RemoveTrailingSlash(VirtualPathUtility.ToAbsolute(virtualPath)
                    .ToLowerInvariant());

                if (!string.IsNullOrEmpty(path))
                {
                    if (_ignoredDirectories.Any(path.Contains))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceInformation("IsVirtualDir", ex);
            }

            return false;
        }

        public bool IsVirtualFile(string virtualPath)
        {
            /*
          * Se o path n�o possir extension, ignora.
          * Se a extens�o n�o for permitida, ignora.
          * Se o path for reservado, ignora.
          * Se o diretório pai possuir extension, ignora
          */
            try
            {
                var extension = VirtualPathUtility.GetExtension(virtualPath);
                if (string.IsNullOrEmpty(extension))
                    return false;

                var directory = VirtualPathUtility.ToAbsolute(VirtualPathUtility.GetDirectory(virtualPath));

                var dirHasExtension = !string.IsNullOrEmpty(VirtualPathUtility.GetExtension(directory));
                if (dirHasExtension)
                    return false;

                var fileName = VirtualPathUtility.GetFileName(virtualPath);

                if (string.IsNullOrEmpty(fileName))
                    return false;

                if (_ignoredFiles.Any(x => x.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)))
                    return false;

                if (!_allowedExtensions.Any(x => x.Equals(extension, StringComparison.InvariantCultureIgnoreCase)))
                    return false;

                if (_ignoredDirectories.Any(directory.Contains))
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                Trace.TraceInformation("IsVirtualFile", ex);
            }

            return false;
            //return virtualPath.StartsWith(_virtualRootPath);
        }

        public abstract bool FileExists(string virtualPath);

        public abstract string GetFileHash(string virtualPath);

        public abstract CustomVirtualFile GetFile(string virtualPath);

        public abstract bool DirectoryExists(string virtualDir);

        public abstract CustomVirtualDir GetDirectory(string virtualDir);

        public abstract void RemoveFromCache(string key);
    }
}