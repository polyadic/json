using System.Text.Json;

namespace Polyadic.Json.NewType
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddNewTypeConverter(this JsonSerializerOptions options)
        {
            options.Converters.Add(new JsonNewTypeJsonConverterFactory());
            return options;
        }
    }
}
