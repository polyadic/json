using System;

namespace Polyadic.Json.TaggedUnion
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class JsonTaggedUnionAttribute : Attribute
    {
        public string? Tag { get; init; }
    }
}
