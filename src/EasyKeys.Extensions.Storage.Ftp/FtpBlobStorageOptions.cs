using EasyKeys.Extensions.Storage.Abstractions;

using FluentFTP;

namespace EasyKeys.Extensions.Storage.Ftp
{
    public class FtpBlobStorageOptions : BlobStorageOptions
    {
        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public FtpEncryptionMode FtpEncryptionMode { get; set; }
    }
}
