using System.Data.Entity;
using System.IO;
using System.Web.WebPages;
using Newtonsoft.Json;

namespace MvcLib.Common.Mvc.ExtJs
{
    public abstract class ExtJsCrudBase<TContext, TEntity> : ExtJsBase<TContext, TEntity>
        where TContext : DbContext, new()
        where TEntity : class, new()
    {
        public TEntity Entity { get; private set; }

        protected readonly WebPageRenderingBase Page;

        protected ExtJsCrudBase(WebPageRenderingBase page)
        {
            Page = page;
            Entity = GetEntity();
        }

        protected virtual TEntity GetEntity()
        {
            var input = Page.Request.InputStream;
            input.Seek(0, SeekOrigin.Begin);
            var json = new StreamReader(input).ReadToEnd();

            var entity = JsonConvert.DeserializeObject<TEntity>(json);

            return entity;
        }

        protected virtual ExtJsResult CreateResult()
        {
            var result = new ExtJsResult(Page.Response)
            {
                data = new[] { Entity },
                success = true,
                msg = "",
                total = 1
            };

            return result;
        }

    }
}