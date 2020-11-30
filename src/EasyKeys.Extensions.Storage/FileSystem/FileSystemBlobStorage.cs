using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Storage.Abstractions;

using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Storage.FileSystem
{
    /// <summary>
    /// A local file system based blob storage.
    /// </summary>
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class FileSystemBlobStorage : IBlobStorage, IBlobContainer
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        private FileSystemBlobStorageOptions _options;

        public FileSystemBlobStorage(IOptions<FileSystemBlobStorageOptions> options)
        {
            _options = options.Value;
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(path);
            return Task.FromResult(File.Exists(fullPath));
        }

        public Task<BlobElement> GetAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                var fullPath = GetFullPath(path);
                return Task.FromResult(new BlobElement(
                    path,
                    string.Empty,
                    BlobElementType.Blob,
                    new FileInfo(fullPath).Length,
                    File.GetCreationTimeUtc(fullPath),
                    File.GetLastWriteTimeUtc(fullPath)));
            }
            catch (FileNotFoundException e)
            {
                throw new BlobNotFoundException(path, e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = GetFullPath(path);
                return Task.FromResult<Stream>(File.OpenRead(fullPath));
            }
            catch (FileNotFoundException e)
            {
                throw new BlobNotFoundException(path, e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        public Task<Stream> OpenAppendAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = GetFullPath(path);
                return Task.FromResult<Stream>(File.Open(fullPath, FileMode.Append, FileAccess.Write));
            }
            catch (FileNotFoundException e)
            {
                throw new BlobNotFoundException(path, e);
            }
            catch (DirectoryNotFoundException e)
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return Task.FromResult<Stream>(File.OpenWrite(fullPath));
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            var fullPath = GetFullPath(path);
            File.Delete(fullPath);
            return Task.CompletedTask;
        }

        public Task<BlobElement[]> ListAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = GetFullPath(path);

                var directories = Directory.GetDirectories(fullPath)
                    .Select(d => new BlobElement(d, Path.GetFileName(d), BlobElementType.Container));

                var files = Directory.GetFiles(fullPath)
                    .Select(d => new BlobElement(d, Path.GetFileName(d), BlobElementType.Blob));

                return Task.FromResult(directories.Concat(files).ToArray());
            }
            catch (DirectoryNotFoundException e)
            {
                throw new ContainerNotFoundException(path, e);
            }
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
        }

        protected virtual string GetFullPath(string identifier)
        {
            return Path.Combine(_options.BasePath, identifier);
        }
    }
}
