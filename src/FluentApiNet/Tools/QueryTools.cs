using System.Linq;

namespace FluentApiNet.Tools
{
    /// <summary>
    /// Query tools class
    /// </summary>
    public static class QueryTools
    {
        /// <summary>
        /// Pagines the specified query.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="page">The page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Pagined query</returns>
        public static IQueryable<TEntity> Pagine<TEntity>(this IQueryable<TEntity> query, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            var take = pageSize;
            return query.Skip(skip).Take(take);
        }
    }
}
