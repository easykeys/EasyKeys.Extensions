using System.Reflection;

using EasyKeys.Extensions.Data.Dapper.Paging;

namespace Dapper
{
    public static class DapperExtensions
    {
        /// <summary>
        /// Adds a parameter if it is not null to the DynamicParameters object.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void AddNonNullParameter(this DynamicParameters parameters, string name, object value)
        {
            if (value != null)
            {
                parameters.Add(name, value);
            }
        }

        /// <summary>
        /// A helper method fro fetching the start and end numbers from a paged request.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static PagingDbParameter GetParameters(this PagedRequest request)
        {
            var pageSize = request.PageSize ?? 10;
            var pageNo = request.PageNo ?? 1;

            return new PagingDbParameter
            {
                OffSet = pageSize * (pageNo - 1),
                PageSize = pageSize
            };
        }

        public static void Trim<T>(this T item)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var p in properties)
            {
                if (p.PropertyType != typeof(string) || !p.CanWrite || !p.CanRead)
                {
                    continue;
                }

                var value = p.GetValue(item) as string;
                p.SetValue(item, value?.Trim());
            }
        }
    }
}
