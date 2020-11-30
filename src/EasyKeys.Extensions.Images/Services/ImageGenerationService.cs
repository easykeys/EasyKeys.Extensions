using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BarcodeLib;

using Microsoft.Extensions.Logging;

namespace EasyKeys.Extensions.Images
{
    /// <inheritdoc/>
    public class ImageGenerationService : IImageGenerationService
    {
        private readonly Barcode _barcode;
        private readonly ILogger<ImageGenerationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageGenerationService"/> class.
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="logger"></param>
        public ImageGenerationService(Barcode barcode, ILogger<ImageGenerationService> logger)
        {
            _barcode = barcode ?? throw new ArgumentNullException(nameof(barcode));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<byte[]> ResizeImageAsync(
            byte[] imageBytes,
            int width,
            int height,
            int quality = 75,
            Guid decoder = default,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("[Started][{name}]", nameof(ImageGenerationService));

            // gets original image from bytes.
            var originalImage = await GetImageAsync(imageBytes);

            return await ResizeImageAsync(originalImage, width, height, quality, decoder, cancellationToken);
        }

        public Task<byte[]> ResizeImageAsync(
            Image originalImage,
            int width,
            int height,
            int quality = 75,
            Guid decoder = default,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    if (decoder == default)
                    {
                        decoder = ImageFormat.Jpeg.Guid;
                    }

                    // gets resize aspect ratio based on width.
                    var (newWidth, newHeight) = GetImageSizeBasedOnAspectRatio(originalImage.Width, originalImage.Height, width, height);

                    using var image = new Bitmap(originalImage);
                    using var resized = new Bitmap(newWidth, newHeight);
                    using var graphics = Graphics.FromImage(resized);

                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(image, 0, 0, newWidth, newHeight);

                    using var output = new MemoryStream();
                    var qualityParamId = Encoder.Quality;
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(qualityParamId, quality);
                    var codec = ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == decoder);
                    resized.Save(output, codec, encoderParameters);
                    output.Position = 0;

                    _logger.LogInformation("[Ended][{name}]", nameof(ImageGenerationService));

                    return output.ToArray();
                },
                cancellationToken);
        }

        public async Task<Image> GetImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
        {
            using var stream = new MemoryStream();
            await stream.WriteAsync(imageBytes, 0, imageBytes.Length, cancellationToken);
            stream.Position = 0;

            return Image.FromStream(stream);
        }

        public (int width, int height) GetImageSizeBasedOnAspectRatio(
            int originalWidth,
            int originalHeight,
            int resizeWidth,
            int resizeHeight)
        {
            if (resizeWidth == 0)
            {
                resizeWidth = originalWidth;
            }

            if (resizeHeight == 0)
            {
                resizeHeight = originalHeight;
            }

            var ratioX = (double)resizeWidth / originalWidth;
            var ratioY = (double)resizeHeight / originalHeight;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(originalWidth * ratio);
            var newHeight = (int)(originalHeight * ratio);

            return (newWidth, newHeight);
        }

        public Task<byte[]> GetBarCodeAsync(string text, int width, int height, CancellationToken cancellationToken = default)
        {
            return Task.Run(
                () =>
                {
                    _barcode.ForeColor = Color.Black;
                    _barcode.BackColor = Color.White;

                    _barcode.Width = width;
                    _barcode.Height = height;

                    _barcode.ImageFormat = ImageFormat.Png;

                    _barcode.Encode(TYPE.CODE39, text);

                    var result = _barcode.GetImageData(SaveTypes.PNG);
                    var err = _barcode.Errors;
                    return result;
                },
                cancellationToken);
        }
    }
}
