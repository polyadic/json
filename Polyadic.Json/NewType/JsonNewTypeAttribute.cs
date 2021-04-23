using System;
using System.Text.Json.Serialization;

namespace Polyadic.Json.NewType
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class JsonNewTypeAttribute : JsonConverterAttribute
    {
        public override JsonConverter CreateConverter(Type typeToConvert) => new JsonNewTypeJsonConverterFactory();
    }
}
