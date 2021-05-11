using EasyKeys.Extensions.Storage.Abstractions;

namespace EasyKeys.Extensions.Storage.SFtp
{
    public class SFtpBlobStorageOptions : BlobStorageOptions
    {
        /// <summary>
        /// The name of the host. The default is empty.
        /// </summary>
        public string Host { get; set; } = string.Empty;

        /// <summary>
        /// The sftp port number. The default is 22.
        /// </summary>
        public int Port { get; set; } = 22;

        /// <summary>
        /// The user name for login.
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// If  password is empty and Private Key is present then it uses SSH2 ENCRYPTED PRIVATE KEY.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// OpenSSH SSH2 ENCRYPTED PRIVATE KEY.
        /// </summary>
        public string PrivateKey { get; set; } = string.Empty;
    }
}
