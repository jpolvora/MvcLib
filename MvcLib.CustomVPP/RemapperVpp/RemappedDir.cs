using System.Collections;
using System.IO;
using System.Web;
using System.Web.Hosting;

namespace MvcLib.CustomVPP.RemapperVpp
{
    public class RemappedDir : VirtualDirectory
    {
        public readonly DirectoryInfo FullPath;
        public bool Exists { get; private set; }

        public RemappedDir(string virtualPath, string fullPath)
            : base(virtualPath)
        {
            FullPath = new DirectoryInfo(fullPath);

            Exists = FullPath.Exists;
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
                if (FullPath.Exists)
                {
                    foreach (var directoryInfo in FullPath.EnumerateDirectories())
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
                if (FullPath.Exists)
                {
                    foreach (var fileInfo in FullPath.EnumerateFiles())
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
                if (FullPath.Exists)
                {
                    foreach (var info in FullPath.EnumerateFileSystemInfos())
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