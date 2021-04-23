using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Polyadic.Json.NewType
{
    internal sealed record NewTypeMetadata(
        Type InnerType,
        JsonConverter? InnerConverter,
        ConstructorInfo Constructor,
        PropertyInfo ValueProperty);
}
