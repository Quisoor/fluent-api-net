namespace FluentApiNet.Tools
{
    /// <summary>
    /// Pagination tools class
    /// </summary>
    public static class PaginationTools
    {
        /// <summary>
        /// The default page
        /// </summary>
        public const int DEFAULT_PAGE = 1;

        /// <summary>
        /// The default pagesize
        /// </summary>
        public const int DEFAULT_PAGESIZE = 25;

        /// <summary>
        /// Limits the page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>Limited page</returns>
        public static int LimitPage(int? page)
        {
            if (page.HasValue && page.Value > 0)
            {
                return page.Value;
            }
            else
            {
                return DEFAULT_PAGE;
            }
        }

        /// <summary>
        /// Limits the size of the page.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns>Limited page size</returns>
        public static int LimitPageSize(int? pageSize)
        {
            if (pageSize.HasValue && pageSize.Value >= 0)
            {
                return pageSize.Value;
            }
            else
            {
                return DEFAULT_PAGESIZE;
            }
        }
    }
}
