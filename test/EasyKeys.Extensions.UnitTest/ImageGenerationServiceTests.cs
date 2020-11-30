using System.Drawing;
using System.IO;
using System.Threading.Tasks;

using Bet.Extensions.Testing.Logging;

using EasyKeys.Extensions.Images;
using EasyKeys.Extensions.Images.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace EasyKeys.Extensions.UnitTest
{
    public class ImageGenerationServiceTests
    {
        private readonly ITestOutputHelper _output;

        public ImageGenerationServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task Test_Image_Resize_Successfully()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output, LogLevel.Debug);
            });

            services.AddImageProcessing();
            var sp = services.BuildServiceProvider();

            var width = 100;
            var height = 100;
            var originaFile = "Data/knoll-K217-lock-test.jpg";
            var imageService = sp.GetRequiredService<IImageGenerationService>();

            var file = File.ReadAllBytes(originaFile);
            var resizedBytes = await imageService.ResizeImageAsync(file, width, height);

            Assert.NotNull(resizedBytes);
            File.WriteAllBytes("Data/Updated.jpg", resizedBytes);

            using var stream = new MemoryStream();
            stream.Write(resizedBytes, 0, resizedBytes.Length);
            stream.Position = 0;

            var originalImage = Image.FromFile(originaFile);
            var resizedImage = Image.FromStream(stream);
            var (newWidth, newHeight) = imageService.GetImageSizeBasedOnAspectRatio(originalImage.Width, originalImage.Height, width, height);
            Assert.Equal(97, resizedImage.Width);
            Assert.Equal(newHeight, resizedImage.Height);
        }

        [Fact]
        public async Task Test_Image_Download()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output, LogLevel.Debug);
            });
            services.AddImageProcessing();
            var sp = services.BuildServiceProvider();
            var client = sp.GetRequiredService<IImageDownloadService>();

            var image = await client.GetImageAsync("https://cdn.easykeys.com/Images/dki/201904-Steelcase-XF_crop_sm.jpg");
            Assert.NotNull(image);
            image.Save("Data/Dowloaded.jpg");
        }

        [Fact]
        public async Task Test_Barcode()
        {
            const string textToEncode = "4582288";
            var services = new ServiceCollection();
            services.AddLogging(builder =>
                       {
                           builder.AddXunit(_output, LogLevel.Debug);
                       });
            services.AddImageProcessing();
            var sp = services.BuildServiceProvider();
            var barcode = sp.GetRequiredService<IImageGenerationService>();
            var bytes = await barcode.GetBarCodeAsync(textToEncode, 290, 120);
            Assert.NotNull(bytes);
            await File.WriteAllBytesAsync("Data/barcode.png", bytes);

            // create a barcode reader instance
            var result = BarcodeImageUtil.ReadCode39((Bitmap)Image.FromFile("Data/barcode.png"), false, Color.Yellow);
            Assert.NotNull(result);

            // do something with the result
            if (result != null)
            {
                Assert.Equal("4582288", result.Replace("*", ""));
                _output.WriteLine(result);
            }
        }
    }
}
