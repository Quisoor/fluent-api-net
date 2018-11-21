using FluentApiNet.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Tools
{
    /// <summary>
    /// Query tools class
    /// </summary>
    public static class QueryTools
    {
        /// <summary>
        /// Transposes the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>List of model</returns>
        public static List<TModel> Transpose<TModel, TEntity>(IQueryable<TEntity> query, List<Mapping> mappings)
        {
            // define principal parameter
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            // initialize translator
            var translator = new TranslationVisitor<TEntity>(mappings, parameter);
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
            var select = Expression.Lambda<Func<TEntity, TModel>>(init, parameter);
            // apply select expression
            return query.Select(select).ToList();
        }

        /// <summary>
        /// Pagines the specified query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public static IQueryable<TEntity> Pagine<TEntity>(this IQueryable<TEntity> query, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var take = pageSize;
            return query.Skip(skip).Take(take);
        }
    }
}
