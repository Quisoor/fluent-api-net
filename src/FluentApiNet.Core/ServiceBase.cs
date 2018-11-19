using FluentApiNet.Abstract;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Core
{
    public class ServiceBase<TModel, TEntity, TContext>
        where TModel : class, new()
        where TEntity : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// The default page
        /// </summary>
        private const int DEFAULT_PAGE = 1;

        /// <summary>
        /// The default pagesize
        /// </summary>
        private const int DEFAULT_PAGESIZE = 25;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase{TModel, TEntity, TContext}"/> class.
        /// </summary>
        public ServiceBase()
        {
            SelectMapping = new List<Mapping>();
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        protected TContext Context { get; set; }

        /// <summary>
        /// Gets or sets the select mapping.
        /// </summary>
        /// <value>
        /// The select mapping.
        /// </value>
        protected List<Mapping> SelectMapping { get; set; }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters)
        {
            return this.Get(filters, DEFAULT_PAGE, DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page)
        {
            return this.Get(filters, page, DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel,bool>> filters, int? page, int? pageSize)
        {
            var results = new Results<TModel>();

            // format pagination
            page = LimitPage(page);
            pageSize = LimitPageSize(pageSize);

            var query = GetQuery(filters);

            // get the count
            results.Count = query.Count();

            // apply pagination
            query = query.Skip(page.Value - 1 * pageSize.Value).Take(pageSize.Value);

            // get the results
            results.Result = Transpose(query);

            return results;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        protected IQueryable<TEntity> GetQuery(Expression<Func<TModel, bool>> filters)
        {
            var translator = new TranslationVisitor(SelectMapping);
            var query = GetQuery();
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(translator.Visit(filters)));
            return query;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns></returns>
        private IQueryable<TEntity> GetQuery()
        {
            var repositoryProperty = typeof(TContext).GetProperties().Single(x => x.PropertyType.GenericTypeArguments.First() == typeof(TEntity));
            var repository = (DbSet<TEntity>)repositoryProperty.GetValue(Context);
            return repository;
        }

        private List<TModel> Transpose(IQueryable<TEntity> query)
        {
            var parameter = Expression.Parameter(typeof(TEntity));
            var ctor = Expression.New(typeof(TModel));
            var assignments = new List<MemberAssignment>();
            foreach(var map in SelectMapping)
            {
                var property = typeof(TModel).GetProperty(map.ModelMember.Member.Name);
                assignments.Add(Expression.Bind(property, map.EntityMember));

            }
            var init = Expression.MemberInit(ctor, assignments.ToArray());
            var select = Expression.Lambda<Func<TEntity, TModel>>(init, parameter);
            return query.Select(select).ToList();
        }

        /// <summary>
        /// Limits the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        private int LimitPage(int? page)
        {
            if (page.HasValue && page.Value > 0)
            {
                return page.Value;
            }
            else
            {
                return DEFAULT_PAGE;
            }
        }

        /// <summary>
        /// Limits the size of the page.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        private int LimitPageSize(int? pageSize)
        {
            if (pageSize.HasValue && pageSize.Value >= 0)
            {
                return pageSize.Value;
            }
            else
            {
                return DEFAULT_PAGESIZE;
            }
        }
    }
}
