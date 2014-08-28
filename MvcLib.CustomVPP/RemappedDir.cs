using System.Collections;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class RemappedDir : VirtualDirectory
    {
        private readonly DirectoryInfo _fullPath;
        public bool Exists { get; private set; }

        public RemappedDir(string virtualPath, string fullPath)
            : base(virtualPath)
        {
            _fullPath = new DirectoryInfo(fullPath);

            Exists = _fullPath.Exists;
        }

        static RemappedDir CreateFromPath(string baseVirtualPath, DirectoryInfo directoryInfo)
        {
            var virtualDir = VirtualPathUtility.AppendTrailingSlash(baseVirtualPath) + directoryInfo.Name;
            return new RemappedDir(virtualDir, directoryInfo.FullName);
        }

        static RemappedFile CreateFromPath(string baseVirtualPath, FileInfo fileInfo)
        {
            var virtualPath = VirtualPathUtility.AppendTrailingSlash(baseVirtualPath) + fileInfo.Name;
            return new RemappedFile(virtualPath, fileInfo.FullName);
        }

        public override IEnumerable Directories
        {
            get
            {
                if (_fullPath.Exists)
                {
                    foreach (var directoryInfo in _fullPath.EnumerateDirectories())
                    {
                        yield return CreateFromPath(this.VirtualPath, directoryInfo);
                    }
                }
            }
        }

        public override IEnumerable Files
        {
            get
            {
                if (_fullPath.Exists)
                {
                    foreach (var fileInfo in _fullPath.EnumerateFiles())
                    {
                        yield return CreateFromPath(this.VirtualPath, fileInfo);
                    }
                }
            }
        }

        public override IEnumerable Children
        {
            get
            {
                if (_fullPath.Exists)
                {
                    foreach (var info in _fullPath.EnumerateFileSystemInfos())
                    {
                        if (info is DirectoryInfo)
                            yield return CreateFromPath(this.VirtualPath, (DirectoryInfo)info);
                        else if (info is FileInfo)
                            yield return CreateFromPath(this.VirtualPath, (FileInfo)info);
                    }
                }
            }
        }
    }
}