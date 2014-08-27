using System;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc
{
    public static class WebPageExtensions
    {
        public static Chunk BeginChunk(this WebPageBase page, string tag, string info, params string[] classes)
        {
            TagBuilder tagBuilder = null;
            if (!string.IsNullOrEmpty(tag))
            {
                tagBuilder = new TagBuilder(tag);
                tagBuilder.Attributes["data-virtualpath"] = VirtualPathUtility.ToAbsolute(page.VirtualPath);

                foreach (var @class in classes)
                {
                    tagBuilder.AddCssClass(@class);
                }
            }
            return new Chunk(page, tagBuilder, info);
        }



        public class Chunk : IDisposable
        {
            private readonly WebPageBase _page;
            private readonly TagBuilder _tagBuilder;
            private readonly string _info;

            public Chunk(WebPageBase page, TagBuilder tagBuilder, string info)
            {
                _page = page;
                _tagBuilder = tagBuilder;
                _info = info;


                if (tagBuilder == null) return;

                
                if (Config.IsInDebugMode)
                    page.Output.WriteLine("<!-- BEGIN {0} -->", page.VirtualPath);

                page.Output.WriteLine(Environment.NewLine + tagBuilder.ToString(TagRenderMode.StartTag));
                tagBuilder.ToString(TagRenderMode.Normal);
            }

            public void Dispose()
            {
                if (_tagBuilder == null) return;

                _page.Output.WriteLine(Environment.NewLine + _tagBuilder.ToString(TagRenderMode.EndTag));

                if (Config.IsInDebugMode)
                    _page.Output.WriteLine("<!-- END {0} -->", _page.VirtualPath);
            }
        }
    }
}