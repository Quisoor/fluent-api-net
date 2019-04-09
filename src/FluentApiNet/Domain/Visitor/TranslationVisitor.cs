using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace FluentApiNet.Domain.Visitor
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

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return Visit(node.Operand);
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
            return EntryParameter;
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
            var left = Visit(node.Left);
            var right = Visit(node.Right);
            if (IsNullableType(left.Type) && !IsNullableType(right.Type))
            {
                right = Expression.Convert(right, left.Type);
            }
            if (!IsNullableType(left.Type) && IsNullableType(right.Type))
            {
                left = Expression.Convert(left, right.Type);
            }
            if (left.Type != right.Type)
            {
                right = Expression.Convert(right, left.Type);
            }
            return Expression.MakeBinary(node.NodeType, left, right);
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
            if (node.Expression is ParameterExpression)
            {
                var param = this.Visit(node.Expression) as ParameterExpression;
                MemberExpression mapping = null;
                if (node.Member.DeclaringType == entityType && !modelToEntity)
                {
                    mapping = mappings.Single(x => x.EntityMember.Member.Name == node.Member.Name).ModelMember;
                }
                if (node.Member.DeclaringType == modelType && modelToEntity)
                {
                    mapping = mappings.Single(x => x.ModelMember.Member.Name == node.Member.Name).EntityMember;
                }
                if (mapping != null)
                {
                    if (mapping.Expression is ParameterExpression)
                    {
                        return Expression.MakeMemberAccess(param, mapping.Member);
                    }
                    else
                    {
                        var left = Visit(mapping.Expression);
                        return Expression.MakeMemberAccess(left, mapping.Member);
                    }
                }
            }
            if (node.Member.Name == "Value")
            {
                return base.Visit(node.Expression);
            }
            var result = base.VisitMember(node);
            return result;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return base.VisitConstant(node);
        }

        /// <summary>
        /// Determines whether [is nullable type] [the specified t].
        /// </summary>
        /// <param name="t">The t.</param>
        /// <returns>
        ///   <c>true</c> if [is nullable type] [the specified t]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNullableType(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
