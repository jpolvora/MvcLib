using System;
using System.Web.Mvc;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc
{
    public class CustomWebViewPage<T> : WebViewPage<T>
    {
        public override void Execute()
        {
            //actually this is never called
            throw new NotImplementedException();
        }

        public override void ExecutePageHierarchy()
        {
            if (IsAjax || string.IsNullOrEmpty(Layout))
            {
                base.ExecutePageHierarchy();
                return;
            }

            using (DisposableTimer.StartNew(GetType().Name))
            {
                using (this.BeginChunk("div", VirtualPath))
                {
                    base.ExecutePageHierarchy();
                }
            }
        }

        public HelperResult RenderSectionEx(string name, bool required = false)
        {
            using (this.BeginChunk("div", "RenderSection: " + name))
            {
                return RenderSection(name, false);
            }
        }

    }
}