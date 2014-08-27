using System;
using System.Collections.Generic;

namespace MvcLib.CustomVPP
{
    public interface IDbService
    {
        bool FileExistsImpl(string path);
        bool DirectoryExistsImpl(string path);
        byte[] GetFileBytes(string path);
        int GetDirectoryId(string path);
        string GetFileHash(string path);
        IEnumerable<Tuple<string, bool>> GetChildren(int parentId);
    }
}