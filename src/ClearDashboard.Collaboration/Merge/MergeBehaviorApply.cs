using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public class MergeBehaviorApply : MergeBehaviorBase
{
    private readonly ProjectDbContext _projectDbContext;

    private DbConnection? _connection;
    private DbTransaction? _transaction;
    private readonly Dictionary<Type, DbCommand> _insertCommandsByType = new();
    private bool _connectionOpen = false;

    public MergeBehaviorApply(/* pass in configuration */ILogger logger, ProjectDbContext projectDbContext, MergeCache mergeCache, IProgress<ProgressStatus> progress) : base(logger, mergeCache, progress)
    {
        _projectDbContext = projectDbContext;
    }

    public override async Task MergeStartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Opening database connection and starting transaction");

        await _projectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
        _connectionOpen = true;

        _connection = _projectDbContext.Database.GetDbConnection();
        _transaction = await _connection.BeginTransactionAsync(cancellationToken);
        _projectDbContext.Database.UseTransaction(_transaction);
    }

    public override async Task MergeEndAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
        {
            _logger.LogInformation("Saving ProjectDbContext changes");
            await _projectDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Committing transaction");
            await _transaction.CommitAsync(cancellationToken);
            _projectDbContext.Database.UseTransaction(null);

            await _projectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            _connectionOpen = false;
        }
    }

    public override async Task MergeErrorAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null)
        {
            _logger.LogInformation("Rolling back transaction");
            await _transaction.RollbackAsync(cancellationToken);
            _projectDbContext.Database.UseTransaction(null);

            await _projectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            _connectionOpen = false;
        }
    }

    public override async Task<Dictionary<string, object>> DeleteModelAsync(IModelSnapshot itemToDelete, Dictionary<string, object> where, CancellationToken cancellationToken)
    {
        if (_connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");
        var id = string.Join(", ", where);

        var resolvedWhereClause = await ResolveWhereClauseAsync(where, itemToDelete);
        var command = CreateModelSnapshotDeleteCommand(_connection, itemToDelete, resolvedWhereClause);

        _logger.LogInformation($"Executing delete query for model type '{itemToDelete.EntityType.ShortDisplayName()}' having id '{id}'");

        await Task.CompletedTask;
        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return resolvedWhereClause;
    }

    public override void StartInsertModelCommand(IModelSnapshot itemToCreate)
    {
        if (_connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");
        if (_insertCommandsByType.ContainsKey(itemToCreate.EntityType))
        {
            throw new Exception($"Insert model command for type {itemToCreate.EntityType.ShortDisplayName()} is already in progress");
        }

        _logger.LogDebug($"Creating insert command for model type {itemToCreate.EntityType.ShortDisplayName()}");

        var command = CreateModelSnapshotInsertCommandNoParameters(_connection, itemToCreate);
        _insertCommandsByType.Add(itemToCreate.EntityType, command);
    }

    public override async Task<Guid> RunInsertModelCommand(IModelSnapshot itemToCreate, CancellationToken cancellationToken)
    {
        if (!_insertCommandsByType.ContainsKey(itemToCreate.EntityType))
        {
            throw new Exception($"Insert model command for type {itemToCreate.EntityType.ShortDisplayName()} has not been created yet");
        }

        _logger.LogDebug($"Running insert command for model type:  '{itemToCreate.EntityType.ShortDisplayName()}' having id '{itemToCreate.GetId()}'");

        var command = _insertCommandsByType[itemToCreate.EntityType];
        var id = await AddModelSnapshotParametersToInsertCommand(command, itemToCreate);

        await Task.CompletedTask;
        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return id;
    }

    public override void CompleteInsertModelCommand(Type modelType)
    {
        if (!_insertCommandsByType.ContainsKey(modelType))
        {
            throw new Exception($"Insert model command for type {modelType.ShortDisplayName()} has not been created yet");
        }

        _logger.LogDebug($"Completing (disposing) insert command for model type:  '{modelType.ShortDisplayName()}'");

        _insertCommandsByType[modelType].Dispose();
        _insertCommandsByType.Remove(modelType);
    }

    public override async Task<Dictionary<string, object>> ModifyModelAsync(IModelDifference modelDifference, IModelSnapshot itemToModify, Dictionary<string, object> where, CancellationToken cancellationToken)
    {
        if (_connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");
        var id = string.Join(", ", where);

        var resolvedWhereClause = await ResolveWhereClauseAsync(where, itemToModify);
        var command = await CreateModelSnapshotUpdateCommand(_connection, modelDifference, itemToModify, resolvedWhereClause);

        _logger.LogDebug($"Running update command for model type: '{itemToModify.EntityType.ShortDisplayName()}' having id '{id}'");

        foreach (DbParameter p in command.Parameters)
        {
            _logger.LogDebug($"Parameter name {p.ParameterName}, value {p.Value}");
        }

        await Task.CompletedTask;

        // FIXME:  what type of exception is thrown if the where clause doesn't match any
        // record in the database?  Propbably log the details but continue?
        _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        return resolvedWhereClause;
    }

    public override async Task<IEnumerable<Dictionary<string, object?>>> SelectEntityValuesAsync(Type entityType, IEnumerable<string> selectColumns, Dictionary<string, object?> whereClause, bool useNotIndexedInFromClause, CancellationToken cancellationToken)
    {
        var tableType = ResolveTableName(entityType, whereClause);

        return await DataUtil.SelectEntityValuesAsync(
            _connection, 
            tableType, 
            selectColumns, 
            whereClause, 
            useNotIndexedInFromClause, 
            cancellationToken);
    }

    public override async Task<int> DeleteEntityValuesAsync(Type entityType, Dictionary<string, object?> whereClause, CancellationToken cancellationToken)
    {
        var tableType = ResolveTableName(entityType, whereClause);

        return await DataUtil.DeleteEntityValuesAsync(
            _connection,
            tableType,
            whereClause,
            cancellationToken);
    }

    public override async Task RunProjectDbContextQueryAsync(string description, ProjectDbContextMergeQueryAsync query, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug($"Running handler '{GetType().Name}' specific query:  '{description}'");
        await query.Invoke(_projectDbContext, MergeCache, _logger, Progress, cancellationToken);
    }

    public override async Task RunDbConnectionQueryAsync(string description, DbConnectionMergeQueryAsync query, CancellationToken cancellationToken = default)
    {
        if (_connection is null || !_connectionOpen)
        {
            throw new PropertyResolutionException($"Unable to run '{description}' because DbConnection is closed");
        }

        _logger.LogDebug($"Running handler '{GetType().Name}' specific query:  '{description}'");
        await query.Invoke(_connection, MergeCache, _logger, Progress, cancellationToken);
    }

    public override async Task<object?> RunEntityValueResolverAsync(IModelSnapshot modelSnapshot, string propertyName, EntityValueResolverAsync propertyValueConverter)
    {
        if (_connection is null || !_connectionOpen)
        {
            throw new PropertyResolutionException($"Unable to resolve '{propertyName}' because DbConnection is closed");
        }
        return await propertyValueConverter.Invoke(modelSnapshot, _projectDbContext, _connection, MergeCache, _logger);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var commandKey in _insertCommandsByType.Keys)
            {
                _logger.LogInformation($"Disposing DbCommand {commandKey}");

                _insertCommandsByType[commandKey].Dispose();
                _insertCommandsByType.Remove(commandKey);
            }

            if (_transaction is not null)
            {
                _logger.LogInformation($"Disposing transaction");
                _transaction.Dispose();
            }
            if (_connection is not null)
            {
                _logger.LogInformation($"Disposing connection");
                _connection.Dispose();
            }

            _transaction = null;
            _connection = null;

            if (_connectionOpen)
            {
                _logger.LogInformation($"Closing connection");

                _projectDbContext.Database.CloseConnection();
               _connectionOpen = false;
            }
            _projectDbContext.Dispose();
        }
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        foreach (var commandKey in _insertCommandsByType.Keys)
        {
            _logger.LogInformation($"Disposing DbCommand {commandKey} (async)");

            await _insertCommandsByType[commandKey].DisposeAsync();
            _insertCommandsByType.Remove(commandKey);
        }

        if (_transaction is not null)
        {
            _logger.LogInformation($"Disposing transaction (async)");
            await _transaction.DisposeAsync().ConfigureAwait(false);
        }
        if (_connection is not null)
        {
            _logger.LogInformation($"Disposing connection (async)");
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _transaction = null;
        _connection = null;

        if (_connectionOpen)
        {
            _logger.LogInformation($"Closing connection (async)");

            await _projectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            _connectionOpen = false;
        }
        await _projectDbContext.DisposeAsync().ConfigureAwait(false);
    }
}

