using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using MvcLib.Common;

namespace MvcLib.CustomVPP
{
    public class SubfolderVpp : VirtualPathProvider
    {
        private readonly string _subfolder;
        private readonly string _relativePath;
        private readonly string _absolutePath;

        public SubfolderVpp()
        {
            var cfg = Config.ValueOrDefault("DumpToLocalFolder", "~/dbfiles");

            _relativePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAppRelative(cfg));
            _absolutePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAbsolute(cfg));

            _subfolder = HostingEnvironment.MapPath(_relativePath);
            if (!Directory.Exists(_subfolder))
                Directory.CreateDirectory(_subfolder);
        }

        string NewAbsolutePath(string virtualPath)
        {
            return string.Format("{0}{1}", _absolutePath, virtualPath.Substring(1));
        }

        string NewRelativePath(string virtualPath)
        {
            return string.Format("{0}/{1}", _relativePath, virtualPath.Substring(2));
        }

        string GetPath(string virtualPath)
        {
            return HostingEnvironment.MapPath(VirtualPathUtility.IsAbsolute(virtualPath) 
                ? NewAbsolutePath(virtualPath) 
                : NewRelativePath(virtualPath));
        }

        public override bool DirectoryExists(string virtualDir)
        {
            if (base.DirectoryExists(virtualDir))
                return true;

            string fullPath = GetPath(virtualDir);

            if (Directory.Exists(fullPath))
                return true;

            return false;
        }

        public override bool FileExists(string virtualPath)
        {
            if (base.FileExists(virtualPath))
                return true;

            string fullPath = GetPath(virtualPath);

            if (File.Exists(fullPath))
                return true;

            return false;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var rem = new RemappedFile(virtualPath, GetPath(virtualPath));
            if (rem.Exists)
                return rem;

            return base.GetFile(virtualPath);

        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var rem = new RemappedDir(virtualDir, GetPath(virtualDir));
            if (rem.Exists)
                return rem;

            return base.GetDirectory(virtualDir);
        }
    }
}
