using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Storage.Abstractions;

using FluentFTP;

using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Storage.Ftp
{
    public class FtpBlobStorage : IBlobStorage, IBlobContainer
    {
        private readonly FtpClient _client;

        private bool _loadPropertiesWithListing = false;

        public FtpBlobStorage(IOptions<FtpBlobStorageOptions> options)
        {
            _client = options.Value.Port != 0 ? new FtpClient(options.Value.Host) : new FtpClient(options.Value.Host, options.Value.Port, null, null);

            _client.EncryptionMode = options.Value.FtpEncryptionMode;

            if (!string.IsNullOrEmpty(options.Value.UserName))
            {
                _client.Credentials = new NetworkCredential(options.Value.UserName, options.Value.Password);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await _client.OpenReadAsync(path, FtpDataType.Binary, 0, true, cancellationToken).ConfigureAwait(false);
            }
            catch (FtpCommandException e) when (e.CompletionCode == "550")
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        public async Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await _client.OpenWriteAsync(path, FtpDataType.Binary, true, cancellationToken).ConfigureAwait(false);
            }
            catch (FtpCommandException e) when (e.CompletionCode == "550")
            {
                var directory = Path.GetDirectoryName(path);
                await _client.CreateDirectoryAsync(directory, cancellationToken);
                return await _client.OpenWriteAsync(path, FtpDataType.Binary, true, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<Stream> OpenAppendAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await _client.OpenAppendAsync(path, FtpDataType.Binary, true, cancellationToken).ConfigureAwait(false);
            }
            catch (FtpCommandException e) when (e.CompletionCode == "550")
            {
                var directory = Path.GetDirectoryName(path);
                await _client.CreateDirectoryAsync(directory, cancellationToken);
                return await _client.OpenAppendAsync(path, FtpDataType.Binary, true, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);
                await _client.DeleteFileAsync(path, cancellationToken).ConfigureAwait(false);
            }
            catch (FtpCommandException e) when (e.CompletionCode == "550")
            {
                // Ignore file does not exist
            }
        }

        public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);
            return await _client.FileExistsAsync(path, cancellationToken).ConfigureAwait(false);
        }

        public async Task<BlobElement[]> ListAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var items = await _client.GetListingAsync(path).ConfigureAwait(false);
                return items
                    .Select(i => i.Type == FtpFileSystemObjectType.Directory ?
                        new BlobElement(i.FullName, i.Name, BlobElementType.Container) :
                        new BlobElement(i.FullName, i.Name, BlobElementType.Blob, i.Size, i.Created.ToUniversalTime(), i.Modified.ToUniversalTime()))
                    .ToArray();
            }
            catch (FtpCommandException e) when (e.CompletionCode == "550")
            {
                throw new ContainerNotFoundException(path, e);
            }
        }

        public async Task<BlobElement> GetAsync(string path, CancellationToken cancellationToken = default)
        {
            await _client.AutoConnectAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (_client.Capabilities.Contains(FtpCapability.MLSD) && !_loadPropertiesWithListing)
                {
                    try
                    {
                        var info = await _client.GetObjectInfoAsync(path, true).ConfigureAwait(false);
                        return new BlobElement(
                            path,
                            string.Empty,
                            BlobElementType.Blob,
                            info.Size,
                            info.Created.ToUniversalTime(),
                            info.Modified.ToUniversalTime());
                    }
                    catch (PlatformNotSupportedException)
                    {
                        _loadPropertiesWithListing = true;
                        return await GetPropertiesWithListingAsync(path).ConfigureAwait(false);
                    }
                }
                else
                {
                    return await GetPropertiesWithListingAsync(path).ConfigureAwait(false);
                }
            }
            catch (FtpCommandException e) when (e.CompletionCode == "550")
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Disconnect();
                _client?.Dispose();
            }
        }

        private async Task<BlobElement> GetPropertiesWithListingAsync(string path)
        {
            var name = Path.GetFileName(path);
            var directory = Path.GetDirectoryName(path);

            var items = await _client.GetListingAsync(directory).ConfigureAwait(false);
            var item = items.SingleOrDefault(i => i.Name == name);
            if (item == null)
            {
                throw new BlobNotFoundException(path, null);
            }

            return new BlobElement(
                path,
                string.Empty,
                BlobElementType.Blob,
                item.Size,
                item.Created.ToUniversalTime(),
                item.Modified.ToUniversalTime());
        }
    }
}
