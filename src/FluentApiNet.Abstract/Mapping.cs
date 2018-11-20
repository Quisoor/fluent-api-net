using System;
using System.Linq.Expressions;

namespace FluentApiNet.Abstract
{
    public class Mapping
    {
        public MemberExpression ModelMember { get; private set; }
        public MemberExpression EntityMember { get; private set; }

        public void Set<TModel, TEntity>(Expression<Func<TModel, dynamic>> modelLambda, Expression<Func<TEntity, dynamic>> entityLambda)
        {
            var visitor = new MemberVisitor();
            visitor.Visit(modelLambda);
            ModelMember = visitor.Result;
            visitor.Visit(entityLambda);
            EntityMember = visitor.Result;
        }

        public static Mapping Init<TModel, TEntity>(Expression<Func<TModel, dynamic>> modelLambda, Expression<Func<TEntity, dynamic>> entityLambda)
        {
            var mapping = new Mapping();
            mapping.Set<TModel, TEntity>(modelLambda, entityLambda);
            return mapping;
        }
    }
}
