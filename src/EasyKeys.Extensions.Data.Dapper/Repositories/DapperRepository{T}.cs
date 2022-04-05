using Dapper;
using Dapper.Contrib.Extensions;

using EasyKeys.Extensions.Data.Dapper.Paging;
using EasyKeys.Extensions.Data.Dapper.Sorting;

namespace EasyKeys.Extensions.Data.Dapper.Repositories
{
    public class DapperRepository<T> : IAsyncRepository<T> where T : BaseEntity
    {
        public DapperRepository(ICommandExecuter commandExecuter)
        {
            CommandExecuter = commandExecuter;
        }

        public ICommandExecuter CommandExecuter { get; }

        public string TableName => (typeof(T).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute)?.Name ?? typeof(T).Name;

        public virtual Task<T> GetByIdAsync(
            int id,
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c => await c.GetAsync<T>(id).ConfigureAwait(false),
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public virtual Task<IEnumerable<T>> GetAllAsync(
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c => await c.GetAllAsync<T>().ConfigureAwait(false),
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public virtual Task<int> InsertAsync(
            T data,
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c => await c.InsertAsync(data).ConfigureAwait(false),
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public virtual Task<bool> UpdateAsync(
            T data,
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            var nameedOption = namedOption ?? typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c => await c.UpdateAsync(data),
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public virtual Task<bool> DeleteAsync(
            T data,
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c => await c.DeleteAsync(data),
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public virtual Task<PagedResults<T>> GetAsync(
            PagedRequest pagedRequest,
            object filters,
            List<SortDescriptor>? sortDescriptors = default,
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c =>
                {
                    var result = new PagedResults<T>();
                    var container = FilterHelper.ParseProperties(filters);
                    var where = string.Empty;

                    var builder = new SqlBuilder();

                    // assumes the table name = entity name
                    var selectorStatement = $"SELECT * FROM {TableName}";

                    if (filters != null)
                    {
                        var q = FilterHelper.GetSqlPairs(container.AllNames, " AND ");
                        if (!string.IsNullOrEmpty(q))
                        {
                            where = $" WHERE {q} ";
                            selectorStatement += where;
                        }
                    }

                    selectorStatement += " /**orderby**/ OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                    // TODO: needs addition of order by in total count statement?
                    var totalCountStatement = $" SELECT [TotalCount] = COUNT(*) FROM {TableName}";

                    if (filters != null)
                    {
                        totalCountStatement += where;
                    }

                    selectorStatement += totalCountStatement;

                    var selectorQuery = builder.AddTemplate(selectorStatement);

                    if (sortDescriptors != null)
                    {
                        foreach (var sorting in sortDescriptors)
                        {
                            if (string.IsNullOrWhiteSpace(sorting.Field))
                            {
                                continue;
                            }

                            if (sorting.Direction == SortingDirection.Ascending)
                            {
                                builder.OrderBy(sorting.Field);
                            }
                            else if (sorting.Direction == SortingDirection.Descending)
                            {
                                builder.OrderBy(sorting.Field + " desc");
                            }
                        }
                    }
                    else
                    {
                        // set to the first property
                        builder.OrderBy(typeof(T).GetProperties()[0].Name);
                    }

                    var pagedParameters = pagedRequest.GetParameters();

                    var param = container.AllPairs.ToList();
                    param.Add(new KeyValuePair<string, object>("Offset", pagedParameters.OffSet));
                    param.Add(new KeyValuePair<string, object>("PageSize", pagedParameters.PageSize));

                    using (var gridResult = await c.QueryMultipleAsync(selectorQuery.RawSql, param))
                    {
                        result.Items = gridResult.Read<T>();
                        result.TotalCount = gridResult.ReadSingle<int>();
                    }

                    return result;
                },
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public virtual Task<PagedResults<T>> GetAsync(
            PagedRequest pagedRequest,
            string filters,
            List<SortDescriptor>? sortDescriptors = default,
            string? namedOption = default,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return CommandExecuter.ExecuteAsync(
                async c =>
                {
                    var result = new PagedResults<T>();
                    var where = string.Empty;

                    var builder = new SqlBuilder();

                    // assumes the table name = entity name
                    var selectorStatement = $"SELECT * FROM {TableName}";

                    if (!string.IsNullOrEmpty(filters))
                    {
                        where = $" WHERE {filters} ";
                        selectorStatement += where;
                    }

                    selectorStatement += " /**orderby**/ OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

                    // TODO: needs addition of order by in total count statement?
                    var totalCountStatement = $" SELECT [TotalCount] = COUNT(*) FROM {TableName}";

                    if (filters != null)
                    {
                        totalCountStatement += where;
                    }

                    selectorStatement += totalCountStatement;

                    var selectorQuery = builder.AddTemplate(selectorStatement);

                    if (sortDescriptors != null)
                    {
                        foreach (var sorting in sortDescriptors)
                        {
                            if (string.IsNullOrWhiteSpace(sorting.Field))
                            {
                                continue;
                            }

                            if (sorting.Direction == SortingDirection.Ascending)
                            {
                                builder.OrderBy(sorting.Field);
                            }
                            else if (sorting.Direction == SortingDirection.Descending)
                            {
                                builder.OrderBy(sorting.Field + " desc");
                            }
                        }
                    }
                    else
                    {
                        // set to the first property
                        builder.OrderBy(typeof(T).GetProperties()[0].Name);
                    }

                    var pagedParameters = pagedRequest.GetParameters();

                    var param = new PropertyContainer().AllPairs.ToList();
                    param.Add(new KeyValuePair<string, object>("Offset", pagedParameters.OffSet));
                    param.Add(new KeyValuePair<string, object>("PageSize", pagedParameters.PageSize));

                    using (var gridResult = await c.QueryMultipleAsync(selectorQuery.RawSql, param))
                    {
                        result.Items = gridResult.Read<T>();
                        result.TotalCount = gridResult.ReadSingle<int>();
                    }

                    return result;
                },
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAsync(
            object filters,
            List<SortDescriptor>? sortDescriptors = null,
            string? namedOption = null,
            CancellationToken cancellationToken = default)
        {
            namedOption ??= typeof(T).Name;

            return await CommandExecuter.ExecuteAsync(
                async c =>
                {
                    var container = FilterHelper.ParseProperties(filters);
                    var where = string.Empty;

                    var builder = new SqlBuilder();

                    // assumes the table name = entity name
                    var selectorStatement = $"SELECT * FROM {TableName}";

                    if (filters != null)
                    {
                        var q = FilterHelper.GetSqlPairs(container.AllNames, " AND ");
                        if (!string.IsNullOrEmpty(q))
                        {
                            where = $" WHERE {q} ";
                            selectorStatement += where;
                        }
                    }

                    selectorStatement += " /**orderby**/;";

                    var selectorQuery = builder.AddTemplate(selectorStatement);

                    if (sortDescriptors != null)
                    {
                        foreach (var sorting in sortDescriptors)
                        {
                            if (string.IsNullOrWhiteSpace(sorting.Field))
                            {
                                continue;
                            }

                            if (sorting.Direction == SortingDirection.Ascending)
                            {
                                builder.OrderBy(sorting.Field);
                            }
                            else if (sorting.Direction == SortingDirection.Descending)
                            {
                                builder.OrderBy(sorting.Field + " desc");
                            }
                        }
                    }
                    else
                    {
                        // set to the first property
                        builder.OrderBy(typeof(T).GetProperties()[0].Name);
                    }

                    var param = container.AllPairs.ToList();
                    return await c.QueryAsync<T>(selectorQuery.RawSql, filters);
                },
                namedOption: namedOption,
                cancellationToken: cancellationToken);
        }
    }
}
