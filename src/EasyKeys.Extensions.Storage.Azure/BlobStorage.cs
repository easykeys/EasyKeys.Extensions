using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using EasyKeys.Extensions.Storage.Abstractions;

using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace EasyKeys.Extensions.Storage.Azure
{
#pragma warning disable CA1063 // Implement IDisposable Correctly
    public class BlobStorage : IBlobStorage, IBlobContainer
#pragma warning restore CA1063 // Implement IDisposable Correctly
    {
        private readonly CloudStorageAccount _storageAccount;

        public BlobStorage(ICloudBlobClientFactory cloudBlobClientFactory)
        {
            _storageAccount = cloudBlobClientFactory.CreateAccount();
        }

        public async Task<BlobElement> GetAsync(string path, CancellationToken cancellationToken)
        {
            try
            {
                var blob = await GetBlobReferenceAsync(path, cancellationToken).ConfigureAwait(false);
                await blob.FetchAttributesAsync().ConfigureAwait(false);
                return new BlobElement(
                    path,
                    string.Empty,
                    BlobElementType.Blob,
                    blob.Properties.Length,
                    blob.Properties.Created,
                    blob.Properties.LastModified,
                    blob.Properties.ETag);
            }
            catch (StorageException e) when (e.Message.Contains("does not exist."))
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        public async Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var blob = await GetBlobReferenceAsync(path, cancellationToken).ConfigureAwait(false);
                return await blob.OpenReadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (StorageException e) when (e.Message.Contains("does not exist."))
            {
                throw new BlobNotFoundException(path, e);
            }
        }

        public async Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            var blob = await GetBlobReferenceAsync(path, cancellationToken).ConfigureAwait(false);

            // blob.StreamWriteSizeInBytes = 1024 * 1024;
            return await blob.OpenWriteAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<Stream> OpenAppendAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var blob = await GetBlobReferenceAsync(path, cancellationToken).ConfigureAwait(false);
            return await blob.ExistsAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var blob = await GetBlobReferenceAsync(path, cancellationToken).ConfigureAwait(false);
                await blob.DeleteAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (StorageException e) when (e.Message.Contains("does not exist."))
            {
            }
        }

        public async Task<BlobElement[]> ListAsync(string path, CancellationToken cancellationToken = default)
        {
            var pathSegments = PathUtilities.GetSegments(path);
            if (pathSegments.Length == 0)
            {
                var cloudBlobClient = _storageAccount.CreateCloudBlobClient();

                BlobContinuationToken continuationToken = null;
                var results = new List<BlobElement>();
                do
                {
                    var response = await cloudBlobClient.ListContainersSegmentedAsync(continuationToken).ConfigureAwait(false);
                    continuationToken = response.ContinuationToken;
                    results.AddRange(response.Results.Select(c => new BlobElement(c.Name, c.Name, BlobElementType.Container)));
                }
                while (continuationToken != null);

                return results.ToArray();
            }
            else
            {
                var containerName = pathSegments.First();
                var containerPath = string.Join(PathUtilities.Delimiter, pathSegments.Skip(1));
                var container = await GetCloudBlobContainerAsync(containerName, cancellationToken).ConfigureAwait(false);

                BlobContinuationToken continuationToken = null;
                var results = new List<BlobElement>();
                do
                {
                    var response = pathSegments.Skip(1).Any() ?
                        await container.ListBlobsSegmentedAsync(containerPath + PathUtilities.Delimiter, continuationToken).ConfigureAwait(false) :
                        await container.ListBlobsSegmentedAsync(continuationToken).ConfigureAwait(false);

                    continuationToken = response.ContinuationToken;
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                    results.AddRange(response.Results.Select(i =>
                    {
                        if (i is CloudBlobDirectory directory)
                        {
                            return new BlobElement(directory.Prefix, PathUtilities.GetSegments(directory.Prefix).Last(), BlobElementType.Container);
                        }
                        else if (i is CloudBlob blob)
                        {
                            var blobNameSegments = blob.Name.Split(PathUtilities.DelimiterChar);
                            return new BlobElement(
                                blob.Name,
                                string.Join(PathUtilities.Delimiter, blobNameSegments.Skip(blobNameSegments.Length - 1)),
                                BlobElementType.Blob,
                                blob.Properties.Length,
                                blob.Properties.Created,
                                blob.Properties.LastModified);
                        }
                        else
                        {
                            return null;
                        }
                    }).Where(c => c != null));
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
                }
                while (continuationToken != null);

                if (results.Count == 0)
                {
                    throw new ContainerNotFoundException(path, null);
                }

                return results.ToArray();
            }
        }

#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
#pragma warning restore CA1063 // Implement IDisposable Correctly
        {
        }

        private async Task<CloudBlockBlob> GetBlobReferenceAsync(string path, CancellationToken cancellationToken)
        {
            path = path.Replace("\\", "/");

            var pathSegments = PathUtilities.GetSegments(path);
            var containerName = pathSegments.First();

            var blobName = string.Join(PathUtilities.Delimiter, pathSegments.Skip(1));

            var container = await GetCloudBlobContainerAsync(containerName, cancellationToken).ConfigureAwait(false);

            var result = container.GetBlockBlobReference(blobName);

            // set proper content type based on the file name extension
            result.Properties.ContentType = MimeTypeLookup.GetMimeType(Path.GetExtension(blobName));

            return result;
        }

        private async Task<CloudBlobContainer> GetCloudBlobContainerAsync(string containerName, CancellationToken cancellationToken)
        {
            var cloudBlobClient = _storageAccount.CreateCloudBlobClient();
            var cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);

            // TODO: Create on throw and do not always check
            if (!await cloudBlobContainer.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                await cloudBlobContainer.CreateAsync(cancellationToken).ConfigureAwait(false);
            }

            return cloudBlobContainer;
        }
    }
}
