using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP
{
    public class CustomVirtualPathProvider : VirtualPathProvider
    {
        private readonly List<IFileSystemProvider> _providers = new List<IFileSystemProvider>();

        public CustomVirtualPathProvider AddImpl(IFileSystemProvider provider)
        {
            _providers.Add(provider);
            return this;
        }

        public CustomVirtualPathProvider()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            foreach (var provider in _providers)
            {
                provider.Initialize();
            }
        }

        public override bool FileExists(string virtualPath)
        {
            foreach (var provider in _providers)
            {
                if (provider.IsVirtualFile(virtualPath) && provider.FileExists(virtualPath))
                {
                    Trace.TraceInformation("[{0}]: File '{1}' found", provider.GetType().Name, virtualPath);
                    return true;
                }
            }

            return Previous.FileExists(virtualPath);
        }

        public override bool DirectoryExists(string virtualDir)
        {
            foreach (var provider in _providers)
            {
                if (provider.IsVirtualDir(virtualDir) && provider.DirectoryExists(virtualDir))
                {
                    Trace.TraceInformation("[{0}]: Directory '{1}' found", provider.GetType().Name, virtualDir);
                    return true;
                }
            }

            return Previous.DirectoryExists(virtualDir);
        }

        public override VirtualDirectory GetDirectory(string virtualDir)
        {
            foreach (var provider in _providers)
            {
                if (provider.IsVirtualDir(virtualDir) && provider.DirectoryExists(virtualDir))
                {
                    Trace.TraceInformation("[{0}]: Fetching Directory '{1}' found", provider.GetType().Name, virtualDir);
                    return provider.GetDirectory(virtualDir);
                }
            }
            return Previous.GetDirectory(virtualDir);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            foreach (var provider in _providers)
            {
                if (provider.IsVirtualFile(virtualPath) && provider.FileExists(virtualPath))
                {
                    Trace.TraceInformation("[{0}]: Fetching File '{1}'", provider.GetType().Name, virtualPath);
                    return provider.GetFile(virtualPath);
                }
            }

            return Previous.GetFile(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            foreach (var provider in _providers)
            {
                if (provider.IsVirtualFile(virtualPath) && provider.FileExists(virtualPath))
                {
                    Trace.TraceInformation("[{0}]: Fetching Hash for file '{1}'", provider.GetType().Name, virtualPath);
                    return provider.GetFileHash(virtualPath);
                }
            }
            return Previous.GetFileHash(virtualPath, virtualPathDependencies);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return _providers.Any(x => x.IsVirtualFile(virtualPath) && x.FileExists(virtualPath)) 
                ? null 
                : Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }
}