using System;
using System.Collections.Generic;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public interface IDbService
    {
        /// <summary>
        /// Utilizada pelo FileExists na primeira vez
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        Tuple<bool, string, byte[]> GetFileInfo(string virtualPath);

        bool FileExistsImpl(string path);

        byte[] GetFileBytes(string path);

        string GetFileHash(string path);

        bool DirectoryExistsImpl(string path);

        IEnumerable<Tuple<string, string, byte[]>> GetChildren(string virtualPath);

        IEnumerable<VirtualFileBase> Preload();
    }
}