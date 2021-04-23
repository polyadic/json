using System;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Polyadic.Json.NewType
{
    internal sealed class JsonNewTypeConverter<TNewType, TInner> : JsonConverter<TNewType>
        where TNewType : notnull
    {
        private readonly Func<TInner, TNewType> _constructor;
        private readonly Func<TNewType, TInner> _valueAccessor;
        private readonly JsonConverter<TInner>? _innerConverter;

        public JsonNewTypeConverter(NewTypeMetadata metadata)
        {
            _constructor = Lazy(() => CompileConstructor(metadata));
            _valueAccessor = Lazy(() => CompileValueAccessor(metadata));
            _innerConverter = metadata.InnerConverter as JsonConverter<TInner>;
        }

        public override TNewType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var innerValue = _innerConverter is { } converter
                ? converter.Read(ref reader, typeof(TInner), options)
                : JsonSerializer.Deserialize<TInner>(ref reader, options);
            return innerValue is { }
                ? _constructor(innerValue)
                : default;
        }

        public override void Write(Utf8JsonWriter writer, TNewType value, JsonSerializerOptions options)
        {
            if (_innerConverter is { } converter)
            {
                converter.Write(writer, _valueAccessor(value), options);
            }
            else
            {
                JsonSerializer.Serialize(writer, _valueAccessor(value), options);
            }
        }

        private static Func<TInner, TNewType> CompileConstructor(NewTypeMetadata metadata)
        {
            var valueParameter = Expression.Parameter(metadata.InnerType, "value");
            var expression = Expression.Lambda<Func<TInner, TNewType>>(
                Expression.New(metadata.Constructor, valueParameter),
                valueParameter);
            return expression.Compile();
        }

        private static Func<TNewType, TInner> CompileValueAccessor(NewTypeMetadata metadata)
        {
            var newTypeParameter = Expression.Parameter(typeof(TNewType), "newType");
            var expression = Expression.Lambda<Func<TNewType, TInner>>(
                Expression.Property(newTypeParameter, metadata.ValueProperty),
                newTypeParameter);
            return expression.Compile();
        }

        private static Func<T1, TResult> Lazy<T1, TResult>(Func<Func<T1, TResult>> compileFunc)
        {
            var lazy = new Lazy<Func<T1, TResult>>(compileFunc);
            return inner => lazy.Value(inner);
        }
    }
}
