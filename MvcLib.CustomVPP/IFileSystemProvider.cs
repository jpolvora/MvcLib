using System.Collections.Generic;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public interface IFileSystemProvider
    {
        /// <summary>
        /// Custom initialization
        /// </summary>
        void Initialize();

        /// <summary>
        /// Check wheter this VPP will serve desired directories
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        bool IsVirtualDir(string virtualPath);

        /// <summary>
        /// Check wheter this VPP serves desired file
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        bool IsVirtualFile(string virtualPath);

        /// <summary>
        /// Check wheter this VPP 
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        bool FileExists(string virtualPath);
        string GetFileHash(string virtualPath);
        CustomVirtualFile GetFile(string virtualPath);
        bool DirectoryExists(string virtualDir);
        CustomVirtualDir GetDirectory(string virtualDir);
        

        void RemoveFromCache(string key);
    }
}