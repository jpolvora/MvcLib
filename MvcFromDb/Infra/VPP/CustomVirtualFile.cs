using System.IO;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP
{
    public class CustomVirtualFile : VirtualFile
    {
        private readonly byte[] _bytes;

        public CustomVirtualFile(string virtualPath, byte[] bytes)
            : base(virtualPath)
        {
            _bytes = bytes ?? new byte[0];
        }

        public override Stream Open()
        {
            //var buffer = Encoding.UTF8.GetPreamble().Concat(_bytes).ToArray();
            return new MemoryStream(_bytes);
        }
    }
}