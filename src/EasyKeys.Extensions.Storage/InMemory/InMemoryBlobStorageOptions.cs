using System.Collections.Generic;

using EasyKeys.Extensions.Storage.Abstractions;

namespace EasyKeys.Extensions.Storage.InMemory
{
    public class InMemoryBlobStorageOptions : BlobStorageOptions
    {
        public IDictionary<string, byte[]> Blobs { get; set; } = new Dictionary<string, byte[]>();
    }
}
