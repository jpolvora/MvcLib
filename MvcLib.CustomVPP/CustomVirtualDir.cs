using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class CustomVirtualDir : VirtualDirectory
    {
        public readonly bool Loaded;
        private readonly IEnumerable<VirtualFileBase> _children;

        public CustomVirtualDir(string virtualPath, bool loaded, IEnumerable<VirtualFileBase> children)
            : base(virtualPath)
        {
            Loaded = loaded;
            _children = children;
        }

        public override IEnumerable Directories
        {
            get
            {
                var dirs = _children.OfType<CustomVirtualDir>();
                return dirs;
            }
        }

        public override IEnumerable Files
        {
            get
            {
                var files = _children.OfType<CustomVirtualFile>();
                return files;
            }
        }

        public override IEnumerable Children
        {
            get
            {
                return _children;
            }
        }

        public override string ToString()
        {
            return string.Format("CustomVirtualDir: {0}", VirtualPath);
        }
    }
}