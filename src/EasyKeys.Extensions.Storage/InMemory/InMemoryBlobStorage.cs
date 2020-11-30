using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Storage.Abstractions;

using Microsoft.Extensions.Options;

namespace EasyKeys.Extensions.Storage.InMemory
{
    public class InMemoryBlobStorage : IBlobStorage, IBlobContainer
    {
        private readonly IDictionary<string, byte[]> _blobs;
        private readonly object _lock = new object();

        public InMemoryBlobStorage(IOptions<InMemoryBlobStorageOptions> options)
        {
            _blobs = options.Value.Blobs;
        }

        public Task<BlobElement> GetAsync(string path, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                if (!_blobs.ContainsKey(path))
                {
                    throw new BlobNotFoundException(path, null);
                }

                return Task.FromResult(new BlobElement(path, string.Empty, BlobElementType.Blob, _blobs[path].LongLength));
            }
        }

        public Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                return Task.FromResult(_blobs.ContainsKey(path));
            }
        }

        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (!_blobs.ContainsKey(path))
                {
                    throw new BlobNotFoundException(path, null);
                }

                return Task.FromResult<Stream>(new MemoryStream(_blobs[path].ToArray())
                {
                    Position = 0
                });
            }
        }

        public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Stream>(new InternalMemoryStream(this, path));
        }

        public Task<Stream> OpenAppendAsync(string path, CancellationToken cancellationToken = default)
        {
            var stream = new InternalMemoryStream(this, path);
            stream.Write(_blobs[path], 0, _blobs[path].Length);
            return Task.FromResult<Stream>(stream);
        }

        public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_blobs.ContainsKey(path))
                {
                    _blobs.Remove(path);
                }

                return Task.CompletedTask;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Task<BlobElement[]> ListAsync(string path, CancellationToken cancellationToken = default)
        {
            var pathSegments = PathUtilities.GetSegments(path);
            var elements = ListInternal(pathSegments)
                .GroupBy(i => i.Id)
                .Select(g => g.First())
                .Where(i => PathUtilities.GetSegments(i.Id).Length == pathSegments.Length + 1)
                .ToArray();

            if (elements.Length == 0)
            {
                throw new ContainerNotFoundException(path, null);
            }

            return Task.FromResult(elements);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    _blobs.Clear();
                }
            }
        }

        private IEnumerable<BlobElement> ListInternal(string[] pathSegments)
        {
            lock (_lock)
            {
                foreach (var blob in _blobs)
                {
                    var blobSegments = PathUtilities.GetSegments(blob.Key);
                    if (blobSegments.Length >= pathSegments.Length + 1)
                    {
                        if (blobSegments.Length == pathSegments.Length + 1 &&
                            blobSegments.Take(blobSegments.Length - 1).SequenceEqual(pathSegments))
                        {
                            yield return new BlobElement(blob.Key, blobSegments.Last(), BlobElementType.Blob);
                        }

                        for (var i = 1; i < blobSegments.Length; i++)
                        {
                            var path = string.Join("/", blobSegments.Take(i));
                            yield return new BlobElement(path, blobSegments.Skip(i - 1).First(), BlobElementType.Container);
                        }
                    }
                }
            }
        }

        internal class InternalMemoryStream : MemoryStream
        {
            private readonly InMemoryBlobStorage _storage;
            private readonly string _identifier;

            public InternalMemoryStream(InMemoryBlobStorage storage, string identifier)
            {
                _storage = storage;
                _identifier = identifier;
            }

            protected override void Dispose(bool disposing)
            {
                lock (_storage._lock)
                {
                    _storage._blobs[_identifier] = ToArray();
                }

                base.Dispose(disposing);
            }
        }
    }
}
