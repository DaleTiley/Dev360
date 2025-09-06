
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MillenniumWebFixed.Helpers
{
    public static class ImageHelper
    {
        public static byte[] ConvertToByteArray(Stream imageStream)
        {
            using (var ms = new MemoryStream())
            {
                imageStream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static bool IsValidImageType(string contentType)
        {
            return contentType == "image/jpeg" || contentType == "image/png" || contentType == "image/gif";
        }
    }
}
