using System;
using System.Text.Json.Serialization;

namespace Polyadic.Json.TaggedUnion
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class JsonTaggedUnionVariantNameAttribute : JsonAttribute
    {
        public JsonTaggedUnionVariantNameAttribute(string name) => Name = name;

        public string Name { get; }
    }
}
