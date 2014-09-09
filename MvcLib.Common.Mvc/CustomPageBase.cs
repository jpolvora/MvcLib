using System;
using System.IO;
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

        public bool IsRazorWebPage
        {
            get { return true; }
        }


        public override void ExecutePageHierarchy()
        {
            if (IsAjax)
            {
                base.ExecutePageHierarchy();
                return;
            }

            using (DisposableTimer.StartNew("CustomPageBase: " + this.VirtualPath))
            {
                using (Output.BeginChunk("div", VirtualPath, false, "page"))
                {
                    base.ExecutePageHierarchy();
                }
            }
        }

        public HelperResult RenderSectionEx(string name, bool required = false)
        {
            if (IsAjax)
            {
                return RenderSection(name, required);
            }

            var result = RenderSection(name, required);

            //encapsula o resultado da section num novo resultado
            return new HelperResult(writer =>
            {
                using (writer.BeginChunk("div", name, true, "section"))
                {
                    result.WriteTo(writer);
                }
            });
        }
    }
}