using System;
using System.Text.Json.Serialization;

namespace Polyadic.Json.TaggedUnion
{
    internal sealed record TaggedUnionVariantMetadata(
        Type VariantType,
        string Tag,
        JsonConverter? Converter);
}
