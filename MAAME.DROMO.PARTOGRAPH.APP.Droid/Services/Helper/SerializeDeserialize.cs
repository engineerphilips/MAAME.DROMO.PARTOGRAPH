namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services.Helper
{
    using System.Globalization;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal static class SerializeDeserialize
    {
        private static readonly JsonSerializerOptions DefaultOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static async Task<T?> DeserializeJsonFromStreamAsync<T>(Stream stream)
        {
            if (stream is not { CanRead: true })
                return default;

            return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions).ConfigureAwait(false);
        }

        public static T? DeserializeBoolean<T>(bool data)
        {
            return (T?)Convert.ChangeType(data, typeof(T));
        }

        public static async Task<string?> StreamToStringAsync(Stream stream)
        {
            if (stream == null)
                return null;

            using var sr = new StreamReader(stream);
            return await sr.ReadToEndAsync().ConfigureAwait(false);
        }

        public static void SerializeJsonIntoStream(object value, Stream stream)
        {
            if (stream is not { CanWrite: true })
                throw new ArgumentException("Stream is not writable.");

            JsonSerializer.Serialize(stream, value, DefaultOptions);
        }

        public static HttpContent? CreateHttpContent(object content)
        {
            if (content == null)
                return default;

            // Option 1: Simple approach using JsonContent (recommended)
            return JsonContent.Create(content, content.GetType(), options: DefaultOptions);

            // Option 2: If you need the stream approach for compatibility
            // var ms = new MemoryStream();
            // JsonSerializer.Serialize(ms, content, DefaultOptions);
            // ms.Seek(0, SeekOrigin.Begin);
            // var httpContent = new StreamContent(ms);
            // httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            // return httpContent;
        }
    }
}
