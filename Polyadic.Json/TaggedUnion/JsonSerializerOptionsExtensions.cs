using System.Text.Json;

namespace Polyadic.Json.TaggedUnion
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonSerializerOptions AddTaggedUnionConverter(this JsonSerializerOptions options, JsonTaggedUnionOptions? taggedUnionOptions = null)
        {
            options.Converters.Add(new JsonTaggedUnionConverterFactory(taggedUnionOptions ?? JsonTaggedUnionOptions.Default));
            return options;
        }
    }
}
