using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.DifferenceModel;

namespace ClearDashboard.Collaboration.Serializer;

public class DifferenceJsonConverter : JsonConverter<IDifference>
{
    public override bool CanConvert(Type objectType)
    {
        var canConvert =
            objectType == typeof(PropertyDifference) ||
            objectType.IsAssignableTo(typeof(IListDifference)) ||
            objectType.IsAssignableToGenericType(typeof(ListMembershipDifference<>)) ||
            objectType.IsAssignableToGenericType(typeof(IModelDifference<>)) ||
            objectType.IsAssignableToGenericType(typeof(ValueDifference<>));

        return canConvert;
    }

    public override IDifference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // So far, this implementation is just to use the serialized form of these
        // objects to help visualize the difference data.  So, no need to deserialize...yet
        throw new NotSupportedException();
    }

    public override void Write(Utf8JsonWriter writer, IDifference value, JsonSerializerOptions options)
    {
        if (value.GetType().IsAssignableToGenericType(typeof(IListDifference<>)))
        {
            var listDifference = (IListDifference)value;

            writer.WriteStartObject();
            if (listDifference.HasMembershipDifferences)
            {
                var listMembershipDifferenceName = nameof(ListDifference<string>.ListMembershipDifference);
                var listMembershipDifferenceValue = value.GetType().GetProperty(listMembershipDifferenceName)!.GetValue(value, null)!;

                writer.WritePropertyName(listMembershipDifferenceName);
                JsonSerializer.Serialize(writer, listMembershipDifferenceValue, listMembershipDifferenceValue.GetType(), options);
            }
            if (listDifference.HasListMemberModelDifferences)
            {
                var listMemberModelDifferencesName = nameof(ListDifference<string>.ListMemberModelDifferences);
                var listMemberModelDifferencesValue = value.GetType().GetProperty(listMemberModelDifferencesName)!.GetValue(value, null)!;

                writer.WritePropertyName(listMemberModelDifferencesName);
                JsonSerializer.Serialize(writer, listMemberModelDifferencesValue, listMemberModelDifferencesValue.GetType(), options);
            }
            writer.WriteEndObject();
        }
        else if (value.GetType().IsAssignableToGenericType(typeof(IModelDifference<>)))
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(ModelDifference.ModelType), ((ModelDifference)value).ModelType.ShortDisplayName());
            writer.WriteString(nameof(ModelDifference.Id), ((ModelDifference)value).Id?.ToString());
            if (((ModelDifference)value).PropertyDifferences.Any())
            {
                writer.WritePropertyName(nameof(ModelDifference.PropertyDifferences));
                JsonSerializer.Serialize(writer, ((ModelDifference)value).PropertyDifferences, ((ModelDifference)value).PropertyDifferences.GetType(), options);
            }
            if (((ModelDifference)value).ChildListDifferences.Any())
            {
                writer.WritePropertyName(nameof(ModelDifference.ChildListDifferences));
                JsonSerializer.Serialize(writer, ((ModelDifference)value).ChildListDifferences, ((ModelDifference)value).ChildListDifferences.GetType(), options);
            }
            writer.WriteEndObject();
        }
        else if (value.GetType().IsAssignableTo(typeof(PropertyDifference)))
        {
            writer.WriteStartObject();
            writer.WriteString(nameof(PropertyDifference.PropertyName), ((PropertyDifference)value).PropertyName);
            writer.WritePropertyName(nameof(PropertyDifference.PropertyValueDifference));
            JsonSerializer.Serialize(writer, ((PropertyDifference)value).PropertyValueDifference, ((PropertyDifference)value).PropertyValueDifference.GetType(), options);
            writer.WriteEndObject();
        }
        else if (value.GetType().IsAssignableToGenericType(typeof(ValueDifference<>)))
        {
            writer.WriteStartObject();
            var valueType = value.GetType().GetGenericArguments()[0];
            var value1Name = nameof(ValueDifference<string>.Value1);
            var value2Name = nameof(ValueDifference<string>.Value2);

            var value1Value = value.GetType().GetProperty(value1Name)!.GetValue(value, null);
            var value2Value = value.GetType().GetProperty(value2Name)!.GetValue(value, null);

            writer.WritePropertyName(value1Name);
            JsonSerializer.Serialize(writer, value1Value, options);
            writer.WritePropertyName(value2Name);
            JsonSerializer.Serialize(writer, value2Value, options);
            writer.WriteString("$type", valueType.IsGenericType ? valueType.ShortDisplayName() : valueType.FullName ?? valueType.Name);

            writer.WriteEndObject();
        }
        else if (value.GetType().IsAssignableToGenericType(typeof(ListMembershipDifference<>)))
        {
            writer.WriteStartObject();

            var valueType = value.GetType().GetGenericArguments()[0];
            var onlyIn1Name = nameof(ListMembershipDifference<string>.OnlyIn1);
            var onlyIn2Name = nameof(ListMembershipDifference<string>.OnlyIn2);

            var onlyIn1Value = value.GetType().GetProperty(onlyIn1Name)!.GetValue(value, null)!;
            var onlyIn2Value = value.GetType().GetProperty(onlyIn2Name)!.GetValue(value, null)!;

            writer.WriteStartArray(onlyIn1Name);
            foreach (var item in (IEnumerable)onlyIn1Value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
            writer.WriteStartArray(onlyIn2Name);
            foreach (var item in (IEnumerable)onlyIn2Value)
            {
                JsonSerializer.Serialize(writer, item, options);
            }
            writer.WriteEndArray();
            writer.WriteString("$type", valueType.IsGenericType ? valueType.ShortDisplayName() : valueType.FullName ?? valueType.Name);

            writer.WriteEndObject();
        }
    }
}