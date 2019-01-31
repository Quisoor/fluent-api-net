using System.Linq.Expressions;

namespace FluentApiNet.Domain.Visitor.Aggregation
{
    /// <summary>
    /// Aggregation Where Multiple Visitor class
    /// </summary>
    /// <seealso cref="System.Linq.Expressions.ExpressionVisitor" />
    public class AggregationWhereMultipleVisitor : ExpressionVisitor
    {
        /// <summary>
        /// The parameter
        /// </summary>
        private ParameterExpression param;

        /// <summary>
        /// Visits the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="param">The parameter.</param>
        /// <returns></returns>
        public Expression Visit(Expression node, ParameterExpression param)
        {
            this.param = param;
            return base.Visit(node);
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
            return param;
        }
    }
}
