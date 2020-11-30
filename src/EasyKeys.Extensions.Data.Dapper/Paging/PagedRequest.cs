namespace EasyKeys.Extensions.Data.Dapper.Paging
{
    /// <summary>
    /// Generic paged request.
    /// </summary>
    public class PagedRequest
    {
        private int? _pageSize;

        /// <summary>
        /// Gets or sets the page number.
        /// </summary>
        public virtual int? PageNo { get; set; }

        /// <summary>
        /// Gets or sets the page size. Defaults to 10.
        /// </summary>
        public virtual int? PageSize { get => _pageSize; set => _pageSize = value ?? 10; }
    }
}
