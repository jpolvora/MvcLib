using System.Data.Entity;
using System.Reflection;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc.ExtJs
{
    public class ExtJsDeleter<TContext, TEntity> : ExtJsCrudBase<TContext, TEntity>
        where TContext : DbContext, new()
        where TEntity : class, new()
    {
        private readonly bool _findFirst;
        private readonly PropertyInfo _id;

        public ExtJsDeleter(WebPageRenderingBase page, bool findFirst = false, string keyProperty = "Id")
            : base(page)
        {
            _findFirst = findFirst;
            if (findFirst)
            {
                var pi = typeof(TEntity).GetProperty(keyProperty);
                _id = pi;
            }
        }

        public override ExtJsResult ExecuteQuery()
        {
            if (_findFirst)
            {
                var key = _id.GetValue(Entity, null);

                var toDelete = Context.Set<TEntity>().Find(key);
                if (toDelete != null)
                {
                    Context.Set<TEntity>().Remove(toDelete);
                }
            }
            else
            {
                Context.Entry(Entity).State = EntityState.Deleted;
            }

            Context.SaveChanges();

            return CreateResult();
        }
    }
}