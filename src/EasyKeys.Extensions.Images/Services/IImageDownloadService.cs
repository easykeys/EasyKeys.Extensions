using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace EasyKeys.Extensions.Images.Services
{
    /// <summary>
    /// The image download service.
    /// </summary>
    public interface IImageDownloadService
    {
        /// <summary>
        /// Downloads image from specified url.
        /// </summary>
        /// <param name="url">The url from which to download the image.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Image> GetImageAsync(string url, CancellationToken cancellationToken = default);
    }
}
