using System;
using System.Collections.Generic;

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
        bool DirectoryExistsImpl(string path);
        byte[] GetFileBytes(string path);
        int GetDirectoryId(string path);
        string GetFileHash(string path);
        IEnumerable<Tuple<string, bool>> GetChildren(int parentId);
    }
}