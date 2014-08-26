using System.Collections;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class RemappedVirtualDir : VirtualDirectory
    {
        private readonly VirtualDirectory _inner;

        public RemappedVirtualDir(string virtualPath, VirtualDirectory inner)
            : base(virtualPath)
        {
            _inner = inner;
        }

        public override IEnumerable Directories
        {
            get { return _inner.Directories; }
        }

        public override IEnumerable Files
        {
            get { return _inner.Files; }
        }

        public override IEnumerable Children
        {
            get { return _inner.Children; }
        }
    }
}