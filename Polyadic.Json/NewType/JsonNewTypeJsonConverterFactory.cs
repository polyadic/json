using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyadic.Json.NewType
{
    internal sealed class JsonNewTypeJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert) => Attribute.IsDefined(typeToConvert, typeof(JsonNewTypeAttribute));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var metadata = GetMetadata(typeToConvert, options);
            return (JsonConverter)Activator.CreateInstance(
                typeof(JsonNewTypeConverter<,>).MakeGenericType(typeToConvert, metadata.InnerType),
                metadata)!;
        }

        private static NewTypeMetadata GetMetadata(Type typeToConvert, JsonSerializerOptions options)
        {
            var constructor = FindConstructor(typeToConvert);
            var innerType = constructor.GetParameters().First().ParameterType;
            var converter = options.GetConverter(innerType);
            var valueProperty = FindValueProperty(typeToConvert, innerType);
            return new NewTypeMetadata(innerType, converter, constructor, valueProperty);
        }

        private static ConstructorInfo FindConstructor(Type typeToConvert)
        {
            var constructorCandidates = typeToConvert.GetConstructors().Where(IsUnary).ToImmutableArray();
            var markedConstructors = constructorCandidates.Where(IsMarkedAsJsonConstructor).ToImmutableArray();
            return (markedConstructors.Length, constructorCandidates.Length) switch
            {
                (0, 0) => throw new JsonException($"No suitable constructors found for type '{typeToConvert}'. There must be at least one constructor with one parameter"),
                (>1, _) => throw new JsonException($"Multiple constructors of type '{typeToConvert}' are marked with [JsonConstructor]"),
                (0, >1) => throw new JsonException($"Multiple suitable constructors found for type '{typeToConvert}'. Choose one with [JsonConstructor]"),
                (1, _) => markedConstructors.First(),
                (_, 1) => constructorCandidates.First(),
                _ => throw new InvalidOperationException(),
            };
        }

        private static PropertyInfo FindValueProperty(Type typeToConvert, Type innerType)
        {
            var propertyCandidates = typeToConvert.GetProperties().Where(p => p.PropertyType == innerType).ToImmutableArray();
            var markedProperties = propertyCandidates.Where(IsMarkedAsJsonNewTypeValue).ToImmutableArray();
            return (markedProperties.Length, propertyCandidates.Length) switch
            {
                (0, 0) => throw new JsonException($"No suitable value property found for type '{typeToConvert}'. There must be at least one property"),
                (>1, _) => throw new JsonException($"Multiple properties of type '{typeToConvert}' are marked with [JsonNewTypeValueProperty]"),
                (0, >1) => throw new JsonException($"Multiple properties found for type '{typeToConvert}'. Choose one with [JsonNewTypeValueProperty]"),
                (1, _) => markedProperties.First(),
                (_, 1) => propertyCandidates.First(),
                _ => throw new InvalidOperationException(),
            };
        }

        private static bool IsMarkedAsJsonNewTypeValue(PropertyInfo property) => Attribute.IsDefined(property, typeof(JsonNewTypeValueAttribute));

        private static bool IsMarkedAsJsonConstructor(ConstructorInfo constructor) => Attribute.IsDefined(constructor, typeof(JsonConstructorAttribute));

        private static bool IsUnary(MethodBase method) => method.GetParameters().Length == 1;
    }
}
