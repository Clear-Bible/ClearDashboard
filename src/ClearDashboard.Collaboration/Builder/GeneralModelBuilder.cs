using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

/// <summary>
/// Model builders:
///     - Extract data from the database
///     - Change it to a externalizable/transferrable form (converting ids to system-neutral "refs" where necessary)
///     - Serialize/deserialize transferable form
/// </summary>
/// <typeparam name="T"></typeparam>
public class GeneralModelBuilder<T> where T: Models.IdentifiableEntity
{
    protected static GeneralModel<T> ExtractUsingModelIds(object modelInstance, IEnumerable<String>? ignorePropertyNames = null)
    {
        var identityPropertyName = string.Empty;
        Guid? identityPropertyValue = null;
        var modelProperties = new Dictionary<string, (Type type, object? value)>();

        var identityProperty = modelInstance.GetType().GetIdentityProperty();

        if (identityProperty is not null && (!ignorePropertyNames?.Contains(identityProperty.Name) ?? true))
        {
            identityPropertyName = identityProperty.Name;
            identityPropertyValue = (Guid?)identityProperty.GetValue(modelInstance, null);
        }
        else
        {
            throw new BuilderEntityException($"Unable to determine identity property for incoming model instance of type {typeof(T).GetType().Name}");
        }

        if (identityPropertyValue is null)
        {
            throw new BuilderEntityException($"Missing identity property value for incoming model instance of type {typeof(T).GetType().Name}");
        }

        var foreignKeyPropertyNames = modelInstance.GetType().GetForeignKeyProperties()
            .Select(p => p.Name)
            .Where(n => ignorePropertyNames == null || !ignorePropertyNames!.Contains(n))
            .ToList();

        foreignKeyPropertyNames.ForEach(n =>
        {
            modelProperties.Add(n!, (typeof(Guid?), modelInstance.GetType().GetProperty(n!)!.GetValue(modelInstance, null)));
        });

        foreach (PropertyInfo property in modelInstance.GetType().GetProperties().OrderBy(p => p.Name))
        {
            if ((ignorePropertyNames?.Contains(property.Name) ?? false) ||
                property.Name == identityProperty?.Name ||
                foreignKeyPropertyNames.Contains(property.Name))
            {
                continue;
            }

            if (property.PropertyType.IsDatabasePrimitiveType())
            {
                modelProperties.Add(property.Name, (property.PropertyType, property.GetValue(modelInstance, null)));
            }
        }

        var generalModel = new GeneralModel<T>(identityPropertyName, identityPropertyValue);
        AddPropertyValuesToGenericModel(generalModel, modelProperties);

        return generalModel;
    }

    protected static Dictionary<string, (Type type, object? value)> ExtractUsingModelRefs(object modelInstance, BuilderContext builderContext, IEnumerable<String>? ignorePropertyNames = null, GeneralModel<T>? genericModel = null)
    {
        var modelInstanceTypeName = modelInstance.GetType().Name;
        var modelProperties = new Dictionary<string, (Type type, object? value)>();

        var identityProperty = modelInstance.GetType().GetIdentityProperty();

        var foreignKeyPropertyNames = modelInstance.GetType().GetForeignKeyProperties()
            .Select(p => p.Name)
            .Where(n => ignorePropertyNames == null || !ignorePropertyNames!.Contains(n))
            .ToList();

        foreignKeyPropertyNames.ForEach(n =>
        {
            var modelPropertyName = n!.Substring(0, n.Length - 2); // Remove "Id" suffix
            var modelPropertyType = modelInstance.GetType().GetProperty(modelPropertyName)?.PropertyType;

            if (modelPropertyType is not null)
            {
                // Since a TokenComponent can be in idIndexMappings as TokenComponent, Token or TokenComposite
                // (or not at all), for simplicity we just always create a TokenRef instead:
                if (modelPropertyType.IsAssignableTo(typeof(Models.TokenComponent)))
                {
                    var tokenComponent = modelInstance.GetType().GetProperty(modelPropertyName)?.GetValue(modelInstance, null);
                    if (tokenComponent is not null)
                    {
                        var propertyRefName = BuildPropertyRefName(modelPropertyName);
                        modelProperties.Add(propertyRefName, (typeof(TokenRef), TokenBuilder.BuildTokenRef((Models.TokenComponent)tokenComponent, builderContext)));
                    }
                    else
                    {
                        throw new BuilderEntityException($"Property {modelPropertyName} of model instance (of type: {modelInstanceTypeName}) does not exist or is empty.");
                    }
                }
                else
                {
                    var foreignKeyId = (Guid)modelInstance.GetType().GetProperty(n)!.GetValue(modelInstance, null)!;

                    if (builderContext.TryGetIdToIndexValue(modelPropertyType.Name, foreignKeyId, out var foreignKeyIndex))
                    {
                        modelProperties.Add(BuildPropertyRefName(modelPropertyName), (typeof(string), BuildPropertyRefValue(modelPropertyType.Name, foreignKeyIndex)));
                    }
                    else
                    {
                        modelProperties.Add(n, (typeof(Guid), foreignKeyId));
                    }
                }
            }
        });

        foreach (PropertyInfo property in modelInstance.GetType().GetProperties().OrderBy(p => p.Name))
        {
            if ((ignorePropertyNames?.Contains(property.Name) ?? false) ||
                foreignKeyPropertyNames.Contains(property.Name))
            {
                continue;
            }

            if (property.PropertyType.IsDatabasePrimitiveType())
            {
                if (property.PropertyType == typeof(Guid) || property.PropertyType == typeof(Guid?))
                {
                    var guidValue = property.GetValue(modelInstance, null);
                    if (guidValue is not null && property.Name == identityProperty?.Name)
                    {
                        builderContext.UpsertIdToIndexValue(modelInstanceTypeName, (Guid)guidValue);
                        continue;
                    }
                }

                modelProperties.Add(property.Name, (property.PropertyType, property.GetValue(modelInstance, null)));
            }
        }

        return modelProperties;
    }

    protected static void AddPropertyValuesToGenericModel(GeneralModel<T> genericModel, Dictionary<string, (Type type, object? value)> modelProperties)
    {
        foreach (var kvp in modelProperties)
        {
            if (kvp.Value.type == typeof(string))
            {
                genericModel.Add(kvp.Key, (string?)kvp.Value.value);
            }
            else if (kvp.Value.type.IsValueType)
            {
                genericModel.Add(kvp.Key, (ValueType?)kvp.Value.value);
            }
            else if (kvp.Value.type.IsAssignableTo(typeof(ModelRef)))
            {
                genericModel.Add(kvp.Key, (ModelRef)kvp.Value.value!);
            }
            else if (kvp.Value.type.IsAssignableTo(typeof(IEnumerable<string>)))
            {
                genericModel.Add(kvp.Key, (IEnumerable<string>)kvp.Value.value!);
            }
            else
            {
                // Should never get here if caller already checked IsDatabasePrimitiveType
                throw new NotSupportedException($"Type {kvp.Value.type.Name} not supported in commit snapshot builder");
            }
        }
    }

    protected static string BuildPropertyRefName(string propertyName = "") => $"{propertyName}Ref";
    protected static string BuildPropertyRefValue(string modelName, int index) => $"{modelName}_{index}";
}

