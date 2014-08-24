using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP.Impl
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