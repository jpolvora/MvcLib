using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc
{
    public class CustomWebViewPage : WebViewPage
    {
        public override void Execute()
        {

        }

        public bool IsRazorWebPage
        {
            get { return false; }
        }

        public override void ExecutePageHierarchy()
        {
            if (IsAjax || string.IsNullOrWhiteSpace(Layout))
            {
                base.ExecutePageHierarchy();
                return;
            }

            using (DisposableTimer.StartNew(GetType().Name))
            {
                using (this.BeginChunk("div", VirtualPath, "section"))
                {
                    base.ExecutePageHierarchy();
                }
            }
        }

        public HelperResult RenderSectionEx(string name, bool required = false)
        {
            using (this.BeginChunk("div", "RenderSection: " + name, "section"))
            {
                return RenderSection(name, false);
            }
        }
    }

    public class CustomWebViewPage<T> : WebViewPage<T>
    {
        public override void Execute()
        {
            //actually this is never called
            throw new NotImplementedException();
        }

        public bool IsRazorWebPage
        {
            get { return false; }
        }

        public override void ExecutePageHierarchy()
        {
            if (IsAjax || string.IsNullOrWhiteSpace(Layout))
            {
                base.ExecutePageHierarchy();
                return;
            }

            using (DisposableTimer.StartNew(GetType().Name))
            {
                using (this.BeginChunk("div", VirtualPath, "section"))
                {
                    base.ExecutePageHierarchy();
                }
            }
        }

        public HelperResult RenderSectionEx(string name, bool required = false)
        {
            using (this.BeginChunk("div", "RenderSection: " + name, "section"))
            {
                return RenderSection(name, false);
            }
        }

    }
}