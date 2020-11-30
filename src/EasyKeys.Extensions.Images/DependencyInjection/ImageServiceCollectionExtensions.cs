using BarcodeLib;

using EasyKeys.Extensions.Images;
using EasyKeys.Extensions.Images.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ImageServiceCollectionExtensions
    {
        public static IServiceCollection AddImageProcessing(
            this IServiceCollection services)
        {
            // TODO: add resilience to this client.
            services.AddHttpClient<IImageDownloadService, ImageDownloadService>();
            services.AddScoped<IImageGenerationService, ImageGenerationService>();

            services.AddTransient((sp) => new Barcode());

            return services;
        }
    }
}
