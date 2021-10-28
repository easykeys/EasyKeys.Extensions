using System.Drawing;

namespace EasyKeys.Extensions.Images.Services
{
    public class ImageDownloadService : IImageDownloadService
    {
        private readonly HttpClient _httpClient;

        public ImageDownloadService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<Image> GetImageAsync(string url, CancellationToken cancellationToken = default)
        {
            var request = await _httpClient.GetAsync(url, cancellationToken);

            return Image.FromStream(await request.Content.ReadAsStreamAsync());
        }

        public Task<byte[]> GetImageBytesAsync(string url, CancellationToken cancellationToken = default)
        {
            return _httpClient.GetByteArrayAsync(url);
        }
    }
}
