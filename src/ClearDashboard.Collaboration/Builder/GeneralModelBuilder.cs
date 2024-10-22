﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

/// <summary>
/// Model builders:
///     - Extract data from the database
///     - Change it to a externalizable/transferrable form (converting ids to system-neutral "refs" where necessary)
///     - Serialize/deserialize transferable form
/// </summary>
/// <typeparam name="T"></typeparam>
public class GeneralModelBuilder<T> : GeneralModelBuilder, IModelBuilder<T> where T : Models.IdentifiableEntity
{
    public const string HASH_SEPARATOR = "_";
    public const string ENCODE_DECODE_SEPARATOR = "||";

    public override string IdentityKey { get; init; } = typeof(T).GetIdentityProperty()!.Name;
    public override IEnumerable<PropertyInfo> PropertyInfos { get; init; } = typeof(T).GetProperties();
    public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes { get; init; } = new Dictionary<string, Type>();

    public virtual IEnumerable<GeneralModel<T>> BuildModelSnapshots(BuilderContext builderContext) =>
        throw new NotImplementedException($"{nameof(BuildModelSnapshots)} for model type {typeof(T).ShortDisplayName()}");

    protected static GeneralModel<T> ExtractUsingModelIds(object modelInstance, IEnumerable<String>? ignorePropertyNames = null)
    {
        return ExtractUsingModelIds<T>(modelInstance, ignorePropertyNames);
    }

    protected static GeneralModel<E> ExtractUsingModelIds<E>(object modelInstance, IEnumerable<String>? ignorePropertyNames = null)
        where E : Models.IdentifiableEntity
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

        foreach (PropertyInfo property in modelInstance.GetType().GetMappedPrimitiveProperties().OrderBy(p => p.Name))
        {
            if ((ignorePropertyNames?.Contains(property.Name) ?? false) ||
                property.Name == identityProperty?.Name ||
                foreignKeyPropertyNames.Contains(property.Name))
            {
                continue;
            }

            modelProperties.Add(property.Name, (property.PropertyType, property.GetValue(modelInstance, null)));
        }

        var generalModel = new GeneralModel<E>(identityPropertyName, identityPropertyValue);
        AddPropertyValuesToGeneralModel(generalModel, modelProperties);

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
                    modelProperties.Add(n!, (typeof(Guid?), modelInstance.GetType().GetProperty(n!)!.GetValue(modelInstance, null)));


                    //var foreignKeyId = (Guid)modelInstance.GetType().GetProperty(n)!.GetValue(modelInstance, null)!;

                    //if (builderContext.TryGetIdToIndexValue(modelPropertyType.Name, foreignKeyId, out var foreignKeyIndex))
                    //{
                    //    modelProperties.Add(BuildPropertyRefName(modelPropertyName), (typeof(string), BuildPropertyRefValue(modelPropertyType.Name, foreignKeyIndex)));
                    //}
                    //else
                    //{
                    //    modelProperties.Add(n, (typeof(Guid), foreignKeyId));
                    //}
                }
            }
        });

        foreach (PropertyInfo property in modelInstance.GetType().GetMappedPrimitiveProperties().OrderBy(p => p.Name))
        {
            if ((ignorePropertyNames?.Contains(property.Name) ?? false) ||
                foreignKeyPropertyNames.Contains(property.Name))
            {
                continue;
            }

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

        return modelProperties;
    }

    internal static void AddPropertyValuesToGeneralModel<E>(GeneralModel<E> generalModel, IDictionary<string, (Type type, object? value)> modelPropertiesTypes)
        where E : Models.IdentifiableEntity
    {
        foreach (var kvp in modelPropertiesTypes)
        {
            if (kvp.Value.type == typeof(string))
            {
                generalModel.Add(kvp.Key, (string?)kvp.Value.value, kvp.Value.type);
            }
            else if (kvp.Value.type.IsValueType)
            {
                generalModel.Add(kvp.Key, (ValueType?)kvp.Value.value, kvp.Value.type);
            }
            else if (kvp.Value.type.IsAssignableTo(typeof(ModelRef)))
            {
                generalModel.Add(kvp.Key, (ModelRef)kvp.Value.value!);
            }
            else if (kvp.Value.type.IsAssignableTo(typeof(ModelExtra)))
            {
                generalModel.Add(kvp.Key, (ModelExtra)kvp.Value.value!);
            }
            else if (kvp.Value.type.IsAssignableTo(typeof(IEnumerable<string>)))
            {
                generalModel.Add(kvp.Key, (IEnumerable<string>)kvp.Value.value!);
            }
            else if (kvp.Value.type.IsAssignableTo(typeof(IDictionary<string, object>)))
            {
                generalModel.Add(kvp.Key, (IDictionary<string, object>)kvp.Value.value!);
            }
			else if (kvp.Value.type.IsAssignableTo(typeof(IList<Models.Metadatum>)))
			{
				generalModel.Add(kvp.Key, (IList<Models.Metadatum>)kvp.Value.value!);
			}
			else
			{
                // Should never get here if caller already checked IsDatabasePrimitiveType
                throw new NotSupportedException($"Type {kvp.Value.type.Name} not supported in commit snapshot builder");
            }
        }
    }

    internal static string HashPartsToRef(string refPrefixNoUnderscore, params string?[] parts)
    {
        var joinedParts = string.Join(HASH_SEPARATOR, parts.Select(e => string.IsNullOrEmpty(e) ? string.Empty : e));
        return $"{refPrefixNoUnderscore}_{joinedParts.ToMD5String()}";
    }

    internal static string EncodePartsToRef(string refPrefixNoUnderscore, params string?[] parts)
    {
        var joinedParts = string.Join(ENCODE_DECODE_SEPARATOR, parts.Select(e => string.IsNullOrEmpty(e) ? string.Empty : e));
        var bytes = Encoding.UTF8.GetBytes(joinedParts);
        return $"{refPrefixNoUnderscore}_{Convert.ToBase64String(bytes)}";
    }

    internal static string[] DecodeRefToParts(string refPrefixNoUnderscore, string refValue, int expectedNumberOfParts)
    {
        var base64String = Regex.Replace(refValue, $@"^{refPrefixNoUnderscore}_", string.Empty);
        var decodedString = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

        var parts = decodedString.Split(ENCODE_DECODE_SEPARATOR);

        if (parts.Length != expectedNumberOfParts)
        {
			// Per issue https://github.com/Clear-Bible/ClearDashboard/issues/1321
			// First try to decode using the new ENCODE_DECODE_SEPARATOR delimiter. If the resulting number of parts
            // doesn't match what was expected, try the older "_" as the delimiter, and if neither match, throw the
            // PropertyResolutionException.
			parts = decodedString.Split('_');
        }

        if (parts.Length != expectedNumberOfParts)
        {
            throw new PropertyResolutionException($"Unable to decode {refPrefixNoUnderscore} Ref {refValue}");
        }

        return parts;
    }

    internal static GeneralModel<E> BuildRefModelSnapshot<E>(E dbModel, string refValue, string? idForFilesystem, IEnumerable<(string refPrefix, string? refValue, bool removeId)>? additionalRefValues, BuilderContext builderContext)
        where E : IdentifiableEntity
    {
        var modelSnapshotProperties = ExtractUsingModelRefs(
            dbModel,
            builderContext,
            builderContext.CommonIgnoreProperties.Union(new List<string>() { "Id" }));

        if (additionalRefValues is not null && additionalRefValues.Any())
        {
            foreach (var rv in additionalRefValues)
            {
                if (rv.removeId)
                    modelSnapshotProperties.Remove(rv.refPrefix + "Id");

                if (rv.refValue is not null)
                    modelSnapshotProperties.Add(BuildPropertyRefName(rv.refPrefix), (typeof(string), rv.refValue));
            }
        }

        var snapshot = new GeneralModel<E>(BuildPropertyRefName(), refValue, idForFilesystem);
        AddPropertyValuesToGeneralModel(snapshot, modelSnapshotProperties);

        return snapshot;
    }

    public static IModelBuilder<T> GetModelBuilder()
    {
        return GetModelBuilder<T>();
    }

    public override GeneralModel<T> BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes)
    {
        if (modelPropertiesTypes.TryGetValue(IdentityKey, out var identityPropertyValue))
        {
            if (identityPropertyValue.value is null)
            {
                throw new ArgumentException($"Invalid - GeneralModel identity key '{IdentityKey}' has a null value");
            }

            GeneralModel<T>? generalModel = null;

            if (identityPropertyValue.type == typeof(string))
                generalModel = new GeneralModel<T>(IdentityKey, (string)identityPropertyValue.value!);
            else if (identityPropertyValue.type.IsValueType)
                generalModel = new GeneralModel<T>(IdentityKey, (ValueType)identityPropertyValue.value!);
            else
                throw new ArgumentException($"Invalid GeneralModel identity key value type: '{identityPropertyValue.GetType().ShortDisplayName()}'");

            modelPropertiesTypes.Remove(IdentityKey);

            AddPropertyValuesToGeneralModel(generalModel, modelPropertiesTypes);
            return generalModel;
        }

        throw new ArgumentException($"Unable to build GeneralModel<{typeof(T).ShortDisplayName()}> due to missing value for IdentityKey '{IdentityKey}'");
    }

    public virtual void UpdateModelSnapshotFormat(ProjectSnapshot projectSnapshot, Dictionary<Type, Dictionary<Guid, Dictionary<string, string>>> updateMappings)
    {
        // NOOP
    }
}

public abstract class GeneralModelBuilder : IModelBuilder
{
    public const string REF = "Ref";

    public abstract string IdentityKey { get; init; }
    public abstract IEnumerable<PropertyInfo> PropertyInfos { get; init; }
    public abstract IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes { get; init; }
    public virtual IEnumerable<string> NoSerializePropertyNames { get; init; } = Enumerable.Empty<string>();

    public abstract GeneralModel BuildGeneralModel(Dictionary<string, (Type type, object? value)> modelPropertiesTypes);

    public static string BuildPropertyRefName(string propertyName = "") => $"{propertyName}{REF}";
    //internal static string BuildPropertyRefValue(string modelName, int index) => $"{modelName}_{index}";

    public static IModelBuilder<T> GetModelBuilder<T>() where T : Models.IdentifiableEntity
    {
        return (IModelBuilder<T>)GetGeneralModelBuilder(typeof(GeneralModel<T>));
    }

    protected static IReadOnlyDictionary<string, Type> ToPropertyRefNamesTypes(params string[] refPrefixes) =>
        refPrefixes.ToDictionary(e => BuildPropertyRefName(e), e => typeof(string));

    public static IModelBuilder GetGeneralModelBuilder(Type generalModelType)
    {
        return generalModelType switch
        {
            Type modelType when modelType == typeof(GeneralModel<Models.LabelGroupAssociation>) => 
                new GeneralModelBuilder<Models.LabelGroupAssociation>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty,  // Plain "Ref"
                        LabelGroupBuilder.LABELGROUP_REF_PREFIX,
                        LabelBuilder.LABEL_REF_PREFIX
                    )
                },
            Type modelType when modelType == typeof(GeneralModel<Models.LabelNoteAssociation>) => 
                new GeneralModelBuilder<Models.LabelNoteAssociation>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty,  // Plain "Ref"
                        LabelBuilder.LABEL_REF_PREFIX
                    )
                },
            Type modelType when modelType == typeof(GeneralModel<Models.NoteUserSeenAssociation>) => 
                new GeneralModelBuilder<Models.NoteUserSeenAssociation>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty   // Plain "Ref"
                    )
                },
            Type modelType when modelType == typeof(GeneralModel<Models.Lexicon_Lexeme>) => new LexiconBuilder(),
            Type modelType when modelType == typeof(GeneralModel<Models.Lexicon_Meaning>) => 
                new GeneralModelBuilder<Models.Lexicon_Meaning>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty,  // Plain "Ref"
                        LexiconBuilder.LEXEME_REF_PREFIX
                    )
                },
            Type modelType when modelType == typeof(GeneralModel<Models.Lexicon_Form>) => 
                new GeneralModelBuilder<Models.Lexicon_Form>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty,  // Plain "Ref"
                        LexiconBuilder.LEXEME_REF_PREFIX
                    )
                },
            Type modelType when modelType == typeof(GeneralModel<Models.Lexicon_Translation>) => 
                new GeneralModelBuilder<Models.Lexicon_Translation>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty,  // Plain "Ref"
                        LexiconBuilder.LEXEME_REF_PREFIX,
                        LexiconBuilder.MEANING_REF_PREFIX
                    )
                },
            Type modelType when modelType == typeof(GeneralModel<Models.Lexicon_SemanticDomain>) => new SemanticDomainBuilder(),
            Type modelType when modelType == typeof(GeneralModel<Models.Lexicon_SemanticDomainMeaningAssociation>) =>
                new GeneralModelBuilder<Models.Lexicon_SemanticDomainMeaningAssociation>
                {
                    IdentityKey = REF,
                    AddedPropertyNamesTypes = ToPropertyRefNamesTypes(
                        string.Empty,  // Plain "Ref"
                        SemanticDomainBuilder.SEMANTIC_DOMAIN_REF_PREFIX,
                        LexiconBuilder.MEANING_REF_PREFIX,
                        LexiconBuilder.LEXEME_REF_PREFIX
                    )
                },
            _ => BuildDefaultModelBuilder(generalModelType)
//            _ => throw new ArgumentOutOfRangeException(generalModelType.ShortDisplayName(), $"No IModelBuilder found for type argument")

        };
    }

    protected static IModelBuilder BuildDefaultModelBuilder(Type generalModelType)
    {
        var modelType = generalModelType.GetGenericArguments()[0];

        var candidate = modelType.Name + "Builder";
        var assembly = typeof(GeneralModelBuilder).Assembly;
        var builderType = assembly.GetType($"{typeof(GeneralModelBuilder).Namespace}.{candidate}");

        if (builderType is null || !builderType.IsAssignableTo(typeof(IModelBuilder)))
        {
            Type[] typeArgs = { modelType };
            builderType = typeof(GeneralModelBuilder<>).MakeGenericType(typeArgs);
        }

        object?[] constructorArgs = { };
        return (IModelBuilder)Activator.CreateInstance(builderType, constructorArgs)!;
    }
}
