using System.Web.Hosting;

namespace MvcLib.CustomVPP.Impl
{
    public class DummyVirtualFile : VirtualFileBase
    {
        private readonly bool _isDir;

        public DummyVirtualFile(bool isDir)
        {
            _isDir = isDir;
        }

        public override bool IsDirectory
        {
            get { return _isDir; }
        }
    }
}