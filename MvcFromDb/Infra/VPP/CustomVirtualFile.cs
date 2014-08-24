using System.IO;
using System.Text;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP
{
    public class CustomVirtualFile : VirtualFile
    {
        private readonly byte[] _bytes;
        public readonly string Hash;

        public bool IsBinary { get; private set; }

        public CustomVirtualFile(string virtualPath, byte[] bytes, string hash, bool isBinary = false)
            : base(virtualPath)
        {
            _bytes = bytes;
            Hash = hash;
            IsBinary = true;
        }

        public CustomVirtualFile(string virtualPath, string lines, string hash)
            : this(virtualPath, Encoding.UTF8.GetBytes(lines), hash, false)
        {
        }

        public override Stream Open()
        {
            return new MemoryStream(_bytes ?? new byte[0]);
        }
    }
}