using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Caching;
using System.Web.Hosting;

namespace MvcFromDb.Infra.VPP
{

    public class CustomVirtualPathProvider : VirtualPathProvider
    {
        private readonly List<IVirtualPathProvider> _providers = new List<IVirtualPathProvider>();

        public CustomVirtualPathProvider AddImpl(IVirtualPathProvider provider)
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
                    Trace.TraceInformation("File '{0}' found at {1}", virtualPath, provider.GetType().Name);
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
                    Trace.TraceInformation("Directory '{0}' fount at {1}", virtualDir, provider.GetType().Name);
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
                    Trace.TraceInformation("Fetching directory '{0}' from {1}", virtualDir, provider.GetType().Name);
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
                    Trace.TraceInformation("Fetching file '{0}' from {1}", virtualPath, provider.GetType().Name);
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
                    Trace.TraceInformation("Getting hash for file '{0}' from {1}", virtualPath, provider.GetType().Name);
                    return provider.GetFileHash(virtualPath);
                }
            }
            return Previous.GetFileHash(virtualPath, virtualPathDependencies);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            foreach (var provider in _providers)
            {
                if (provider.IsVirtualFile(virtualPath) && provider.FileExists(virtualPath))
                {
                    return null;
                }
            }
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }
}