using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyadic.Json.TaggedUnion
{
    internal sealed class JsonTaggedUnionConverter<T> : JsonConverter<T>
    {
        private readonly TaggedUnionMetadata _metadata;

        public JsonTaggedUnionConverter(TaggedUnionMetadata metadata) => _metadata = metadata;

        private delegate T? VariantReader(ref Utf8JsonReader reader, JsonConverter converter, JsonSerializerOptions options);

        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var (variantType, _, converter) = PeekReadVariant(ref reader);
            var variantReader = CreateVariantReader(variantType);
            return variantReader(ref reader, converter ?? options.GetConverter(variantType), options);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) => throw new NotImplementedException();

        private TaggedUnionVariantMetadata PeekReadVariant(ref Utf8JsonReader reader)
            => PeekReadVariantName(reader) is var variantName
               && _metadata.VariantsByTag.TryGetValue(PeekReadVariantName(reader), out var variant)
                 ? variant
                 : throw new JsonException($"Invalid union variant '{variantName}'");

        private string PeekReadVariantName(Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            var propertyName = reader.GetString();
            if (propertyName != _metadata.TagKey)
            {
                throw new JsonException();
            }

            reader.Read();
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException();
            }

            return reader.GetString()!;
        }

        private static VariantReader CreateVariantReader(Type variantType)
            => typeof(JsonTaggedUnionConverter<T>)
                .GetMethod(nameof(ReadVariant), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(variantType)
                .CreateDelegate<VariantReader>();

        private static T? ReadVariant<TVariant>(ref Utf8JsonReader reader, JsonConverter converter, JsonSerializerOptions options)
            where TVariant : T
            => ((JsonConverter<TVariant>)converter).Read(ref reader, typeof(TVariant), options);
    }
}
