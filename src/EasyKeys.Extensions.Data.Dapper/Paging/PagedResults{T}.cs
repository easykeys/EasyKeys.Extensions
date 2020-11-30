using System.Collections.Generic;

namespace EasyKeys.Extensions.Data.Dapper.Paging
{
    public class PagedResults<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();

        public int TotalCount { get; set; }
    }
}
