using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class RemapVpp : VirtualPathProvider
    {
        public RemapVpp()
        {
            Trace.TraceInformation("[DbToLocal]: Starting...");

            var root = HostingEnvironment.MapPath("~/_files");
            DirectoryInfo dirInfo = new DirectoryInfo(root);
            if (!dirInfo.Exists)
                dirInfo.Create();

        }

        string RemapVirtualPath(string virtualPath)
        {
            var path = VirtualPathUtility.ToAbsolute(virtualPath);

            var localpath = "/_files/" + path;

            return localpath;
        }

        public override bool DirectoryExists(string virtualDir)
        {
            var remapped = RemapVirtualPath(virtualDir);
            return base.DirectoryExists(remapped);
        }

        public override bool FileExists(string virtualPath)
        {
            var remapped = RemapVirtualPath(virtualPath);
            return base.FileExists(remapped);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var remapped = RemapVirtualPath(virtualPath);

            var vf = base.GetFile(remapped);

            
            return new RemappedVirtualFile(virtualPath, vf);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var remapped = RemapVirtualPath(virtualDir);
            var vd = base.GetDirectory(remapped);
            return new RemappedVirtualDir(virtualDir, vd);
        }
    }
}