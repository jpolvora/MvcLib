using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcFromDb.Infra
{
    public class ErrorHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.Error += ContextOnError;
        }

        private static void ContextOnError(object sender, EventArgs eventArgs)
        {
            var app = (HttpApplication) sender;
        }

        public void Dispose()
        {

        }
    }
}