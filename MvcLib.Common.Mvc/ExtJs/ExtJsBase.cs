using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web;

namespace MvcLib.Common.Mvc.ExtJs
{
    public abstract class ExtJsBase<TContext, TEntity> : IDisposable
        where TContext : DbContext, new()
        where TEntity : class, new()
    {

        public TContext Context { get; private set; }

        protected ExtJsBase()
        {
            Context = new TContext();
        }

        public virtual void Dispose()
        {
            Context.Dispose();
        }

        public abstract ExtJsResult ExecuteQuery();

        public class ExtJsResult
        {
            private readonly HttpResponseBase _response;

            public ExtJsResult(HttpResponseBase response)
            {
                _response = response;
            }

            public void WriteToResponse()
            {
                _response.WriteAjax(this);
            }

            public string msg { get; set; }
            public bool success { get; set; }
            public int total { get; set; }
            public IEnumerable<TEntity> data { get; set; }
        }

    }
}