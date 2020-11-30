using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Storage.Abstractions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Renci.SshNet;

namespace EasyKeys.Extensions.Storage.SFtp
{
    public class SFtpBlobStorage : IBlobStorage, IBlobContainer
    {
        private readonly ILogger<SFtpBlobStorage> _logger;
        private SFtpBlobStorageOptions _options;
        private SftpClient _client;

        public SFtpBlobStorage(
            IOptions<SFtpBlobStorageOptions> options,
            ILogger<SFtpBlobStorage> logger)
        {
            _options = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await ExecuteAsync(
                    async (client, remotePath) =>
                    {
                        var stream = new MemoryStream();
                        await Task.Factory.FromAsync(client.BeginDownloadFile(path, stream), client.EndDownloadFile);
                        return stream;
                    },
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(OpenReadAsync));
                throw;
            }
        }

        public async Task WriteAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                await ExecuteAsync(
                    async (client, remotePath) =>
                    {
                        await Task.Factory.FromAsync(client.BeginUploadFile(stream, path), client.EndUploadFile);
                    },
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(WriteAsync));
                throw;
            }
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                await ExecuteAsync(
                    async (client, remotePath) => await Task.Run(() => client.Delete(path), cancellationToken),
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(DeleteAsync));
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await ExecuteAsync(async (client, remotePath) => await Task.Run(() => client.Exists(path), cancellationToken), path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(ExistsAsync));
                throw;
            }
        }

        public async Task<BlobElement[]> ListAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await ExecuteAsync(
                    async (client, remotePath) =>
                    {
                        var fileList = await Task.Factory.FromAsync(client.BeginListDirectory(path, null, null), client.EndListDirectory);
                        var result = fileList.Select(i => i.IsDirectory ?
                            new BlobElement(i.FullName, i.Name, BlobElementType.Container) :
                            new BlobElement(i.FullName, i.Name, BlobElementType.Blob, i.Attributes.Size, i.LastWriteTime.ToUniversalTime(), i.LastWriteTime.ToUniversalTime()))
                        .ToArray();

                        return result;
                    },
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(ListAsync));
                throw;
            }
        }

        public async Task<BlobElement> GetAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await ExecuteAsync(
                    async (client, remotePath) =>
                    {
                        var i = client.Get(path);
                        return await Task.FromResult(new BlobElement(
                            i.FullName,
                            i.Name,
                            BlobElementType.Blob,
                            i.Attributes.Size,
                            i.LastWriteTime.ToUniversalTime(),
                            i.LastWriteTime.ToUniversalTime()));
                    },
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(ListAsync));
                throw;
            }
        }

        public async Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                return await ExecuteAsync(
                    async (client, remotePath) => await Task.FromResult(client.OpenWrite(path)),
                    path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{name} failed.", nameof(WriteAsync));
                throw;
            }
        }

        public async Task<Stream> OpenAppendAsync(string path, CancellationToken cancellationToken = default)
        {
            return await OpenWriteAsync(path, cancellationToken);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Disconnect();
                _client?.Dispose();
            }
        }

        private async Task<TResponse> ExecuteAsync<TResponse>(Func<SftpClient, string, Task<TResponse>> operation, string path)
        {
            CreateSFtpClient();
            var remotePath = ChangeDirectory(path, _client);
            return await operation(_client, remotePath);
        }

        private async Task ExecuteAsync(Func<SftpClient, string, Task> operation, string path)
        {
            CreateSFtpClient();
            var remotePath = ChangeDirectory(path, _client);
            await operation(_client, remotePath);
        }

        private void CreateSFtpClient()
        {
            if (_client == null || !_client.IsConnected)
            {
                _client = new SftpClient(_options.Host, _options.Port == 0 ? 22 : _options.Port, _options.UserName, _options.Password);
                _client.Connect();
            }
        }

        private string ChangeDirectory(string remotePath, SftpClient client)
        {
            client.ChangeDirectory(string.Empty);
            var filepath = remotePath.Replace(client.WorkingDirectory + "/", string.Empty).Split('/');
            var path = string.Empty;

            for (var i = 0; i < filepath.Length - 1; i++)
            {
                path += filepath[i] + "/";

                if (!client.Exists(path))
                {
                    client.CreateDirectory(path);
                }
            }

            client.ChangeDirectory(path);

            return path;
        }
    }
}
