using System.Linq;

namespace EasyKeys.Extensions.Storage.Abstractions
{
    public static class PathUtilities
    {
        public const string Delimiter = "/";

        public const char DelimiterChar = '/';

        public static string[] GetSegments(string path)
        {
            return path
                .Trim('/')
                .Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }
    }
}
