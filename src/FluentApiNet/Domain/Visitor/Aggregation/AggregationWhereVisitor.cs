using FluentApiNet.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Domain.Visitor.Aggregation
{
    /// <summary>
    /// Aggregation Where Visitor class
    /// </summary>
    /// <typeparam name="TEntryModel">The type of the entry model.</typeparam>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
    public class AggregationWhereVisitor<TEntryModel> : ExpressionVisitor
    {
        /// <summary>
        /// The mappings
        /// </summary>
        private readonly List<Mapping> mappings;

        /// <summary>
        /// The multiple visitor
        /// </summary>
        private readonly AggregationWhereMultipleVisitor multipleVisitor;

        /// <summary>
        /// The parameter
        /// </summary>
        private ParameterExpression param;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregationWhereVisitor{TEntryModel}"/> class.
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        public AggregationWhereVisitor(List<Mapping> mappings)
        {
            this.mappings = mappings;
            multipleVisitor = new AggregationWhereMultipleVisitor();
        }

        /// <summary>
        /// Visits the lambda.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        public Expression VisitLambda(Expression<Func<TEntryModel, bool>> node, ParameterExpression parameter)
        {
            // define default parameter
            param = parameter;
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
                var lambda = Expression.Lambda(newExpression, new[] { param });
                return lambda;
            }
            return base.VisitLambda(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.MethodCallExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            // if Any or Where of LINQ is used
            if (node.Arguments.Count == 2)
            {
                // remove where or any part
                var left = base.Visit(node.Arguments[0] as MemberExpression) as MemberExpression;
                var rigth = (node.Arguments[1] as LambdaExpression).Body;
                if (left is null || rigth is null)
                {
                    return null;
                }
                return multipleVisitor.Visit(rigth, param);
            }
            else
            {
                // translate left part only
                var left = Visit(node.Object);
                if (left != null)
                { 
                    // update with left part traduced
                    return node.Update(left, node.Arguments);
                }
                else
                {
                    // if can't translate
                    return null;
                }
            }
        }

        /// <summary>
        /// Visits the children of the <see cref="T:System.Linq.Expressions.BinaryExpression" />.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>
        /// The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.
        /// </returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            // translate left part
            var left = Visit(node.Left);
            // translate right part
            var right = Visit(node.Right);
            if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse)
            {
                // return only part traduced
                if (left != null && right == null)
                {
                    return left;
                }
                else if (left == null && right != null)
                {
                    return right;
                }
                else if (left == null)
                {
                    return null;
                }
                else
                {
                    return Expression.MakeBinary(node.NodeType, left, right);
                }
            }
            else
            {
                if ((left == null) != (right == null))
                {
                    return null;
                }
            }
            return Expression.MakeBinary(node.NodeType, left, right);
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
            if (node.Expression is MemberExpression)
            {
                var propertyName = (node.Expression as MemberExpression).Member.Name;
                var mapping = mappings.SingleOrDefault(x => x.ModelMember.Member.Name == propertyName && x.ModelMember.Type == param.Type);
                if (mapping != null)
                {
                    return Expression.MakeMemberAccess(param, node.Member);
                }
                else
                {
                    return null;
                }
            }
            if (TypeTools.GetListType(node.Type) == param.Type)
            {
                return node;
            }
            if (node.Type != param.Type && (!node.Type.FullName.ToUpper().Contains("SYSTEM") || TypeTools.IsList(node.Type)))
            {
                return null;
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
            if (node.Type == typeof(TEntryModel))
            {
                return param;
            }
            return base.VisitParameter(node);
        }
    }
}
