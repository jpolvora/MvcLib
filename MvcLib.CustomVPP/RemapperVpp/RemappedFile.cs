using System.IO;
using System.Web.Hosting;

namespace MvcLib.CustomVPP.RemapperVpp
{
    public class RemappedFile : VirtualFile
    {
        private readonly string _fullPath;

        public bool Exists { get; private set; }

        public RemappedFile(string virtualPath, string fullPath)
            : base(virtualPath)
        {
            _fullPath = fullPath;
            Exists = File.Exists(fullPath);
        }

        public override Stream Open()
        {
            var inStream = new FileStream(_fullPath, FileMode.Open,
                              FileAccess.Read, FileShare.ReadWrite);

            return inStream;
        }
    }
}