using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;

namespace EasyKeys.Extensions.Images
{
    /// <summary>
    /// Abstraction class for all of the image generations needs.
    /// </summary>
    public interface IImageGenerationService
    {
        /// <summary>
        /// Resize an image.
        /// </summary>
        /// <param name="imageBytes">The byte array of the image.</param>
        /// <param name="width">The new image width.</param>
        /// <param name="height">The new image height.</param>
        /// <param name="quality">The quality of the new image.</param>
        /// <param name="decoder">The <see cref="ImageFormat"/> Guid.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<byte[]> ResizeImageAsync(byte[] imageBytes, int width, int height, int quality = 75, Guid decoder = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resized an Image.
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="quality"></param>
        /// <param name="decoder"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> ResizeImageAsync(Image originalImage, int width, int height, int quality = 75, Guid decoder = default, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Bytes as Image.
        /// </summary>
        /// <param name="imageBytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Image> GetImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets image width and heights based on the original image ratio.
        /// </summary>
        /// <param name="originalWidth"></param>
        /// <param name="originalHeight"></param>
        /// <param name="resizeWidth"></param>
        /// <param name="resizeHeight"></param>
        /// <returns></returns>
        (int width, int height) GetImageSizeBasedOnAspectRatio(int originalWidth, int originalHeight, int resizeWidth = 0, int resizeHeight = 0);

        /// <summary>
        /// Generates Barcode with CODE39 Type.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<byte[]> GetBarCodeAsync(string text, int width, int height, CancellationToken cancellationToken = default);
    }
}
