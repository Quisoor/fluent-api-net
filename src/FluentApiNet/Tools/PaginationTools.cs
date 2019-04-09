namespace FluentApiNet.Tools
{
    /// <summary>
    /// Pagination tools class
    /// </summary>
    public static class PaginationTools
    {
        /// <summary>
        /// Gets or sets the default page.
        /// </summary>
        /// <value>
        /// The default page.
        /// </value>
        public static int DEFAULT_PAGE { get; set; } = 1;

        /// <summary>
        /// Gets or sets the default pagesize.
        /// </summary>
        /// <value>
        /// The default pagesize.
        /// </value>
        public static int? DEFAULT_PAGESIZE { get; set; } = null;

        /// <summary>
        /// Gets or sets the maximum pagesize.
        /// </summary>
        /// <value>
        /// The maximum pagesize.
        /// </value>
        public static int MAX_PAGESIZE { get; set; } = 100;

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
        public static int? LimitPageSize(int? pageSize)
        {
            if (pageSize.HasValue && pageSize.Value >= 0)
            {
                if (pageSize.Value <= MAX_PAGESIZE)
                {
                    return pageSize.Value;
                }
                else
                {
                    return MAX_PAGESIZE;
                }
            }
            else
            {
                return DEFAULT_PAGESIZE;
            }
        }
    }
}
