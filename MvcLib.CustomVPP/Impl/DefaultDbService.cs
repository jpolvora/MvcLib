using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MvcLib.DbFileSystem;

namespace MvcLib.CustomVPP.Impl
{
    public class DefaultDbService : IDbService
    {
        public bool FileExistsImpl(string path)
        {
            using (var ctx = new DbFileContext())
            {
                var result = ctx.DbFiles.Any(x => x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase) && !x.IsHidden &&
                                                  !x.IsDirectory);

                Trace.TraceInformation("[DefaultDbService]:FileExistsImpl('{0}') = {1}", path, result);
                return result;
            }
        }

        public bool DirectoryExistsImpl(string path)
        {
            using (var ctx = new DbFileContext())
            {
                var result = ctx.DbFiles.Any(x =>
                    x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase) && !x.IsHidden &&
                    x.IsDirectory);

                Trace.TraceInformation("[DefaultDbService]:DirectoryExistsImpl('{0}') = {1}", path, result);
                return result;
            }
        }

        public byte[] GetFileBytes(string path)
        {
            using (var ctx = new DbFileContext())
            {
                var str = ctx.DbFiles
                    .Where(x =>
                        x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase) &&
                        !x.IsHidden && !x.IsDirectory)
                    .Select(s => s.Texto)
                    .FirstOrDefault() ?? string.Empty;

                Trace.TraceInformation("[DefaultDbService]:GetFileBytes('{0}') = {1} length", path, str.Length);
                return Encoding.UTF8.GetBytes(str);
            }
        }

        public int GetDirectoryId(string path)
        {
            using (var ctx = new DbFileContext())
            {
                var id = ctx.DbFiles
                    .Where(x =>
                        x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase) && !x.IsHidden && x.IsDirectory)
                    .Select(s => s.Id)
                    .FirstOrDefault();

                Trace.TraceInformation("[DefaultDbService]:GetDirectoryId('{0}') = {1}", path, id);
                return id;
            }
        }

        public string GetFileHash(string path)
        {
            using (var ctx = new DbFileContext())
            {
                string result = null;

                var file =
                    ctx.DbFiles.Where(
                        x =>
                            x.VirtualPath.Equals(path, StringComparison.InvariantCultureIgnoreCase) && !x.IsHidden &&
                            !x.IsDirectory)
                        .Select(s => new { s.Created, s.Modified })
                        .FirstOrDefault();

                if (file != null)
                {
                    result = file.Modified.HasValue
                        ? file.Modified.Value.ToUniversalTime().ToString("T")
                        : file.Created.ToUniversalTime().ToString("T");
                }

                Trace.TraceInformation("[DefaultDbService]:GetFileHash('{0}') = '{1}'", path, result);

                return result;
            }
        }

        public IEnumerable<Tuple<string, bool>> GetChildren(int parentId)
        {
            using (var ctx = new DbFileContext())
            {
                var child = ctx.DbFiles
                    .Where(x => x.Id == parentId)
                    .SelectMany(s => s.Children.Select(t => new {t.VirtualPath, t.IsDirectory}))
                    .ToList()
                    .Select(s => new Tuple<string, bool>(s.VirtualPath, s.IsDirectory))
                    .ToList();

                Trace.TraceInformation("[DefaultDbService]:GetChildren('{0}') = {1}", parentId, child.Count);
                return child;
            }
        }
    }
}