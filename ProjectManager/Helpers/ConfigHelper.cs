using System.IO;
using System.Text.Json;

namespace ProjectManager.Helpers
{
    public static class ConfigHelper
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            AllowTrailingCommas = true
        };

        public static async Task<T> LoadConfigAsync<T>(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Configuration file not found: {filePath}");

                await using var fileStream = File.OpenRead(filePath);
                var result = await JsonSerializer.DeserializeAsync<T>(fileStream, _jsonOptions);

                return result ?? throw new InvalidOperationException("Deserialization returned null");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON configuration: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load configuration: {ex.Message}", ex);
            }
        }

        public static async Task SaveConfigAsync<T>(string filePath, T config)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

                if (config == null)
                    throw new ArgumentNullException(nameof(config));

                await using var fileStream = File.Create(filePath);
                await JsonSerializer.SerializeAsync(fileStream, config, _jsonOptions);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
            }
        }

        public static async Task<T> LoadOrCreateConfigAsync<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                var defaultConfig = new T();
                await SaveConfigAsync(filePath, defaultConfig);
                return defaultConfig;
            }

            return await LoadConfigAsync<T>(filePath);
        }
    }
}