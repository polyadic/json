using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyadic.Json.TaggedUnion
{
    internal sealed class JsonTaggedUnionConverterFactory : JsonConverterFactory
    {
        private const string DefaultTagKey = "Type";

        private readonly JsonTaggedUnionOptions _taggedUnionOptions;

        public JsonTaggedUnionConverterFactory(JsonTaggedUnionOptions taggedUnionOptions)
            => _taggedUnionOptions = taggedUnionOptions;

        public override bool CanConvert(Type type)
            => Attribute.IsDefined(type, attributeType: typeof(JsonTaggedUnionAttribute));

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var variantsMetadata = GetVariants(typeToConvert)
                .Select(variantType => CreateVariantMetadata(options, variantType))
                .ToImmutableDictionary(v => v.Tag);

            var metadata = new TaggedUnionMetadata(
                UnionType: typeToConvert,
                TagKey: GetTagKey(typeToConvert, options),
                VariantsByTag: variantsMetadata);

            return (JsonConverter)Activator.CreateInstance(
                typeof(JsonTaggedUnionConverter<>).MakeGenericType(typeToConvert),
                metadata)!;
        }

        private static IEnumerable<Type> GetVariants(Type typeToConvert)
            => typeToConvert.GetNestedTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.BaseType == typeToConvert);

        private TaggedUnionVariantMetadata CreateVariantMetadata(JsonSerializerOptions options, Type variantType)
        {
            var tag = variantType.GetCustomAttribute<JsonTaggedUnionVariantNameAttribute>()?.Name ?? variantType.Name;
            var tagAdjustedToNaming = _taggedUnionOptions.TagNamingPolicy.ConvertName(tag);
            return new TaggedUnionVariantMetadata(
                VariantType: variantType,
                Converter: options.GetConverter(variantType),
                Tag: tagAdjustedToNaming);
        }

        private static string GetTagKey(Type typeToConvert, JsonSerializerOptions options)
        {
            var taggedUnionAttribute = typeToConvert.GetCustomAttribute<JsonTaggedUnionAttribute>()!;
            var tagKey = taggedUnionAttribute.Tag ?? DefaultTagKey;
            return options.PropertyNamingPolicy is { } policy ? policy.ConvertName(tagKey) : tagKey;
        }
    }
}
