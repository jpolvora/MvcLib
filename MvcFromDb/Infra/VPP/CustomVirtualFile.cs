using System.IO;
using System.Text;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP
{
    public class CustomVirtualFile : VirtualFile
    {
        private readonly byte[] _bytes;

        public bool IsBinary { get; private set; }

        public CustomVirtualFile(string virtualPath, byte[] bytes)
            : base(virtualPath)
        {
            _bytes = bytes;
            IsBinary = true;
        }

        public CustomVirtualFile(string virtualPath, string lines)
            : base(virtualPath)
        {
            _bytes = Encoding.UTF8.GetBytes(lines);
        }

        public override Stream Open()
        {
            return new MemoryStream(_bytes ?? new byte[0]);
        }
    }
}