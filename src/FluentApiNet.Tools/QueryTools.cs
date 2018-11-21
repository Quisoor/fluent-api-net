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
    }
}
