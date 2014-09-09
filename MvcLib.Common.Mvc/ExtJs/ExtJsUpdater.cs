using System.Data.Entity;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc.ExtJs
{
    public class ExtJsUpdater<TContext, TEntity> : ExtJsCrudBase<TContext, TEntity>
        where TContext : DbContext, new()
        where TEntity : class, new()
    {
        public ExtJsUpdater(WebPageRenderingBase page)
            : base(page)
        {
        }

        public override ExtJsResult ExecuteQuery()
        {
            Context.Entry(Entity).State = EntityState.Modified;
            Context.SaveChanges();

            return CreateResult();
        }
    }
}