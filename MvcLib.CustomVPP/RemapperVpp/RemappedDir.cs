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
                        yield return RemappedFile.CreateFromPath(this.VirtualPath, fileInfo);
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
                        var directoryInfo = info as DirectoryInfo;
                        if (directoryInfo != null)
                            yield return CreateFromPath(this.VirtualPath, directoryInfo);
                        else 
                            yield return RemappedFile.CreateFromPath(this.VirtualPath, (FileInfo)info);
                    }
                }
            }
        }

        public static RemappedDir CreateFromPath(string baseVirtualPath, DirectoryInfo directoryInfo)
        {
            var virtualDir = VirtualPathUtility.AppendTrailingSlash(baseVirtualPath) + directoryInfo.Name;
            return new RemappedDir(virtualDir, directoryInfo.FullName);
        }
    }
}