using System;
using System.Diagnostics;
using System.IO;
using System.Web;
using MvcLib.Common.Configuration;

namespace MvcLib.HttpModules
{
    public class PathRewriterHttpModule : IHttpModule
    {
        private static readonly string _rewriteBasePath;

        static PathRewriterHttpModule()
        {
            _rewriteBasePath = BootstrapperSection.Instance.DumpToLocal.Folder.TrimEnd('/');
            if (!_rewriteBasePath.StartsWith("~"))
                _rewriteBasePath = "~" + _rewriteBasePath;

            Trace.TraceInformation("[PathRewriterHttpModule]: '{0}'", _rewriteBasePath);
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += (s, e) => ContextOnBeginRequest(context);
        }

        private void ContextOnBeginRequest(HttpApplication application)
        {
            string path = application.Request.Url.AbsolutePath;

            if (path.StartsWith(_rewriteBasePath.Substring(1)))
                return;

            string virtualPath = string.Format("{0}{1}", _rewriteBasePath, path);

            if (string.IsNullOrWhiteSpace(VirtualPathUtility.GetFileName(virtualPath)))
            {
                return;
            }

            var physicalPath = application.Server.MapPath(virtualPath);

            if (!File.Exists(physicalPath)) return;

            string newpath = string.Format("{0}{1}", _rewriteBasePath.Substring(1), path);

            Trace.TraceInformation("[PathRewriterHttpModule]:Rewriting path from '{0}' to '{1}'", path, newpath);
            application.Context.RewritePath(newpath);
        }

        public void Dispose()
        {
        }
    }
}