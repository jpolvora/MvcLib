using System.IO;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class RemappedVirtualFile : VirtualFile
    {
        private readonly VirtualFile _inner;

        public RemappedVirtualFile(string virtualPath, VirtualFile inner)
            : base(virtualPath)
        {
            _inner = inner;
        }

        public override Stream Open()
        {
            return _inner.Open();
        }
    }
}