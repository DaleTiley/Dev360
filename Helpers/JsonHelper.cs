
using System.IO;
using Newtonsoft.Json;

namespace MillenniumWebFixed.Helpers
{
    public static class JsonHelper
    {
        public static T LoadJsonFromFile<T>(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static bool IsValidJson(string json)
        {
            try
            {
                var obj = JsonConvert.DeserializeObject(json);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
