﻿using EasyKeys.Extensions.Storage.Abstractions.Internal;

namespace EasyKeys.Extensions.Storage.Abstractions
{
    /// <summary>
    /// Container based blob storage extension methods.
    /// </summary>
    public static class IBlobStorageExtensions
    {
        /// <summary>
        /// Gets a container inside the storage.
        /// The container path must have at least one segment, e.g. 'foo' or 'foo/bar'.
        /// </summary>
        /// <param name="storage">The blob storage.</param>
        /// <param name="path">The container path.</param>
        /// <returns></returns>
        public static IBlobContainer GetContainer(this IBlobStorage storage, string path)
        {
            return new BlobStorageToContainerAdapter(storage, path);
        }
    }
}
