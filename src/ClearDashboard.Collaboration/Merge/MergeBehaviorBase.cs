using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;
using System.Text.Json;
using ClearBible.Engine.Utils;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using SIL.EventsAndDelegates;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public delegate Task ProjectDbContextMergeQueryAsync(ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken = default);
public delegate object? EntityValueResolver(IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger);

public abstract class MergeBehaviorBase : IDisposable, IAsyncDisposable
{
    public MergeCache MergeCache { get; private set; }
    public IProgress<ProgressStatus> Progress { get; private set; }

    protected readonly ILogger _logger;
    protected readonly DateTimeOffsetToBinaryConverter _dateTimeOffsetToBinary;
    protected readonly NullabilityInfoContext _nullabilityContext;

    protected readonly Dictionary<
        Type,
        (Type TableEntityType, string DiscriminatorColumnName, string DiscriminatorColumnValue)
    > _discriminatorMappings = new();

    protected readonly Dictionary<(Type EntityType, string EntityPropertyName), EntityValueResolver> _entityValueResolvers = new();
    protected readonly Dictionary<(Type EntityType, string PropertyName), IEnumerable<string>> _propertyNameMap = new();
    protected readonly Dictionary<(Type EntityType, string PropertyName), IEnumerable<string>> _idPropertyNameMap = new();
    protected readonly Dictionary<(Type EntityType, string EntityPropertyName), string> _idReversePropertyNameMap = new();

    public MergeBehaviorBase(ILogger logger, MergeCache mergeCache, IProgress<ProgressStatus> progress)
    {
        MergeCache = mergeCache;
        Progress = progress;
        _logger = logger;
        _dateTimeOffsetToBinary = new DateTimeOffsetToBinaryConverter();
        _nullabilityContext = new NullabilityInfoContext();
    }

    public void AddEntityTypeDiscriminatorMapping(
        Type entityType,
        (Type TableEntityType, string DiscriminatorColumnName, string DiscriminatorColumnValue) discriminatorMapping)
    {
        _discriminatorMappings.Add(entityType, discriminatorMapping);
    }

    public void AddEntityValueResolver((Type EntityType, string EntityPropertyName) key, EntityValueResolver entityValueResolver)
    {
        _entityValueResolvers.Add(key, entityValueResolver);
    }

    public void AddPropertyNameMapping((Type EntityType, string PropertyName) key, IEnumerable<string> entityPropertyNames)
    {
        _propertyNameMap.Add(key, entityPropertyNames);
    }

    public void AddIdPropertyNameMapping((Type EntityType, string PropertyName) key, IEnumerable<string> entityPropertyNames)
    {
        _idPropertyNameMap.Add(key, entityPropertyNames);
    }

    public void AddIdReversePropertyNameMapping((Type EntityType, string EntityPropertyName) key, string propertyName)
    {
        _idReversePropertyNameMap.Add(key, propertyName);
    }

    public virtual async Task MergeStartAsync(CancellationToken cancellationToken) { await Task.CompletedTask; }
    public virtual async Task MergeEndAsync(CancellationToken cancellationToken) { await Task.CompletedTask; }
    public virtual async Task MergeErrorAsync(CancellationToken cancellationToken) { await Task.CompletedTask; }

    public abstract Task<Dictionary<string, object>> DeleteModelAsync(IModelSnapshot itemToDelete, Dictionary<string, object> where, CancellationToken cancellationToken);

    public abstract void StartInsertModelCommand(IModelSnapshot itemToCreate);
    public abstract Task<Guid> RunInsertModelCommand(IModelSnapshot itemToCreate, CancellationToken cancellationToken);
    public abstract void CompleteInsertModelCommand(Type entityType);

    public abstract Task<Dictionary<string, object>> ModifyModelAsync(IModelDifference modelDifference, IModelSnapshot itemToModify, Dictionary<string, object> where, CancellationToken cancellationToken);

    public abstract Task RunProjectDbContextQueryAsync(string description, ProjectDbContextMergeQueryAsync query, CancellationToken cancellationToken = default);
    public abstract object? RunEntityValueResolver(IModelSnapshot modelSnapshot, string propertyName, EntityValueResolver entityValueResolver);

    protected DbCommand CreateModelSnapshotInsertCommandNoParameters(DbConnection connection, IModelSnapshot modelSnapshot)
    {
        var entityProperties = modelSnapshot.EntityType.GetMappedPrimitiveProperties().ToDictionary(p => p.Name, p => p);

        var columns = entityProperties.Values
            .Select(p => p.Name)
            .ToList();

        var tableType = modelSnapshot.EntityType;

        if (_discriminatorMappings.TryGetValue(modelSnapshot.EntityType, out var mapping))
        {
            tableType = mapping.TableEntityType;
            columns.Add(mapping.DiscriminatorColumnName);
        }

        var command = connection.CreateCommand();
        DataUtil.ApplyColumnsToInsertCommand(command, tableType, columns.ToArray());
        command.Prepare();

        foreach (DbParameter p in command.Parameters)
        {
            var propertyName = p.ParameterName.Substring(1);  // Skip the @ at the start of each ParameterName
            if (entityProperties.TryGetValue(propertyName, out var property))
            {
                p.Value = property.ToDefaultDatabaseCommandParameterValue();
            }
        }

        return command;
    }

    protected Guid AddModelSnapshotParametersToInsertCommand(DbCommand command, IModelSnapshot modelSnapshot)
    {
        var entityType = modelSnapshot.EntityType;
        var identityPropertyName = entityType.GetIdentityProperty()?.Name;
        Guid identityPropertyValue = Guid.Empty;

        var discriminator = (colName: string.Empty, colValue: string.Empty);
        if (_discriminatorMappings.TryGetValue(modelSnapshot.EntityType, out var mapping))
        {
            discriminator = (mapping.DiscriminatorColumnName, mapping.DiscriminatorColumnValue);
        }

        foreach (DbParameter p in command.Parameters)
        {

            // HACK:  This is a hack to get around the fact that the LexiconTranslationId
            if (p.ParameterName == "@LexiconTranslationId")
            {
                command.Parameters[p.ParameterName].Value = DBNull.Value;
                continue;
            }


            var propertyName = p.ParameterName.Substring(1);  // Skip the @ at the start of each ParameterName

            if (identityPropertyName == propertyName)
            {
                // We're adding an entity, so not trying to resolve to some existing
                // database id value.  If the modelSnapshot happens to have an Id
                // already, use it.  Otherwise create a new Guid:
                if (modelSnapshot.PropertyValues.ContainsKey(propertyName))
                {
                    identityPropertyValue = (Guid)modelSnapshot.PropertyValues[propertyName]!;
                }
                else if (_idReversePropertyNameMap.TryGetValue((entityType, propertyName), out var modelSnapshotPropertyName) &&
                        modelSnapshot.PropertyValues.TryGetValue(modelSnapshotPropertyName, out var value) &&
                        value is Guid guidValue)
                {
                    identityPropertyValue = guidValue;
                }
                else
                {
                    identityPropertyValue = Guid.NewGuid();
                }

                command.Parameters[p.ParameterName].Value = identityPropertyValue;

                continue;
            }
            else if (_entityValueResolvers.TryGetValue((entityType, propertyName), out EntityValueResolver? resolver))
            {
                var entityPropertyValue = RunEntityValueResolver(modelSnapshot, propertyName, resolver);
                command.Parameters[p.ParameterName].Value = entityPropertyValue ?? DBNull.Value;

                continue;
            }

            if (discriminator.colName == propertyName)
            {
                command.Parameters[p.ParameterName].Value = discriminator.colValue;
                continue;
            }

            if (modelSnapshot.PropertyValues.ContainsKey(propertyName))
            {
                var propertyValue = modelSnapshot.PropertyValues[propertyName];
                command.Parameters[p.ParameterName].Value = propertyValue.ToDatabaseCommandParameterValue(_dateTimeOffsetToBinary);

                continue;
            }

            //_logger.LogInformation($"No property or property converter found for property '{propertyName}' of entity type '{entityType.ShortDisplayName()}', setting to null during insert");
            //throw new InvalidModelStateException($"No property or property converter found for property '{propertyName}' of entity type '{entityType.ShortDisplayName()}'");
        }

        return identityPropertyValue;
    }

    protected Dictionary<string, object> ResolveWhereClause(Dictionary<string, object> where, IModelSnapshot modelSnapshot)
    {
        var entityType = modelSnapshot.EntityType;
        var resolvedValues = new Dictionary<string, object>();

        var whereColumns = where.Keys
            .SelectMany(key =>
                _idPropertyNameMap.TryGetValue((entityType, key), out var entityPropertyNames)
                    ? entityPropertyNames
                    : new[] { key }
                )
            .ToArray();

        foreach (var key in whereColumns)
        {
            if (_entityValueResolvers.TryGetValue((entityType, key), out var resolver))
            {
                var resolvedValue = RunEntityValueResolver(modelSnapshot, key, resolver);
                resolvedValues[key] = resolvedValue ?? DBNull.Value;
            }
            else
            {
                if (!where.ContainsKey(key))
                {
                    // Anything mapped has to have a resolver to get its value
                    throw new PropertyResolutionException($"Found mapped where clause key '{key}' but no resolver configured");
                }

                resolvedValues[key] = where[key].ToDatabaseCommandParameterValue(_dateTimeOffsetToBinary);
            }
        }

        return resolvedValues;
    }

    protected DbCommand CreateModelSnapshotUpdateCommand(DbConnection connection, IModelDifference modelDifference, IModelSnapshot modelSnapshot, Dictionary<string, object> resolvedWhereClause)
    {
        var entityType = modelSnapshot.EntityType;

        // Note that if there are any matching _propertyValueConverter mappings,
        // we output the mapped "EntityPropertyName" into columns:
        var columns = modelDifference.PropertyDifferences
            .SelectMany(pd =>
                _propertyNameMap.TryGetValue((entityType, pd.PropertyName), out var entityPropertyNames)
                    ? entityPropertyNames
                    : new[] { pd.PropertyName }
                )
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray();

        var command = connection.CreateCommand();

        var tableType = _discriminatorMappings.TryGetValue(entityType, out var mapping)
            ? mapping.TableEntityType
            : entityType;

        DataUtil.ApplyColumnsToUpdateCommand(command, tableType, columns, resolvedWhereClause.Keys.ToArray());

        try
        {
            command.Prepare();
        }
        catch (Exception ex)
        {
            throw new PropertyResolutionException($"Entity type '{entityType}' had following error when trying to prepare DbCommand: {ex.Message}");
        }

        resolvedWhereClause.ToList().ForEach(kvp => command.Parameters[$"@{kvp.Key}"].Value = kvp.Value);

        ApplyPropertyModelDifferencesToCommand(command, modelDifference, modelSnapshot);
        ApplyPropertyValueDifferencesToCommand(command, modelDifference, modelSnapshot);

        return command;
    }

    private void ApplyPropertyValueDifferencesToCommand(DbCommand command, IModelDifference modelDifference, IModelSnapshot modelSnapshot)
    {
        var entityType = modelSnapshot.EntityType;

        // Each property difference here refers to a simple value difference.
        foreach (var (propertyName, propertyValueDifference) in modelDifference.PropertyValueDifferences)
        {
            // Grab the propertyName + 'value2' from the value difference
            // and run it through any (optional) converter(s), and assign
            // to DbCommand
            var propertyValue = propertyValueDifference.Value2AsObject;

            IEnumerable<string>? entityPropertyValues = null;
            if (!_propertyNameMap.TryGetValue((entityType, propertyName), out entityPropertyValues))
            {
                entityPropertyValues = new List<string>() { propertyName };
            }

            entityPropertyValues = entityPropertyValues.Where(e => !string.IsNullOrEmpty(e)).ToList();
            if (!entityPropertyValues.Any())
            {
                continue;
            }

            foreach (var ep in entityPropertyValues)
            {
                if (_entityValueResolvers.TryGetValue((entityType, ep), out var resolver))
                {
                    propertyValue = RunEntityValueResolver(modelSnapshot, ep, resolver);
                    command.Parameters[$"@{ep}"].Value = propertyValue ?? DBNull.Value;

                    continue;
                }
                else
                {
                    propertyValue = propertyValue.ToDatabaseCommandParameterValue(_dateTimeOffsetToBinary);
                    command.Parameters[$"@{ep}"].Value = propertyValue;
                }
            }
        }
    }


    private void ApplyPropertyModelDifferencesToCommand(DbCommand command, IModelDifference modelDifference, IModelSnapshot modelSnapshot)
    {
        var entityType = modelSnapshot.EntityType;

        // Each property difference here refers to difference within a more complex
        // property (e.g. NoteModelRef.ModelRef), which has to have converter(s) in
        // order to apply to simple database column(s).
        foreach (var propertyModelDifference in modelDifference.PropertyDifferences
            .Where(d => d.PropertyValueDifference.GetType().IsAssignableTo(typeof(IModelDifference))))
        {
            var modelValueDifference = (IModelDifference)propertyModelDifference.PropertyValueDifference;

            // First apply the property difference to modelSnapshot (this
            // may have to traverse down a hierarachy), and then run the
            // converter(s) to get the DbCommand database value(s):
            modelSnapshot.ApplyPropertyDifference(propertyModelDifference);

            IEnumerable<string>? entityPropertyValues = null;
            if (!_propertyNameMap.TryGetValue((entityType, propertyModelDifference.PropertyName), out entityPropertyValues))
            {
                entityPropertyValues = new List<string>() { propertyModelDifference.PropertyName };
            }

            entityPropertyValues = entityPropertyValues.Where(e => !string.IsNullOrEmpty(e)).ToList();
            if (!entityPropertyValues.Any())
            {
                continue;
            }

            foreach (var ep in entityPropertyValues)
            {
                if (_entityValueResolvers.TryGetValue((entityType, ep), out var resolver))
                {
                    var propertyValue = RunEntityValueResolver(modelSnapshot, ep, resolver);
                    command.Parameters[$"@{ep}"].Value = propertyValue ?? DBNull.Value;
                }
                else if (modelValueDifference.ModelType.IsAssignableTo(typeof(IDictionary<string, object>)))
                {
                    command.Parameters[$"@{ep}"].Value = modelSnapshot.PropertyValues[propertyModelDifference.PropertyName]
                        .ToDatabaseCommandParameterValue(_dateTimeOffsetToBinary);
                }
                else
                {
                    throw new PropertyResolutionException($"Found mapped property '{propertyModelDifference.PropertyName}' ModelDifference for entity type '{entityType}' property '{ep}', but no resolver configured");
                }
            }
        }

    }

    protected DbCommand CreateModelSnapshotDeleteCommand(DbConnection connection, IModelSnapshot modelSnapshot, Dictionary<string, object> resolvedWhereClause)
    {
        var entityType = modelSnapshot.EntityType;

        var command = connection.CreateCommand();

        var whereColumns = resolvedWhereClause.Keys;
        var tableType = _discriminatorMappings.TryGetValue(entityType, out var mapping)
            ? mapping.TableEntityType
            : entityType;

        // ====================================================================
        //FIXME:  move to DataUtil.ApplyColumnsToDeleteCommand
        command.CommandText =
            $@"
                DELETE FROM {tableType.Name}
                WHERE {string.Join(", ", whereColumns.Select(c => c + " = @" + c))}
            ";

        foreach (var column in whereColumns)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@{column}";
            command.Parameters.Add(parameter);
        }
        // ====================================================================

        command.Prepare();
        resolvedWhereClause.ToList().ForEach(kvp => command.Parameters[$"@{kvp.Key}"].Value = kvp.Value);

        return command;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        await ValueTask.CompletedTask;
    }
}

public static class DbCommandExtensions
{
    private static readonly NullabilityInfoContext _nullabilityContext = new();

    internal static string GetGeneratedQuery(this DbCommand dbCommand)
    {
        var query = dbCommand.CommandText;
        foreach (DbParameter parameter in dbCommand.Parameters)
        {
            query = query.Replace(parameter.ParameterName, parameter.Value?.ToString());
        }

        return query;
    }

    internal static object? ToDefaultDatabaseCommandParameterValue(this PropertyInfo property)
    {
        var nullabilityInfo = _nullabilityContext.Create(property);

        if (property.PropertyType.IsAssignableTo(typeof(IDictionary<string, object>)))
        {
            var columnAttribute = (ColumnAttribute?)property.GetCustomAttribute(typeof(ColumnAttribute), true);
            if (columnAttribute?.TypeName == "jsonb")
            {
                if (nullabilityInfo.WriteState is not NullabilityState.Nullable)
                {
                    return JsonSerializer.Serialize(new Dictionary<string, object>());
                }
            }
        }

        if (nullabilityInfo.WriteState is not NullabilityState.Nullable)
        {
            if (property.PropertyType.IsValueType)
            {
                return Activator.CreateInstance(property.PropertyType);
            }
            else
            {
                throw new NotImplementedException($"Default value generator for non-nullable property of type '{property.PropertyType.ShortDisplayName()}' not implemented.");
            }
        }

        return DBNull.Value;
    }

    internal static object ToDatabaseCommandParameterValue(this object? value, DateTimeOffsetToBinaryConverter dateTimeOffsetToBinary)
    {
        if (value is DateTimeOffset)
        {
            value = dateTimeOffsetToBinary.ConvertToProvider(value);
        }
        else if (value is IDictionary<string, object>)
        {
            value = JsonSerializer.Serialize((IDictionary<string, object>)value);
        }

        return value != null ? value : DBNull.Value;
    }

    internal static Type ToDomainEntityIdType(this string domainEntityTypeName)
    {
        Type entityIdType = typeof(EntityId<>);
        Type[] typeArgs = { Type.GetType(domainEntityTypeName)! };
        return entityIdType.MakeGenericType(typeArgs);
    }
}