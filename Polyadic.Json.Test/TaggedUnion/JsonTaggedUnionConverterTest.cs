using System.Text.Json;
using Polyadic.Json.TaggedUnion;
using Xunit;

namespace Polyadic.Json.Test.TaggedUnion
{
    public sealed class JsonTaggedUnionConverterTest
    {
        [Fact]
        public void DeserializingATaggedUnionFromJsonWhereTagIsNotStringThrows()
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EmailDelivery>("{ \"type\": 3 }", options));
        }

        [Fact]
        public void DeserializingATaggedUnionFromJsonWithMissingTagKeyThrows()
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EmailDelivery>(string.Empty, options));
        }

        [Fact]
        public void DeserializingATaggedUnionFromJsonWithInvalidTagThrows()
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EmailDelivery>("{ \"Type\": \"Custom\" }", options));
        }

        [Theory]
        [MemberData(nameof(DeserializesTaggedUnionWithDefaultOptionsData))]
        public void DeserializesTaggedUnionWithDefaultOptions(EmailDelivery expected, string json)
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Equal(expected, JsonSerializer.Deserialize<EmailDelivery>(json, options));
        }

        public static TheoryData<EmailDelivery, string> DeserializesTaggedUnionWithDefaultOptionsData()
            => new()
            {
                { new EmailDelivery.Null(), "{ \"Type\": \"Null\" }" },
                {
                    new EmailDelivery.Pickup("/tmp/email-pickup"),
                    "{ \"Type\": \"Pickup\", \"DirectoryPath\": \"/tmp/email-pickup\" }"
                },
                {
                    new EmailDelivery.SmtpServer("localhost", 25),
                    "{ \"Type\": \"SmtpServer\", \"Host\": \"localhost\", \"Port\": 25 }"
                },
            };

        [Theory]
        [MemberData(nameof(DeserializesTaggedUnionWithCustomTagNamingPolicyData))]
        public void DeserializesTaggedUnionWithCustomTagNamingPolicy(EmailDelivery expected, string json)
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter(
                JsonTaggedUnionOptions.Default with { TagNamingPolicy = JsonNamingPolicy.CamelCase });
            Assert.Equal(expected, JsonSerializer.Deserialize<EmailDelivery>(json, options));
        }

        public static TheoryData<EmailDelivery, string> DeserializesTaggedUnionWithCustomTagNamingPolicyData()
            => new()
            {
                { new EmailDelivery.Null(), "{ \"Type\": \"null\" }" },
                {
                    new EmailDelivery.Pickup("/tmp/email-pickup"),
                    "{ \"Type\": \"pickup\", \"DirectoryPath\": \"/tmp/email-pickup\" }"
                },
                {
                    new EmailDelivery.SmtpServer("localhost", 25),
                    "{ \"Type\": \"smtpServer\", \"Host\": \"localhost\", \"Port\": 25 }"
                },
            };

        [Theory]
        [MemberData(nameof(DeserializesTaggedUnionWithCustomPropertyNamingPolicyData))]
        public void DeserializesTaggedUnionWithCustomPropertyNamingPolicy(EmailDelivery expected, string json)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                .AddTaggedUnionConverter();
            Assert.Equal(expected, JsonSerializer.Deserialize<EmailDelivery>(json, options));
        }

        public static TheoryData<EmailDelivery, string> DeserializesTaggedUnionWithCustomPropertyNamingPolicyData()
            => new()
            {
                { new EmailDelivery.Null(), "{ \"type\": \"Null\" }" },
                {
                    new EmailDelivery.Pickup("/tmp/email-pickup"),
                    "{ \"type\": \"Pickup\", \"directoryPath\": \"/tmp/email-pickup\" }"
                },
                {
                    new EmailDelivery.SmtpServer("localhost", 25),
                    "{ \"type\": \"SmtpServer\", \"host\": \"localhost\", \"port\": 25 }"
                },
            };

        [Fact]
        public void NonAbstractUnionTypeCanBeDeserialized()
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Equal(
                new NonAbstractUnion.Variant(),
                JsonSerializer.Deserialize<NonAbstractUnion>("{ \"Type\": \"Variant\" }", options));
        }

        [Theory]
        [MemberData(nameof(DeserializesTaggedUnionWithMinimalAttributeConfigurationData))]
        public void DeserializesTaggedUnionWithMinimalAttributeConfiguration(UpdateMode expected, string json)
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Equal(expected, JsonSerializer.Deserialize<UpdateMode>(json, options));
        }

        [Theory]
        [InlineData("{ \"Type\": \"" + nameof(UnionWithInvalidNestedTypes.Abstract) + "\" }")]
        [InlineData("{ \"Type\": \"" + nameof(UnionWithInvalidNestedTypes.Detached) + "\" }")]
        public void DeserializingInvalidNestedTypeDoesNotWork(string json)
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UnionWithInvalidNestedTypes>(json, options));
        }

        [Fact]
        public void DeserializingVariantsDirectlyWorksWithoutDiscriminator()
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            const string json = "{ \"ChannelName\": \"beta\" }";
            Assert.Equal(new UpdateMode.Latest("beta"), JsonSerializer.Deserialize<UpdateMode.Latest>(json, options));
        }

        [Fact]
        public void DeserializingVariantsDirectlyIgnoresNonMatchingDiscriminator()
        {
            var options = new JsonSerializerOptions().AddTaggedUnionConverter();
            const string json = "{ \"Type\": \"Pinned\", \"ChannelName\": \"beta\" }";
            Assert.Equal(new UpdateMode.Latest("beta"), JsonSerializer.Deserialize<UpdateMode.Latest>(json, options));
        }

        public static TheoryData<UpdateMode, string> DeserializesTaggedUnionWithMinimalAttributeConfigurationData()
            => new()
            {
                { new UpdateMode.Auto(), "{ \"Type\": \"Auto\" }" },
                {
                    new UpdateMode.Pinned("1.4.2"),
                    "{ \"Type\": \"Pinned\", \"Version\": \"1.4.2\" }"
                },
                {
                    new UpdateMode.Latest("beta"),
                    "{ \"Type\": \"Latest\", \"ChannelName\": \"beta\" }"
                },
            };

        [JsonTaggedUnion(Tag = "Type")]
        public abstract record EmailDelivery
        {
            [JsonTaggedUnionVariantName(nameof(Null))]
            public sealed record Null : EmailDelivery
            {
            }

            [JsonTaggedUnionVariantName(nameof(Pickup))]
            public sealed record Pickup(string DirectoryPath) : EmailDelivery
            {
            }

            [JsonTaggedUnionVariantName(nameof(SmtpServer))]
            public sealed record SmtpServer(string Host, int Port) : EmailDelivery
            {
            }
        }

        [JsonTaggedUnion(Tag = "Type")]
        public record NonAbstractUnion
        {
            public sealed record Variant : NonAbstractUnion;
        }

        [JsonTaggedUnion]
        public abstract record UpdateMode
        {
            public sealed record Auto : UpdateMode;

            public sealed record Pinned(string Version) : UpdateMode;

            public sealed record Latest(string ChannelName) : UpdateMode;
        }

        [JsonTaggedUnion]
        public abstract record UnionWithInvalidNestedTypes
        {
            public enum Enumeration
            {
            }

            public abstract record Abstract : UnionWithInvalidNestedTypes;

            public sealed record Detached;
        }
    }
}
