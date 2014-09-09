using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Caching;
using System.Web.Hosting;

namespace MvcLib.CustomVPP
{
    public class CustomVirtualPathProvider : VirtualPathProvider
    {
        private static readonly List<IFileSystemProvider> Providers = new List<IFileSystemProvider>();

        public static IReadOnlyList<IFileSystemProvider> GetProviders()
        {
            return new List<IFileSystemProvider>(Providers);
        }

        public CustomVirtualPathProvider AddImpl(IFileSystemProvider provider)
        {
            Providers.Add(provider);
            return this;
        }

        public CustomVirtualPathProvider()
        {
        }

        protected override void Initialize()
        {
            base.Initialize();

            foreach (var provider in Providers)
            {
                provider.Initialize();
            }
        }

        public override bool FileExists(string virtualPath)
        {
            foreach (var provider in Providers)
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
            foreach (var provider in Providers)
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
            foreach (var provider in Providers)
            {
                if (provider.IsVirtualDir(virtualDir) && provider.DirectoryExists(virtualDir))
                {
                    return provider.GetDirectory(virtualDir);
                }
            }
            return Previous.GetDirectory(virtualDir);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            foreach (var provider in Providers)
            {
                if (provider.IsVirtualFile(virtualPath) && provider.FileExists(virtualPath))
                {
                    return provider.GetFile(virtualPath);
                }
            }

            return Previous.GetFile(virtualPath);
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            foreach (var provider in Providers)
            {
                if (provider.IsVirtualFile(virtualPath) && provider.FileExists(virtualPath))
                {
                    return provider.GetFileHash(virtualPath);
                }
            }
            return Previous.GetFileHash(virtualPath, virtualPathDependencies);
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            return Providers.Any(x => x.IsVirtualFile(virtualPath) && x.FileExists(virtualPath))
                ? null
                : Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }
    }
}