﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Abstract
{
    /// <summary>
    /// Translation visitor class
    /// </summary>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
    public class TranslationVisitor<TEntry> : ExpressionVisitor
    {
        /// <summary>
        /// The default parameter name
        /// </summary>
        public const string DEFAULT_PARAMETER_NAME = "traduct";

        /// <summary>
        /// The entity type
        /// </summary>
        private Type entityType;

        /// <summary>
        /// The model type
        /// </summary>
        private Type modelType;

        /// <summary>
        /// The model to entity
        /// </summary>
        private readonly bool modelToEntity;

        /// <summary>
        /// The mappings
        /// </summary>
        private readonly List<Mapping> mappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationVisitor{TEntry}"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="entryParameter">The entry parameter.</param>
        public TranslationVisitor() : this(true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationVisitor{TEntry}"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="entryParameter">The entry parameter.</param>
        /// <param name="modelToEntity">if set to <c>true</c> [model to entity].</param>
        public TranslationVisitor(bool modelToEntity)
        {
            this.mappings = new List<Mapping>();
            this.modelToEntity = modelToEntity;
        }

        public ParameterExpression EntryParameter { get; set; }

        /// <summary>
        /// Adds the mapping.
        /// </summary>
        /// <param name="mapping">The mapping.</param>
        public void AddMapping(Mapping mapping)
        {
            if (!mappings.Any())
            {
                modelType = mapping.ModelMember.Member.DeclaringType;
                entityType = mapping.EntityMember.Member.DeclaringType;
            }
            mappings.Add(mapping);
        }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        public override Expression Visit(Expression node)
        {
            Debug.WriteLine("Expression : " + node.ToString() + " [=>] " + base.Visit(node));
            return base.Visit(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.Expression`1" />.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.Parameters.Count == 1)
            {
                var parameter = node.Parameters.Single();
                var param = TranslateParameter(parameter);

                if (param != null)
                {
                    var newExpression = Visit(node.Body);
                    var lambda = Expression.Lambda(typeof(TEntry), newExpression, new[] { param });
                    return lambda;
                }
            }
            return base.VisitLambda(node);
        }

        /// <summary>
        /// Visits the <see cref="T:System.Linq.Expressions.ParameterExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node.Name == EntryParameter.Name)
            {
                return EntryParameter;
            }
            return base.VisitParameter(node);
        }

        /// <summary>
        /// Translates the parameter.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        protected ParameterExpression TranslateParameter(ParameterExpression node)
        {
            if (node.Type == entityType)
            {
                EntryParameter = Expression.Parameter(modelType, node.Name);
            }
            if (node.Type == modelType)
            {
                EntryParameter = Expression.Parameter(entityType, node.Name);
            }
            return EntryParameter;
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            if ((node.Expression as ParameterExpression)?.Name == EntryParameter.Name)
            {
                var param = this.Visit(node.Expression) as ParameterExpression;
                if (node.Member.DeclaringType == entityType && !modelToEntity)
                {
                    return Expression.MakeMemberAccess(param, mappings.Single(x => x.EntityMember.Member.Name == node.Member.Name).ModelMember.Member);
                }
                if (node.Member.DeclaringType == modelType && modelToEntity)
                {
                    return Expression.MakeMemberAccess(param, mappings.Single(x => x.ModelMember.Member.Name == node.Member.Name).EntityMember.Member);
                }
            }
            return base.VisitMember(node);
        }
    }
}
