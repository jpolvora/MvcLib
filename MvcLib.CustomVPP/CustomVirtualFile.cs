using System.IO;
using System.Text;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class CustomVirtualFile : VirtualFile
    {
        public byte[] Bytes { get; private set; }
        public readonly string Hash;

        public bool IsBinary { get; private set; }

        public CustomVirtualFile(string virtualPath, string hash, byte[] bytes, bool isBinary = false)
            : base(virtualPath)
        {
            Bytes = bytes;
            Hash = hash;
            IsBinary = isBinary;
        }

        public CustomVirtualFile(string virtualPath, string lines, string hash)
            : this(virtualPath, hash, Encoding.UTF8.GetBytes(lines))
        {
        }

        public override Stream Open()
        {
            return new MemoryStream(Bytes ?? new byte[0]);
        }

        public override string ToString()
        {
            return string.Format("CustomVirtualFile: {0}, {1}", VirtualPath, Hash);
        }
    }
}