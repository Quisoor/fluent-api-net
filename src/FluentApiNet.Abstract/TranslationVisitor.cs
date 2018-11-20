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
    public class TranslationVisitor : ExpressionVisitor
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
        /// Initializes a new instance of the <see cref="TranslationVisitor"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public TranslationVisitor(List<Mapping> mappings)
        {
            this.mappings = mappings;
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
            Debug.WriteLine(node.ToString());
            return base.Visit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Debug.WriteLine(node.ToString());
            Debug.WriteLine(node.Parameters.ToString());
            //TODO parameter type to map
            if (typeof(T) == entityType)
            {
                return node.Update(node.Body, node.Parameters);
            }
            if (typeof(T) == modelType)
            {
                return node.Update(node.Body, node.Parameters);
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
            Debug.WriteLine(node.ToString());
            if (node.Type == modelType)
            {
                return Expression.Parameter(entityType);
            }
            if (node.Type == entityType)
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
            Debug.WriteLine(node.ToString());
            if (node.Member.DeclaringType == entityType)
            {
                return mappings.Single(x => x.EntityMember.Member.Name == node.Member.Name).ModelMember;
            }
            if (node.Member.DeclaringType == modelType)
            {
                return mappings.Single(x => x.ModelMember.Member.Name == node.Member.Name).EntityMember;
            }
            return base.VisitMember(node);
        }


    }
}
