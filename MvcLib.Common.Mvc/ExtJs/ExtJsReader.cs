using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Helpers;
using System.Web.WebPages;

namespace MvcLib.Common.Mvc.ExtJs
{
    public class ExtJsReader<TContext, TEntity> : ExtJsBase<TContext, TEntity>
        where TContext : DbContext, new()
        where TEntity : class, new()
    {
        private readonly WebPageRenderingBase _page;
        private readonly IQueryable<TEntity> _query;

        private readonly Dictionary<string, Expression<Func<TEntity, object>>> _filters =
            new Dictionary<string, Expression<Func<TEntity, object>>>();

        private readonly Dictionary<string, Expression<Func<TEntity, object>>> _sorters =
            new Dictionary<string, Expression<Func<TEntity, object>>>();

        private readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _orderByExpression;

        public ExtJsReader(WebPageRenderingBase page, Func<IQueryable<TEntity>, IQueryable<TEntity>> orderByExpression)
        {
            _page = page;
            _query = Context.Set<TEntity>();
            _orderByExpression = orderByExpression;
        }

        /// <summary>
        /// immutable constructor
        /// </summary>
        /// <param name="previous"></param>
        /// <param name="query"></param>
        private ExtJsReader(ExtJsReader<TContext, TEntity> previous, IQueryable<TEntity> query)
        {
            _query = query;
            _page = previous._page;
            _orderByExpression = previous._orderByExpression;
            previous._filters.ForEach(x => _filters.Add(x.Key, x.Value));
            previous._sorters.ForEach(x => _sorters.Add(x.Key, x.Value));
        }

        public ExtJsReader<TContext, TEntity> Include(Expression<Func<TEntity, object>> expression)
        {
            return new ExtJsReader<TContext, TEntity>(this, _query.Include(expression));
        }

        public ExtJsReader<TContext, TEntity> CanSortBy(Expression<Func<TEntity, object>> expression)
        {
            string key = Lambda.ExtractMemberName(expression);

            return CanSortBy(key, expression);
        }

        public ExtJsReader<TContext, TEntity> CanSortBy(string key, Expression<Func<TEntity, object>> expression)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (expression == null)
                throw new ArgumentNullException("expression");

            _sorters[key] = expression;

            return new ExtJsReader<TContext, TEntity>(this, _query);
        }

        public ExtJsReader<TContext, TEntity> CanFilterBy(Expression<Func<TEntity, object>> expression)
        {
            string key = Lambda.ExtractMemberName(expression);

            return CanFilterBy(key, expression);
        }

        public ExtJsReader<TContext, TEntity> CanFilterBy(string key, Expression<Func<TEntity, object>> expression)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (expression == null)
                throw new ArgumentNullException("expression");

            _filters[key] = expression;
            return new ExtJsReader<TContext, TEntity>(this, _query);
        }

        public ExtJsReader<TContext, TEntity> CanFilterByCustom(Expression<Func<TEntity, object>> leftExpression, Expression<Func<TEntity, object>> rightExpression)
        {
            string key = Lambda.ExtractMemberName(leftExpression);

            return CanFilterBy(key, rightExpression);
        }


        public override ExtJsResult ExecuteQuery()
        {
            var query = _query;

            var filterFn = _page.Server.UrlDecode(_page.Request.Unvalidated["filter"]);
            if (!filterFn.IsEmpty())
            {
                var filtersArray = Json.Decode(filterFn);
                foreach (var filter in filtersArray)
                {
                    string key = filter.property ?? filter.field;
                    if (string.IsNullOrEmpty(key) || !_filters.ContainsKey(key))
                        continue;

                    var tempExpr = _filters[key];
                    var resExpr = Lambda.ModifyExpression(tempExpr, (object)filter.value);

                    query = query.Where(resExpr);
                }
            }

            var isOrdered = false;

            var sortFn = _page.Server.UrlDecode(_page.Request.Unvalidated["sort"]);
            if (!sortFn.IsEmpty())
            {
                var sortersArray = Json.Decode(sortFn);
                foreach (var sort in sortersArray)
                {
                    string key = sort.property;
                    if (!string.IsNullOrEmpty(key) && _sorters.ContainsKey(key))
                    {
                        var expr = _sorters[key];
                        if (isOrdered)
                        {
                            var ordered = query as IOrderedQueryable<TEntity>;
                            query = ordered.ThenBy(expr);
                        }
                        else
                        {
                            query = query.OrderBy(expr);
                        }

                        isOrdered = true;
                    }
                }
            }

            if (isOrdered)
            {
                //var ordered = (IOrderedQueryable<TEntity>)query;
                //query = ordered.ThenBy(_setOrderBy(ordered));
            }
            else
            {
                query = _orderByExpression(query);
            }

            var total = query.Count();

            if (!_page.Request["limit"].IsEmpty())
            {
                var start = _page.Request["start"].AsInt(0);
                if (start > total)
                    start = 0;

                var limit = _page.Request["limit"].AsInt(25);

                query = query.Skip(start).Take(limit);
            }

            var result = new ExtJsResult(_page.Response)
            {
                msg = "",
                success = true,
                total = total,
                data = query.ToList()
            };

            return result;
        }
    }
}