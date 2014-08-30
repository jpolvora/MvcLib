using System;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc
{
    /// <summary>
    /// Para WebPages
    /// </summary>
    public class CustomPageBase : WebPage
    {
        public override void Execute()
        {
            //actually this is never called
            throw new NotImplementedException();
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