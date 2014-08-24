using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MvcFromDb.Infra.VPP
{
    public interface IDbService
    {
        bool FileExistsImpl(string path);
        bool DirectoryExistsImpl(string path);
        byte[] GetFileBytes(string path);
        int GetDirectoryId(string path);
        string GetFileHash(string path);
        List<Tuple<string, bool>> GetChildren(int parentId);
    }
}