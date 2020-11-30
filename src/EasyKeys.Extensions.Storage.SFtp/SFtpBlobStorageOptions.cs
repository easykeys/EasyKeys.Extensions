using EasyKeys.Extensions.Storage.Abstractions;

namespace EasyKeys.Extensions.Storage.SFtp
{
    public class SFtpBlobStorageOptions : BlobStorageOptions
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 22;

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
