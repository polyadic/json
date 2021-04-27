using System.Text.Json;

namespace Polyadic.Json.TaggedUnion
{
    internal sealed class DefaultNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name;
    }
}
