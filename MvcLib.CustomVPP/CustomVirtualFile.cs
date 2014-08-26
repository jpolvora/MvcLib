using System.IO;
using System.Text;
using System.Web.Hosting;
using MvcLib.Common.Cache;

namespace MvcLib.CustomVPP
{
    public class CustomVirtualFile : VirtualFile, ICacheableBytes
    {

        public byte[] Bytes { get; private set; }
        public readonly string Hash;

        public bool IsBinary { get; private set; }

        public CustomVirtualFile(string virtualPath, byte[] bytes, string hash, bool isBinary = false)
            : base(virtualPath)
        {
            Bytes = bytes;
            Hash = hash;
            IsBinary = isBinary;
        }

        public CustomVirtualFile(string virtualPath, string lines, string hash)
            : this(virtualPath, Encoding.UTF8.GetBytes(lines), hash, false)
        {
        }

        public override Stream Open()
        {
            return new MemoryStream(Bytes ?? new byte[0]);
        }

        
    }
}