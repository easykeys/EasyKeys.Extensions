using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using StackExchange.Redis;

namespace Microsoft.Extensions.Caching.StackExchangeRedis
{
    /// <summary>
    /// A workaround class.
    /// https://github.com/dotnet/runtime/issues/36547#issuecomment-208971111.
    /// </summary>
    public class StackExchangeRedisDistributedCacheExtended : IDistributedCacheExtended, IDisposable
    {
        private const string ClearCacheLuaScript =
            "for _,k in ipairs(redis.call('KEYS', ARGV[1])) do\n" +
            "    redis.call('DEL', k)\n" +
            "end";

        private const string GetKeysLuaScript = "return redis.call('keys', ARGV[1])";
        private readonly RedisCacheOptions _options;
        private ConnectionMultiplexer _connection;
        private IDatabase _cache;
        private bool _isDisposed;

        public StackExchangeRedisDistributedCacheExtended(IOptions<RedisCacheOptions> redisCacheOptions)
        {
            _options = redisCacheOptions.Value;
        }

        ~StackExchangeRedisDistributedCacheExtended()
        {
            Dispose(false);
        }

        public async Task ClearAsync()
        {
            ThrowIfDisposed();
            await EnsureInitialized();
            await _cache.ScriptEvaluateAsync(
                ClearCacheLuaScript,
                values: new RedisValue[]
                {
                _options.InstanceName + "*"
                });
        }

        public async Task<IEnumerable<string>> GetKeysAsync()
        {
            ThrowIfDisposed();
            await EnsureInitialized();
            var result = await _cache.ScriptEvaluateAsync(
                GetKeysLuaScript,
                values: new RedisValue[]
                {
                _options.InstanceName + "*"
                });
            return ((RedisResult[])result).Select(x => x.ToString().Substring(_options.InstanceName.Length)).ToArray();
        }

        public async Task RemoveAsync(string[] keys)
        {
            ThrowIfDisposed();
            if (keys == null) { throw new ArgumentNullException(nameof(keys)); }
            await EnsureInitialized();
            var keysArray = keys.Select(x => (RedisKey)(_options.InstanceName + x)).ToArray();
            await _cache.KeyDeleteAsync(keysArray);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected async Task EnsureInitialized()
        {
            if (_connection == null)
            {
                _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
                _cache = _connection.GetDatabase();
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing && _connection != null)
                {
                    _connection.Close();
                }

                _isDisposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(StackExchangeRedisDistributedCacheExtended));
            }
        }
    }
}
