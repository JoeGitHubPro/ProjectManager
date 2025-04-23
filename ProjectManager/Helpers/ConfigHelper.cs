using System.IO;
using System.Text.Json;

namespace ProjectManager.Helpers
{
    public static class ConfigHelper
    {
        public static T LoadConfig<T>(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Configuration file not found: {filePath}");

            string json = File.ReadAllText(filePath);

            var result = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }
}
