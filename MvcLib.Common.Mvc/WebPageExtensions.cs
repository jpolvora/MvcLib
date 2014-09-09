using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;
using System.Web.Mvc;

namespace MvcLib.Common.Mvc
{
    public static class WebPageExtensions
    {
        public static string FingerPrint(string rootRelativePath)
        {
            if (HttpContext.Current.Request.IsLocal)
                return rootRelativePath;

            if (HttpRuntime.Cache[rootRelativePath] == null)
            {
                string relative = VirtualPathUtility.ToAbsolute("~" + rootRelativePath);
                string absolute = HostingEnvironment.MapPath(relative);

                if (!File.Exists(absolute))
                    throw new FileNotFoundException("File not found", absolute);

                DateTime date = File.GetLastWriteTime(absolute);
                int index = relative.LastIndexOf('.');

                string result = relative.Insert(index, "_" + date.Ticks);

                HttpRuntime.Cache.Insert(rootRelativePath, result, new CacheDependency(absolute));
            }

            return HttpRuntime.Cache[rootRelativePath] as string;
        }

        public static Chunk BeginChunk(this TextWriter writer, string tag, string info, bool isSection, params string[] classes)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new ArgumentNullException("tag");

            var tagBuilder = new TagBuilder(tag);
            if (isSection)
            {
                tagBuilder.Attributes["data-section"] = info.ToLowerInvariant();
            }
            else
            {
                tagBuilder.Attributes["data-virtualpath"] = info.TrimStart('~').ToLowerInvariant();
            }


            foreach (var @class in classes)
            {
                tagBuilder.AddCssClass(@class);
            }

            return new Chunk(writer, tagBuilder);
        }

        public class Chunk : IDisposable
        {
            private readonly TextWriter _writer;
            private readonly TagBuilder _tagBuilder;

            public Chunk(TextWriter writer, TagBuilder tagBuilder)
            {
                _writer = writer;
                _tagBuilder = tagBuilder;


                if (tagBuilder == null) return;

                writer.WriteLine(Environment.NewLine + tagBuilder.ToString(TagRenderMode.StartTag));
                tagBuilder.ToString(TagRenderMode.Normal);
            }

            public void Dispose()
            {
                if (_tagBuilder == null) return;

                _writer.WriteLine(Environment.NewLine + _tagBuilder.ToString(TagRenderMode.EndTag));
            }
        }
    }
}