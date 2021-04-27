using System;
using System.Collections.Generic;

namespace Polyadic.Json.TaggedUnion
{
    internal sealed record TaggedUnionMetadata(
        Type UnionType,
        string TagKey,
        IReadOnlyDictionary<string, TaggedUnionVariantMetadata> VariantsByTag);
}
