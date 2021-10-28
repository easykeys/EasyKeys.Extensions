using BarcodeLib;

using EasyKeys.Extensions.Images;
using EasyKeys.Extensions.Images.Services;

using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ImageServiceCollectionExtensions
    {
        /// <summary>
        /// Adds image processing i.e. resize. Also add image download.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="policySelector"></param>
        /// <returns></returns>
        public static IServiceCollection AddImageProcessing(
            this IServiceCollection services,
            Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>? policySelector = null)
        {
            var builder = services
                .AddHttpClient<IImageDownloadService, ImageDownloadService>();

            // adds policy.
            if (policySelector != null)
            {
                builder.AddPolicyHandler(policySelector);
            }

            services.AddScoped<IImageGenerationService, ImageGenerationService>();

            services.AddTransient((sp) => new Barcode());

            return services;
        }
    }
}
