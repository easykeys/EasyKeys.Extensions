using System.Drawing;

namespace EasyKeys.Extensions.Images.Services
{
    /// <summary>
    /// The image download service.
    /// </summary>
    public interface IImageDownloadService
    {
        /// <summary>
        /// Downloads image from specified url as <see cref="Image"/>.
        /// </summary>
        /// <param name="url">The url from which to download the image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Image> GetImageAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads image from specified url as array of bytes.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> GetImageBytesAsync(string url, CancellationToken cancellationToken = default);
    }
}
