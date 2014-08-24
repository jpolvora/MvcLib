using System.Collections.Generic;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP
{
    public interface IVirtualPathProvider
    {
        void Initialize();
        bool IsVirtualDir(string virtualPath);
        bool IsVirtualFile(string virtualPath);

        bool FileExists(string virtualPath);
        string GetFileHash(string virtualPath);
        CustomVirtualFile GetFile(string virtualPath);
        bool DirectoryExists(string virtualDir);
        CustomVirtualDir GetDirectory(string virtualDir);
        IEnumerable<VirtualFileBase> LazyGetChildren(int key);
    }
}