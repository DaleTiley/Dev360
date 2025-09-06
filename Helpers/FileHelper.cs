
using System.IO;
using System.Linq;
namespace MillenniumWebFixed.Helpers
{
    public static class FileHelper
    {
        public static bool HasAllowedExtension(string fileName, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(fileName)?.ToLower();
            return allowedExtensions.Contains(extension);
        }

        public static string GetFileNameWithoutExtension(string fileName)
        {
            return Path.GetFileNameWithoutExtension(fileName);
        }
    }
}
