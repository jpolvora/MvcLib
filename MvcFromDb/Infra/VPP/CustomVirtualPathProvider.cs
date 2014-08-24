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

        private readonly TraceSource _source = new TraceSource("CustomVirtualPathProvider", SourceLevels.All);

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
                    _source.TraceEvent(TraceEventType.Information, 0, "FileExists {0} exists on {1}", virtualPath, provider.GetType().Name);
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
                    _source.TraceEvent(TraceEventType.Information, 0, "DirectoryExists {0} exists on {1}", virtualDir, provider.GetType().Name);
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
                    _source.TraceEvent(TraceEventType.Verbose, 0, "GetDirectory {0} on {1}", virtualDir, provider.GetType().Name);
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
                    _source.TraceEvent(TraceEventType.Verbose, 0, "GetFile {0} on {1}", virtualPath, provider.GetType().Name);
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
                    _source.TraceEvent(TraceEventType.Verbose, 0, "GetFileHash {0} on {1}", virtualPath, provider.GetType().Name);
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
                    _source.TraceEvent(TraceEventType.Verbose, 0, "GetCacheDependency {0} on {1}", virtualPath, provider.GetType().Name);
                    return null;
                }
            }
            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }
}