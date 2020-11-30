using System;

namespace EasyKeys.Extensions.Storage.Abstractions
{
    /// <summary>
    /// Thrown when the blob container does not exist.
    /// </summary>
    public class ContainerNotFoundException : Exception
    {
        public ContainerNotFoundException(string id, Exception? innerException)
            : base($"The container '{id}' does not exist.", innerException)
        {
            Id = id;
        }

        public ContainerNotFoundException() : base()
        {
        }

        public ContainerNotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Gets the requested, non existent blob ID.
        /// </summary>
        public string Id { get; } = string.Empty;
    }
}
