using System;
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
        /// The mappings
        /// </summary>
        private readonly List<Mapping> mappings;

        /// <summary>
        /// The entity type
        /// </summary>
        private readonly Type entityType;

        /// <summary>
        /// The model type
        /// </summary>
        private readonly Type modelType;

        /// <summary>
        /// The model to entity
        /// </summary>
        private readonly bool modelToEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationVisitor"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public TranslationVisitor(List<Mapping> mappings) : this(mappings, true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationVisitor"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public TranslationVisitor(List<Mapping> mappings, bool modelToEntity)
        {
            this.mappings = mappings;
            this.modelToEntity = modelToEntity;
            if (mappings.Any())
            {
                entityType = mappings.First().EntityMember.Member.DeclaringType;
                modelType = mappings.First().ModelMember.Member.DeclaringType;
            }
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
            Debug.WriteLine("Expression : " + node.ToString());
            return base.Visit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Debug.WriteLine("Lambda : " + node.ToString());
            if (node.Parameters.Count == 1)
            {
                var parameter = node.Parameters.Single();
                ParameterExpression param = null;
                Type target = null;
                if (parameter.Type == entityType && !modelToEntity)
                {
                    param = Expression.Parameter(modelType, parameter.Name);
                    target = modelType;
                }
                else if (parameter.Type == modelType && modelToEntity)
                {
                    param = Expression.Parameter(entityType, parameter.Name);
                    target = entityType;
                }

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
            Debug.WriteLine("Parameter : " + node.ToString());
            if (node.Type == modelType && modelToEntity)
            {
                return Expression.Parameter(entityType);
            }
            if (node.Type == entityType && !modelToEntity)
            {
                return Expression.Parameter(modelType);
            }
            return base.VisitParameter(node);
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
            Debug.WriteLine("Member : " + node.ToString());
            if (node.Member.DeclaringType == entityType && !modelToEntity)
            {
                return mappings.Single(x => x.EntityMember.Member.Name == node.Member.Name).ModelMember;
            }
            if (node.Member.DeclaringType == modelType && modelToEntity)
            {
                return mappings.Single(x => x.ModelMember.Member.Name == node.Member.Name).EntityMember;
            }
            return base.VisitMember(node);
        }        
    }
}
