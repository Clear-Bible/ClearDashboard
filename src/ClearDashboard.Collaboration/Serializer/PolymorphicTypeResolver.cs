using System;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;

namespace ClearDashboard.Collaboration.Serializer;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        if (jsonTypeInfo.Type == typeof(ModelRef))
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                        {
                            new JsonDerivedType(typeof(TokenRef), nameof(TokenRef)),
                            new JsonDerivedType(typeof(AlignmentRef), nameof(AlignmentRef)),
                            new JsonDerivedType(typeof(TranslationRef), nameof(TranslationRef)),
                            new JsonDerivedType(typeof(DomainEntityRef), nameof(DomainEntityRef))
                        }
            };
        }

        return jsonTypeInfo;
    }
}
