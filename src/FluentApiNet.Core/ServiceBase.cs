using FluentApiNet.Abstract;
using FluentApiNet.Tools;
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
        /// The translator
        /// </summary>
        private readonly TranslationVisitor<Func<TEntity, bool>> translator;

        /// <summary>
        /// Gets or sets mapping.
        /// </summary>
        /// <value>
        /// The select mapping.
        /// </value>
        private readonly List<Mapping> mappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase{TModel, TEntity, TContext}"/> class.
        /// </summary>
        public ServiceBase()
        {
            mappings = new List<Mapping>();
            // initialize the translator
            translator = new TranslationVisitor<Func<TEntity, bool>>();
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        protected TContext Context { get; set; }

        /// <summary>
        /// Gets or sets the order by.
        /// </summary>
        /// <value>
        /// The order by.
        /// </value>
        protected MemberExpression OrderBy { get; set; }

        /// <summary>
        /// Adds the mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        protected void AddMapping(Mapping mapping)
        {
            this.mappings.Add(mapping);
            this.translator.AddMapping(mapping);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters)
        {
            return this.Get(filters, PaginationTools.DEFAULT_PAGE, PaginationTools.DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page)
        {
            return this.Get(filters, page, PaginationTools.DEFAULT_PAGESIZE);
        }

        /// <summary>
        /// Gets the specified filters.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public Results<TModel> Get(Expression<Func<TModel, bool>> filters, int? page, int? pageSize)
        {
            var results = new Results<TModel>();

            // format pagination
            page = PaginationTools.LimitPage(page);
            pageSize = PaginationTools.LimitPageSize(pageSize);

            var query = GetQuery(filters);

            // get the count
            results.Count = query.Count();

            // apply pagination
            query = query.Pagine(page.Value, pageSize.Value);

            // get the results
            results.Result = ApplySelect(query);

            return results;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        protected IQueryable<TEntity> GetQuery(Expression<Func<TModel, bool>> filters)
        {
            // get basic query of the repository
            var query = GetQuery();
            // apply the where expression to the query
            query = ApplyWhere(query, filters);
            // apply order by expression to the query
            query = ApplyOrderBy(query);
            return query;
        }

        /// <summary>
        /// Gets the query.
        /// </summary>
        /// <returns>The repository</returns>
        private IQueryable<TEntity> GetQuery()
        {
            // get repository property in the context
            var repositoryProperty = typeof(TContext).GetProperties()
                .Single(p => p.PropertyType.IsGenericType
                    && p.PropertyType.Name.StartsWith("DbSet")
                    && p.PropertyType.GetGenericArguments().Length > 0
                    && p.PropertyType.GetGenericArguments().First() == typeof(TEntity));
            // get value of the repository
            var repository = (DbSet<TEntity>)repositoryProperty.GetValue(Context);
            return repository;
        }

        /// <summary>
        /// Applies the where.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="filters">The filters.</param>
        /// <returns>Query filtered</returns>
        private IQueryable<TEntity> ApplyWhere(IQueryable<TEntity> query, Expression<Func<TModel, bool>> filters)
        {
            var where = translator.Visit(filters) as LambdaExpression;
            query = query.Where(Expression.Lambda<Func<TEntity, bool>>(where.Body, translator.EntryParameter));
            return query;
        }

        /// <summary>
        /// Applies the order by.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Query ordered</returns>
        private IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query)
        {
            // get the order by expression
            MemberExpression orderBy = null;
            if (OrderBy == null && mappings.Count >= 1)
            {
                orderBy = translator.Visit(mappings.First().ModelMember) as MemberExpression;
            }
            else if (OrderBy != null)
            {
                orderBy = translator.Visit(OrderBy) as MemberExpression;
            }

            // apply expressions
            if (orderBy != null)
            {
                var lambda = (dynamic)Expression.Lambda(orderBy, translator.EntryParameter);
                query = Queryable.OrderBy(query, lambda);
            }

            return query;
        }

        /// <summary>
        /// Applies the select.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of results</returns>
        private List<TModel> ApplySelect(IQueryable<TEntity> query)
        {
            // new TModel()
            var ctor = Expression.New(typeof(TModel));
            var assignments = new List<MemberAssignment>();
            foreach (var map in mappings)
            {
                // add assignment for the model property
                var property = typeof(TModel).GetProperty(map.ModelMember.Member.Name);
                assignments.Add(Expression.Bind(property, translator.Visit(map.ModelMember)));
            }
            // initialize the model
            var init = Expression.MemberInit(ctor, assignments.ToArray());
            // create select expression
            var select = Expression.Lambda<Func<TEntity, TModel>>(init, translator.EntryParameter);
            // apply select expression
            return query.Select(select).ToList();
        }
    }
}
