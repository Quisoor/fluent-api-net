using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Domain.Visitor.Aggregation
{
    /// <summary>
    /// Aggregation Select Visitor class
    /// </summary>
    /// <typeparam name="TEntryModel">The type of the entry model.</typeparam>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
    public class AggregationSelectVisitor<TEntryModel> : ExpressionVisitor
    {
        /// <summary>
        /// The mappings
        /// </summary>
        private readonly List<Mapping> mappings;

        /// <summary>
        /// The parameter
        /// </summary>
        private ParameterExpression param;

        /// <summary>
        /// The target
        /// </summary>
        private Type target;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationSelectVisitor{TEntryModel}"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public AggregationSelectVisitor(List<Mapping> mappings)
        {
            this.mappings = mappings;
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
            // define the entry parameter
            param = node.Parameters.FirstOrDefault();
            // only if is a lambda expression with one parameter
            if (node.Parameters.Count == 1)
            {
                // translate the body
                var newExpression = Visit(node.Body);
                if (newExpression == null)
                {
                    // if can't translate the body, return true
                    newExpression = Expression.Constant(true);
                }
                // construct new translated lambda expression
                var lambda = Expression.Lambda<T>(newExpression, new[] { param });
                return lambda;
            }
            return base.VisitLambda(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MemberInitExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            // just to know target type
            if (mappings.Any(x => x.ModelMember.Type == node.NewExpression.Type))
            {
                target = node.NewExpression.Type;
            }
            return base.VisitMemberInit(node);
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
            if (node.Expression is ParameterExpression)
            {
                // retrieve the mapping
                var mapping = mappings.SingleOrDefault(x => x.ModelMember.Type == target);
                if (mapping != null)
                {
                    // if mapping exist, return new member access with traduced member
                    var property = param.Type.GetProperty(mapping.ModelMember.Member.Name);
                    var left = Expression.MakeMemberAccess(param, property);
                    return Expression.MakeMemberAccess(left, node.Member);
                }
            }
            return base.VisitMember(node);
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
            // if type equal entry type, return the default parameter
            if (node.Type == typeof(TEntryModel))
            {
                return param;
            }
            return base.VisitParameter(node);
        }
    }
}
