using System.Text.Json;

namespace Polyadic.Json.TaggedUnion
{
    public sealed record JsonTaggedUnionOptions
    {
        private JsonTaggedUnionOptions(JsonNamingPolicy tagNamingPolicy) => TagNamingPolicy = tagNamingPolicy;

        public static JsonTaggedUnionOptions Default { get; } = new(tagNamingPolicy: new DefaultNamingPolicy());

        public JsonNamingPolicy TagNamingPolicy { get; init; }
    }
}
