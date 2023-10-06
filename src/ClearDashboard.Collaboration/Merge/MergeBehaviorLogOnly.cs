using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ClearDashboard.DataAccessLayer.Data;
using SIL.Machine.Utils;

namespace ClearDashboard.Collaboration.Merge;

public class MergeBehaviorLogOnly : MergeBehaviorBase
{
    public MergeBehaviorLogOnly(/* pass in configuration */ILogger logger, MergeCache mergeCache, IProgress<ProgressStatus> progress) : base(logger, mergeCache, progress)
    {
    }

    public override async Task MergeStartAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Opening database connection and starting transaction");
    }

    public override async Task MergeEndAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Committing transaction");
    }

    public override async Task MergeErrorAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _logger.LogInformation("Rolling back transaction");
    }

    public override async Task<Dictionary<string, object>> DeleteModelAsync(IModelSnapshot itemToDelete, Dictionary<string, object> where, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var id = string.Join(", ", where);
        _logger.LogInformation($"HARD DELETING:  '{itemToDelete.EntityType.ShortDisplayName()}' item having id '{id}'");

        return where;
    }

    public override void StartInsertModelCommand(IModelSnapshot itemToCreate)
    {
        _logger.LogInformation($"STARTING INSERT:  '{itemToCreate.EntityType.ShortDisplayName()}'");
    }

    public override async Task<Guid> RunInsertModelCommand(IModelSnapshot itemToCreate, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        _logger.LogInformation($"RUNNING INSERT:  '{itemToCreate.EntityType.ShortDisplayName()}' on item having id '{itemToCreate.GetId()}'");
        return Guid.Empty;
    }

    public override void CompleteInsertModelCommand(Type modelType)
    {
        _logger.LogInformation($"Completing INSERT:  '{modelType.ShortDisplayName()}'");
    }

    public override async Task<Dictionary<string, object>> ModifyModelAsync(IModelDifference modelDifference, IModelSnapshot itemToModify, Dictionary<string, object> where, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var id = string.Join(", ", where);
        _logger.LogInformation($"MODIFYING:  '{itemToModify.EntityType.ShortDisplayName()}' item having id '{id}'");

        return where;
    }

    public override async Task<IEnumerable<Dictionary<string, object?>>> SelectEntityValuesAsync(Type entityType, IEnumerable<string> selectColumns, Dictionary<string, object?> resolvedWhereClause, bool useNotIndexedInFromClause, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        var whereItems = string.Join(", ", resolvedWhereClause);
        _logger.LogInformation($"Getting Ids for :  '{entityType.ShortDisplayName()}' where: '{whereItems}'");

        return Enumerable.Empty<Dictionary<string, object?>>();
    }

    public override async Task RunProjectDbContextQueryAsync(string description, ProjectDbContextMergeQueryAsync query, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        _logger.LogInformation($"RUNNING:  '{description}'");
    }
    public override async Task RunDbConnectionQueryAsync(string description, DbConnectionMergeQueryAsync query, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;

        _logger.LogInformation($"RUNNING:  '{description}'");
    }

    public override async Task<object?> RunEntityValueResolverAsync(IModelSnapshot modelSnapshot, string propertyName, EntityValueResolverAsync propertyValueConverter)
    {
        await Task.CompletedTask;

        _logger.LogInformation($"RUNNING converter for model type {modelSnapshot.EntityType.ShortDisplayName()} propertyName:  '{propertyName}'");
        return modelSnapshot;
    }
}

