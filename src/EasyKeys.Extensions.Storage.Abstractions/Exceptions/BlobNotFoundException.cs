using System;

namespace EasyKeys.Extensions.Storage.Abstractions
{
    /// <summary>
    /// Thrown when the blob does not exist.
    /// </summary>
    public class BlobNotFoundException : Exception
    {
        public BlobNotFoundException(string id, Exception? innerException)
            : base($"The blob '{id}' does not exist.", innerException)
        {
            Id = id;
        }

        public BlobNotFoundException() : base()
        {
        }

        public BlobNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Gets the requested, non existent blob ID.
        /// </summary>
        public string Id { get; } = string.Empty;
    }
}
