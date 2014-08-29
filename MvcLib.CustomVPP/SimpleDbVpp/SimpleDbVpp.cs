using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using MvcLib.Common;
using MvcLib.CustomVPP.RemapperVpp;
using MvcLib.DbFileSystem;

namespace MvcLib.CustomVPP.SimpleDbVpp
{
    public class SimpleDbVpp : VirtualPathProvider
    {
        private readonly string _relativePath;
        private readonly string _absolutePath;

        public DbFileContext Db { get; private set; }

        public SimpleDbVpp()
        {
            var cfg = Config.ValueOrDefault("SimpleDbVppLocalCache", "~/db");

            _relativePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAppRelative(cfg));
            _absolutePath = VirtualPathUtility.AppendTrailingSlash(VirtualPathUtility.ToAbsolute(cfg));

            string subfolder = HostingEnvironment.MapPath(_relativePath);
            if (!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);

            Db = new DbFileContext();
        }

        public override bool DirectoryExists(string virtualDir)
        {
            if (base.DirectoryExists(virtualDir))
                return true;

            return IsVirtualPath(virtualDir);
        }

        public override bool FileExists(string virtualPath)
        {
            if (base.FileExists(virtualPath))
                return true;

            return IsVirtualPath(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            var rem = new RemappedFile(virtualPath, GetFullPath(virtualPath));
            if (rem.Exists)
            {
                Trace.TraceInformation("[SubfolderVpp]: remapping FILE {0} to {1}", virtualPath, rem.FullPath);
                return rem;
            }

            return base.GetFile(virtualPath);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            var rem = new RemappedDir(virtualDir, GetFullPath(virtualDir));
            if (rem.Exists)
            {
                Trace.TraceInformation("[SubfolderVpp]: remapping DIR {0} to {1}", virtualDir, rem.FullPath.FullName);
                return rem;
            }

            return base.GetDirectory(virtualDir);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            if (IsVirtualPath(virtualPath))
                return null;

            return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            if (IsVirtualPath(virtualPath))
            {
                var path = GetFullPath(virtualPath);
                string hash;

                if (IsFile(path))
                {
                    var fInfo = new FileInfo(path);
                    hash = fInfo.LastWriteTimeUtc.ToString("u");
                }
                else
                {
                    var dInfo = new DirectoryInfo(path);
                    hash = dInfo.LastWriteTimeUtc.ToString("u");
                }

                Trace.TraceInformation("[SubfolderVpp]: hash for '{0}' is '{1}'", virtualPath, hash);
                return hash;
            }

            return base.GetFileHash(virtualPath, virtualPathDependencies);
        }

        private bool IsVirtualPath(string virtualPath)
        {
            var path = GetFullPath(virtualPath);

            if (IsFile(path))
                return true;

            return Directory.Exists(path);
        }

        string GetFullPath(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath).ToLowerInvariant();
        }

        private bool IsFile(string fullPath)
        {
            var func = new Func<DbFile, bool>(x => !x.IsDirectory && !x.IsHidden && x.VirtualPath.Equals(fullPath, StringComparison.OrdinalIgnoreCase));

            var cache = CheckCache(func);

            if (cache != null)
                return true;

            return CheckCache(func) != null;
        }

        #region cache

        private DbFile CheckCache(Func<DbFile, bool> predicate)
        {
            return Db.DbFiles.Local.Where(predicate).FirstOrDefault();
        }

        private DbFile CheckDb(Func<DbFile, bool> predicate)
        {
            return Db.DbFiles.Where(predicate).FirstOrDefault();
        }


        #endregion
    }

    public enum CacheEntryKind
    {
        None,
        File,
        Directory
    }

    public class CacheEntry
    {
        public bool Exists { get; set; }
        public string Hash { get; set; }
        public VirtualFileBase VirtualFile { get; set; }
        public CacheEntryKind Kind { get; set; }
    }

    public class SimpleCache
    {
        readonly ConcurrentDictionary<string, bool> _entries
            = new ConcurrentDictionary<string, bool>();

        public CacheEntry Get(string key)
        {
            //bool hasEntry;
            //CacheEntry result;
            //if (_entries.TryGetValue(key, out hasEntry))
            //    if (hasEntry)
            //    {
            //        //verifica se não expirou
            //        result = WebC
            //    }

            //return entry;

            return new CacheEntry();
        }

        public void Set(string key, CacheEntry value)
        {
            //_entries.AddOrUpdate(key, s => value, (s, entry) => value);
        }

        public void Remove(string key)
        {
            //CacheEntry entry;
            //_entries.TryRemove(key, out entry);
        }
    }
}