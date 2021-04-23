using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Polyadic.Json.NewType;
using Xunit;

namespace Polyadic.Json.Test.NewType
{
    public sealed class JsonNewTypeConverterTest
    {
        [Theory]
        [InlineData(typeof(NewTypeWithNoConstructors))]
        [InlineData(typeof(NewTypeWithNoSuitableConstructors))]
        [InlineData(typeof(NewTypeWithMoreThanOneSuitableConstructor))]
        [InlineData(typeof(NewTypeWithMoreThanOneMarkedConstructors))]
        [InlineData(typeof(NewTypeWithUnsuitableMarkedConstructor))]
        [InlineData(typeof(ValueTypeNewTypeWithNoConstructors))]
        public void GetConverter_NewTypeWithNoSuitableConstructor_ThrowsException(Type typeToConvert)
        {
            var options = new JsonSerializerOptions().AddNewTypeConverter();
            Assert.Throws<JsonException>(() => options.GetConverter(typeToConvert));
        }

        [Theory]
        [MemberData(nameof(DeserializesNewTypesWithSuitableConstructorsData))]
        public void Deserialize_NewTypesWithSuitableConstructors_ReturnsExpectedValue(string json, object expectedValue)
        {
            var options = new JsonSerializerOptions().AddNewTypeConverter();
            var value = JsonSerializer.Deserialize(json, expectedValue.GetType(), options);
            Assert.Equal(expectedValue, value);
        }

        [Fact]
        public void Deserialize_Null_ReturnsNullNewType()
        {
            var options = new JsonSerializerOptions().AddNewTypeConverter();
            var value = JsonSerializer.Deserialize<GenericNewType<string>>("null", options);
            Assert.Null(value);
        }

        [Fact]
        public void Serialize_NullNewType_ReturnsNull()
        {
            var options = new JsonSerializerOptions().AddNewTypeConverter();
            var value = JsonSerializer.Serialize<GenericNewType<string>?>(null, options);
            Assert.Equal("null", value);
        }

        [Fact]
        public void Serialize_NullInnerValue_ReturnsNull()
        {
            var options = new JsonSerializerOptions().AddNewTypeConverter();
            var value = JsonSerializer.Serialize(new GenericNewType<string?>(null), options);
            Assert.Equal("null", value);
        }

        [Theory]
        [MemberData(nameof(DeserializesNewTypesWithSuitableConstructorsData))]
        public void Serialize_NewTypesWithSuitableConstructors_ReturnsExpectedValue(string expectedJson, object value)
        {
            var options = new JsonSerializerOptions().AddNewTypeConverter();
            var json = JsonSerializer.Serialize(value, options);
            Assert.Equal(expectedJson, json);
        }

        public static TheoryData<string, object> DeserializesNewTypesWithSuitableConstructorsData()
            => new()
            {
                {
                    "\"foo bar\"",
                    new NewTypeWithOneSuitableConstructor("foo bar")
                },
                {
                    "\"foo bar\"",
                    new NewTypeWithOneSuitableConstructorAndNonSuitableConstructors("foo bar")
                },
                {
                    "\"foo bar\"",
                    new NewTypeWithMultipleSuitableConstructorsAndOneMarked("foo bar")
                },
                {
                    "\"foo bar\"",
                    new GenericNewType<string>("foo bar")
                },
                {
                    "42",
                    new GenericNewType<long>(42)
                },
                {
                    "42",
                    new GenericNewType<GenericNewType<GenericNewType<long>>>(new GenericNewType<GenericNewType<long>>(new GenericNewType<long>(42)))
                },
                {
                    $"{{\"{nameof(Person.FirstName)}\":\"Peter\",\"{nameof(Person.LastName)}\":\"Pan\"}}",
                    new GenericNewType<Person>(new Person("Peter", "Pan"))
                },
                {
                    "\"foo bar\"",
                    new ValueTypeNewType("foo bar")
                },
            };

        [JsonNewType]
        private readonly struct ValueTypeNewType
        {
            public ValueTypeNewType(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [JsonNewType]
        private readonly struct ValueTypeNewTypeWithNoConstructors
        {
            public string Value => throw new NotImplementedException();
        }

        [JsonNewType]
        private sealed record NewTypeWithNoConstructors;

        [JsonNewType]
        private sealed class NewTypeWithNoSuitableConstructors
        {
            public NewTypeWithNoSuitableConstructors()
            {
            }

            public NewTypeWithNoSuitableConstructors(string foo, string bar)
            {
            }

            public NewTypeWithNoSuitableConstructors(string foo, string bar, string baz)
            {
            }

            public string Value => throw new NotImplementedException();
        }

        [JsonNewType]
        private sealed record NewTypeWithOneSuitableConstructor
        {
            public NewTypeWithOneSuitableConstructor(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [JsonNewType]
        private sealed record NewTypeWithOneSuitableConstructorAndNonSuitableConstructors
        {
            public NewTypeWithOneSuitableConstructorAndNonSuitableConstructors(string value)
            {
                Value = value;
            }

            public NewTypeWithOneSuitableConstructorAndNonSuitableConstructors(string value, string bar, string baz)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [JsonNewType]
        private sealed record NewTypeWithMultipleSuitableConstructorsAndOneMarked
        {
            [JsonConstructor]
            public NewTypeWithMultipleSuitableConstructorsAndOneMarked(string value)
            {
                Value = value;
            }

            public NewTypeWithMultipleSuitableConstructorsAndOneMarked(int bar, string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        [JsonNewType]
        private sealed class NewTypeWithMoreThanOneSuitableConstructor
        {
            public NewTypeWithMoreThanOneSuitableConstructor(string foo)
            {
            }

            public NewTypeWithMoreThanOneSuitableConstructor(int bar)
            {
            }

            public string Value => throw new NotImplementedException();
        }

        [JsonNewType]
        private sealed class NewTypeWithMoreThanOneMarkedConstructors
        {
            [JsonConstructor]
            public NewTypeWithMoreThanOneMarkedConstructors(string foo)
            {
            }

            [JsonConstructor]
            public NewTypeWithMoreThanOneMarkedConstructors(int bar)
            {
            }

            public string Value => throw new NotImplementedException();
        }

        [JsonNewType]
        private sealed class NewTypeWithUnsuitableMarkedConstructor
        {
            [JsonConstructor]
            public NewTypeWithUnsuitableMarkedConstructor(string foo, string bar)
            {
            }

            public string Value => throw new NotImplementedException();
        }

        [JsonNewType]
        private sealed record GenericNewType<T>
        {
            public GenericNewType(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }

        private record Person(string FirstName, string LastName);
    }
}
