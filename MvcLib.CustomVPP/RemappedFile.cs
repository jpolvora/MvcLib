using System.IO;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
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
            return File.Open(_fullPath, FileMode.Open);
        }
    }
}