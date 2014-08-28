using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class CustomVirtualDir : VirtualDirectory
    {
        private readonly Func<IEnumerable<VirtualFileBase>> _getChildren;

        public CustomVirtualDir(string virtualPath, Func<IEnumerable<VirtualFileBase>> getChildren)
            : base(virtualPath)
        {
            _getChildren = getChildren;
        }

        public override IEnumerable Directories
        {
            get
            {
                var dirs = _getChildren().OfType<VirtualDirectory>();
                return dirs;
            }
        }

        public override IEnumerable Files
        {
            get
            {
                var files = _getChildren().OfType<VirtualFile>();
                return files;
            }
        }

        public override IEnumerable Children
        {
            get
            {
                var all = _getChildren();
                return all;
            }
        }

        public override string ToString()
        {
            return string.Format("CustomVirtualDir: {0}", VirtualPath);
        }
    }
}