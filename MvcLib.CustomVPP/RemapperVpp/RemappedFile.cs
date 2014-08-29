using System.IO;
using System.Web.Hosting;

namespace MvcLib.CustomVPP.RemapperVpp
{
    public class RemappedFile : VirtualFile
    {
        public readonly string FullPath;

        public bool Exists { get; private set; }

        public RemappedFile(string virtualPath, string fullPath)
            : base(virtualPath)
        {
            FullPath = fullPath;
            Exists = File.Exists(fullPath);
        }

        public override Stream Open()
        {
            var inStream = new FileStream(FullPath, FileMode.Open,
                              FileAccess.Read, FileShare.ReadWrite);

            return inStream;
        }
    }
}