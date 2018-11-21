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
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var traductor = new TranslationVisitor<TEntity>(mappings, parameter);
            var ctor = Expression.New(typeof(TModel));
            var assignments = new List<MemberAssignment>();
            foreach (var map in mappings)
            {
                var property = typeof(TModel).GetProperty(map.ModelMember.Member.Name);                
                assignments.Add(Expression.Bind(property, traductor.Visit(map.ModelMember)));
            }
            var init = Expression.MemberInit(ctor, assignments.ToArray());
            var select = Expression.Lambda<Func<TEntity, TModel>>(init, parameter);
            return query.Select(select).ToList();
        }
    }
}
