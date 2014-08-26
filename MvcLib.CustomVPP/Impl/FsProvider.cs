using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace MvcLib.CustomVPP.Impl
{
    public class FsProvider : AbstractFileSystemProvider
    {
        private readonly string _root;
        public FsProvider()
        {
            var root = HostingEnvironment.MapPath("~/_files");
            var info = new DirectoryInfo(root);

            if (!info.Exists)
                info.Create();

            _root = info.FullName;
        }

        private static string AppendAndNormalize(string path)
        {
            //if (VirtualPathUtility.IsAbsolute(path))
            //    path = VirtualPathUtility.ToAppRelative(path);
            
            //var fullPath = HostingEnvironment.MapPath(path);

            //var dir = Path.GetDirectoryName(fullPath);

            //var newDir = Path.Combine(dir, "_files");

            //return NormalizeFilePath("/_files" + path);
            return NormalizeFilePath(path);
        }
        public override bool FileExists(string virtualPath)
        {
            var path = VirtualPathUtility.Combine(_root, virtualPath);
            return FileExists(path);
        }

        public override string GetFileHash(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public override CustomVirtualFile GetFile(string virtualPath)
        {
            throw new NotImplementedException();
        }

        public override bool DirectoryExists(string virtualDir)
        {
            throw new NotImplementedException();
        }

        public override CustomVirtualDir GetDirectory(string virtualDir)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<VirtualFileBase> LazyGetChildren(int key)
        {
            throw new NotImplementedException();
        }
    }
}