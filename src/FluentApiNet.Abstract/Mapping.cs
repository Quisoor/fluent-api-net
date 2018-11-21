using System;
using System.Linq.Expressions;

namespace FluentApiNet.Abstract
{
    /// <summary>
    /// Mapping class
    /// </summary>
    public class Mapping
    {
        /// <summary>
        /// Gets the model member.
        /// </summary>
        /// <value>
        /// The model member.
        /// </value>
        public MemberExpression ModelMember { get; private set; }

        /// <summary>
        /// Gets the entity member.
        /// </summary>
        /// <value>
        /// The entity member.
        /// </value>
        public MemberExpression EntityMember { get; private set; }

        /// <summary>
        /// Sets the specified model lambda.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="modelLambda">The model lambda.</param>
        /// <param name="entityLambda">The entity lambda.</param>
        public void Set<TModel, TEntity>(Expression<Func<TModel, dynamic>> modelLambda, Expression<Func<TEntity, dynamic>> entityLambda)
        {
            var visitor = new MemberVisitor();
            visitor.Visit(modelLambda);
            ModelMember = visitor.Result;
            visitor.Visit(entityLambda);
            EntityMember = visitor.Result;
        }

        /// <summary>
        /// Initializes the specified model lambda.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="modelLambda">The model lambda.</param>
        /// <param name="entityLambda">The entity lambda.</param>
        /// <returns>Initialized mapping</returns>
        public static Mapping Init<TModel, TEntity>(Expression<Func<TModel, dynamic>> modelLambda, Expression<Func<TEntity, dynamic>> entityLambda)
        {
            var mapping = new Mapping();
            mapping.Set(modelLambda, entityLambda);
            return mapping;
        }
    }
}
