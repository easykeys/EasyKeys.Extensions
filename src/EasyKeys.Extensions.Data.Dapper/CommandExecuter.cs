using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Data.Dapper.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Data.Dapper
{
    public class CommandExecuter : ICommandExecuter
    {
        private readonly IOptionsMonitor<DbOptions> _optionsMonitor;
        private readonly ILogger<CommandExecuter> _logger;

        public CommandExecuter(
            IOptionsMonitor<DbOptions> optionsMonitor,
            ILogger<CommandExecuter> logger)
        {
            _optionsMonitor = optionsMonitor;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TReturn> ExecuteAsync<TReturn>(
            Func<IDbConnection, Task<TReturn>> task,
            string? namedOption = "",
            string? connectionString = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sw = ValueStopwatch.StartNew();

                _logger.LogDebug("[ExecuteAsync][Stared]");

                var options = _optionsMonitor.Get(namedOption);

                using var connection = new SqlConnection(connectionString ?? options.ConnectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                var result = await task(connection).ConfigureAwait(false);

                _logger.LogDebug("[ExecuteAsync][Completed] {timeSpan}ms", sw.GetElapsedTime().TotalMilliseconds);

                return result;
            }
            catch (TimeoutException ex)
            {
                throw new Exception(string.Format("{0}.{1}() experienced a SQL timeout", GetType().FullName, nameof(ExecuteAsync)), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(string.Format("{0}.{1}() experienced a SQL exception (not a timeout)", GetType().FullName, nameof(ExecuteAsync)), ex);
            }
        }

        public async Task ExecuteAsync(
            Func<IDbConnection, Task> task,
            string? namedOption = "",
            string? connectionString = default,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sw = ValueStopwatch.StartNew();

                _logger.LogDebug("[ExecuteAsync][Stared]");

                var options = _optionsMonitor.Get(namedOption);

                using var connection = new SqlConnection(connectionString ?? options.ConnectionString);
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                await task(connection).ConfigureAwait(false);

                _logger.LogDebug("[ExecuteAsync][Completed] {timeSpan}ms", sw.GetElapsedTime().TotalMilliseconds);
            }
            catch (TimeoutException ex)
            {
                throw new Exception(string.Format("{0}.{1}() experienced a SQL timeout", GetType().FullName, nameof(ExecuteAsync)), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(string.Format("{0}.{1}() experienced a SQL exception (not a timeout)", GetType().FullName, nameof(ExecuteAsync)), ex);
            }
        }
    }
}
