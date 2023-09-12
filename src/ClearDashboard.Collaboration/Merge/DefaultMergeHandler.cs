using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Services;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DataAccessLayer.Data;
using System.Threading;
using SIL.Machine.Utils;

namespace ClearDashboard.Collaboration.Merge;

public class DefaultMergeHandler<T> : DefaultMergeHandler where T : IModelSnapshot
{
    public DefaultMergeHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    public async Task DeleteListDifferencesAsync(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, CancellationToken cancellationToken = default)
    {
        if (listDifference.HasMembershipDeletes)
        {
            // OnlyIn1: part of last commit merged, then deleted at some point on some remote
            //      if not in current:   they were deleted both here in some remote
            //      if in current:  delete (hard)
            foreach (var onlyIn1 in listDifference.OnlyIn1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var itemInCurrentSnapshot = currentSnapshotList.FindById<T>(onlyIn1.GetId());
                if (itemInCurrentSnapshot is not null)
                {
                    await HandleDeleteAsync(itemInCurrentSnapshot, cancellationToken);
                }
                else
                {
                    _mergeContext.Progress.Report(new ProgressStatus(0, $"Unable to delete - model item of type '{typeof(T).ShortDisplayName()}' having id '{onlyIn1}' not found in current snapshot"));
                    _mergeContext.Logger.LogInformation($"Unable to delete - model item of type '{typeof(T).ShortDisplayName()}' having id '{onlyIn1}' not found in current snapshot");
                }
            }
        }

        if (listDifference.HasListMemberModelDifferences)
        {
            foreach (var modelDifference in listDifference.ListMemberModelDifferences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (modelDifference.ChildListDifferences.Any())
                {
                    // Since this is a ModelDifference of an IModelSnapshot, its Id should
                    // never be null:
                    var itemInCurrentSnapshot = currentSnapshotList.FindById<T>(modelDifference.Id!);

                    await HandleChildListDifferenceDeleteAsync(modelDifference.ChildListDifferences, itemInCurrentSnapshot, cancellationToken);
                }
            }
        }
    }

    public async Task CreateListDifferencesAsync(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, CancellationToken cancellationToken = default)
    {
        if (listDifference.HasMembershipCreates)
        {
            // OnlyIn2: added since last merge on some remote
            //      if not in current:  add
            //      if in current:  added both here and some remote - what to do?  look for differences between the items in OnlyIn2
            //      and current snapshot and apply them?
            foreach (var onlyIn2 in listDifference.OnlyIn2)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var itemInCurrentSnapshot = currentSnapshotList.FindById<T>(onlyIn2.GetId());

                // Added since last merge on some remote
                //      if nitemInCurrentSnapshotot in current:  add
                //      if in current:  added both here and some remote - what to do?  look for differences between the items in OnlyIn2
                //      and current snapshot and apply them?
                if (itemInCurrentSnapshot is null)
                {
                    // FIXME:  should we call something like currentSnapshotList.AddById()?
                    HandleCreateStart(onlyIn2);
                    await HandleCreateAsync(onlyIn2, cancellationToken);
                    await HandleCreateComplete(onlyIn2, cancellationToken);

                    var name = GetModelSnapshotDisplayName(onlyIn2);

                    _mergeContext.Progress.Report(new ProgressStatus(0, $"Inserted {onlyIn2.EntityType.ShortDisplayName()} having name '{name}' and id '{onlyIn2.GetId()}'"));
                    _mergeContext.Logger.LogInformation($"Inserted {onlyIn2.EntityType.ShortDisplayName()} having id '{onlyIn2.GetId()}'");

                    await HandleCreateChildrenAsync(onlyIn2, cancellationToken);
                }
                else
                {
                    var modelDifference = (IModelDifference<T>)itemInCurrentSnapshot.GetModelDifference((IModelDistinguishable<T>)onlyIn2);
                    await HandleModifyPropertiesAsync(modelDifference, itemInCurrentSnapshot, cancellationToken);
                }
            }
        }
    }

    public virtual async Task ModifyListDifferencesAsync(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, IEnumerable<T>? targetCommitSnapshotList, CancellationToken cancellationToken = default)
    {
        if (listDifference.HasListMemberModelDifferences)
        {
            foreach (var modelDifference in listDifference.ListMemberModelDifferences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Since this is a ModelDifference of an IModelSnapshot, its Id should
                // never be null:
                var itemInCurrentSnapshot = currentSnapshotList.FindById<T>(modelDifference.Id!);
                var itemInTargetCommitSnapshot = targetCommitSnapshotList.FindById<T>(modelDifference.Id!);

                if (itemInCurrentSnapshot is not null)
                {
                    await HandleModifyPropertiesAsync(modelDifference, itemInCurrentSnapshot, cancellationToken);
                }
                else
                {
                    if (itemInTargetCommitSnapshot is not null)
                    {
                        // FIXME:  does this mean it got deleted locally but in the commits it
                        // was never deleted but modified instead?
                        HandleCreateStart(itemInTargetCommitSnapshot);
                        await HandleCreateAsync(itemInTargetCommitSnapshot, cancellationToken);
                        await HandleCreateComplete(itemInTargetCommitSnapshot, cancellationToken);

                        var name = GetModelSnapshotDisplayName(itemInTargetCommitSnapshot);

                        _mergeContext.Progress.Report(new ProgressStatus(0, $"Inserted {itemInTargetCommitSnapshot.EntityType.ShortDisplayName()} having name '{name}' and id '{itemInTargetCommitSnapshot.GetId()}'"));
                        _mergeContext.Logger.LogInformation($"Inserted {itemInTargetCommitSnapshot.EntityType.ShortDisplayName()} having id '{itemInTargetCommitSnapshot.GetId()}'");
                    }
                    else
                    {
                        throw new InvalidDifferenceStateException($"Model difference of type '{modelDifference.GetType().ShortDisplayName()}' exists for id {modelDifference.Id}, but IModelSnapshot not found in target commit snapshot.");
                    }
                }

                if (modelDifference.ChildListDifferences.Any())
                {
                    await HandleChildListDifferenceAsync(modelDifference.ChildListDifferences, itemInCurrentSnapshot, itemInTargetCommitSnapshot, cancellationToken);
                }
            }
        }
    }

    protected virtual async Task HandleDeleteAsync(T itemToDelete, CancellationToken cancellationToken)
    {
        var modelSnapshot = (IModelSnapshot)itemToDelete;

        var where = new Dictionary<string, object>() { { modelSnapshot.IdentityKey, modelSnapshot.GetId() } };
        await _mergeContext.MergeBehavior.DeleteModelAsync(modelSnapshot, where, cancellationToken);

        //// If deleting a parent, need to first delete all its children
        //if (itemToDelete is IModelSnapshot)
        //{
        //    var children = ((IModelSnapshot)itemToDelete).Children;
        //    var childrenHandler = _mergeContext.FindMergeHandler(typeof(T));

        //    if (children is not null && children.Any())
        //    {
        //        foreach (var kvp in children)
        //        {
        //            foreach (var child in kvp.Value)
        //            {
        //                HandleDelete(child);
        //                // if a tokenized corpus, need to delete all related tokens (that aren't in the snapshot anywhere)
        //            }
        //        }
        //    }
        //}

        // If deleting a parent, need to first delete all its children
        //if (itemToDelete.Children is not null && itemToDelete.Children.Any())
        //{
        //    foreach (var kvp in itemToDelete.Children)
        //    {
        //        _mergeMessages.Add($"HARD DELETING:  children '{kvp.Key}' of type '{typeof(T).Name}' item having id '{itemToDelete.GetId()}'");
        //        foreach (var child in kvp.Value)
        //        {
        //            //DeleteModel<>(child);
        //            // if a tokenized corpus, need to delete all related tokens (that aren't in the snapshot anywhere)
        //        }
        //    }
        //}
    }

    protected virtual void HandleCreateStart(T itemToCreate)
    {
        _mergeContext.MergeBehavior.StartInsertModelCommand(itemToCreate);
    }
    protected virtual async Task<Guid> HandleCreateAsync(T itemToCreate, CancellationToken cancellationToken)
    {
        var id = await _mergeContext.MergeBehavior.RunInsertModelCommand(itemToCreate, cancellationToken);
        return id;
    }
    protected virtual async Task HandleCreateComplete(T itemToCreate, CancellationToken cancellationToken)
    {
        _mergeContext.MergeBehavior.CompleteInsertModelCommand(itemToCreate.EntityType);
        await Task.CompletedTask;
    }

    public virtual async Task<bool> HandleModifyPropertiesAsync(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
    {
        if (typeof(T).IsAssignableTo(typeof(IModelSnapshot)))
        {
            var modelSnapshotToModify = (IModelSnapshot)itemToModify;
            var modelMergeResult = CheckMerge(modelDifference, itemToModify);

            if (modelMergeResult == ModelMergeResult.ShouldMerge)
            {
                //if (!modelSnapshotToModify.EntityPropertyValues.ContainsKey(modelSnapshotToModify.IdentityKey))
                //{
                //    throw new NotImplementedException($"Derived merge handler with '{typeof(T).ShortDisplayName()}' model-specific HandleModifyProperties functionality for retrieving where clause values");
                //}

                var where = new Dictionary<string, object>() { { modelSnapshotToModify.IdentityKey, modelSnapshotToModify.GetId() } };
                await _mergeContext.MergeBehavior.ModifyModelAsync(modelDifference, modelSnapshotToModify, where, cancellationToken);

                return true;
            }
        }
        else
        {
            throw new NotImplementedException($"Derived merge handler with '{typeof(T).ShortDisplayName()}' model-specific HandleModifyProperties functionality");
        }

        return false;
    }

    protected virtual async Task HandleChildListDifferenceAsync(
        IReadOnlyDictionary<string, IListDifference> childListDifferences,
        T? parentItemInCurrentSnapshot,
        T? parentItemInTargetCommitSnapshot,
        CancellationToken cancellationToken)
    {
        foreach (var (childName, listDifference) in childListDifferences)
        {
            IEnumerable<IModelDistinguishable>? childrenInCurrentSnapshot = null;
            IEnumerable<IModelDistinguishable>? childrenInTargetCommitSnapshot = null;

            if (parentItemInCurrentSnapshot is not null && ((IModelSnapshot)parentItemInCurrentSnapshot).Children.ContainsKey(childName))
            {
                childrenInCurrentSnapshot = ((IModelSnapshot)parentItemInCurrentSnapshot).Children[childName];
            }

            if (parentItemInTargetCommitSnapshot is not null && ((IModelSnapshot)parentItemInTargetCommitSnapshot).Children.ContainsKey(childName))
            {
                childrenInTargetCommitSnapshot = ((IModelSnapshot)parentItemInTargetCommitSnapshot).Children[childName];
            }

            if (listDifference.GetType().IsAssignableToGenericType(typeof(IListDifference<>)))
            {
                var handler = _mergeContext.FindMergeHandler(listDifference.GetType().GetGenericArguments()[0]);

                MethodInfo createMethod = handler.GetType().GetMethod(nameof(DefaultMergeHandler<T>.CreateListDifferencesAsync))!;
                dynamic createAwaitable = createMethod.Invoke(handler, new object?[] { listDifference, childrenInCurrentSnapshot, cancellationToken })!;
                await createAwaitable;

                MethodInfo modifyMethod = handler.GetType().GetMethod(nameof(DefaultMergeHandler<T>.ModifyListDifferencesAsync))!;
                dynamic modifyAwaitable = modifyMethod.Invoke(handler, new object?[] { listDifference, childrenInCurrentSnapshot, childrenInTargetCommitSnapshot, cancellationToken })!;
                await modifyAwaitable;
            }
            else
            {
                throw new InvalidDifferenceStateException($"Encountered child IListDifference without any generic model type!");
            }
        }
    }
    protected virtual async Task HandleChildListDifferenceDeleteAsync(
    IReadOnlyDictionary<string, IListDifference> childListDifferences,
    T? parentItemInCurrentSnapshot,
    CancellationToken cancellationToken)
    {
        foreach (var (childName, listDifference) in childListDifferences)
        {
            IEnumerable<IModelDistinguishable>? childrenInCurrentSnapshot = null;

            if (parentItemInCurrentSnapshot is not null && ((IModelSnapshot)parentItemInCurrentSnapshot).Children.ContainsKey(childName))
            {
                childrenInCurrentSnapshot = ((IModelSnapshot)parentItemInCurrentSnapshot).Children[childName];
            }

            if (listDifference.GetType().IsAssignableToGenericType(typeof(IListDifference<>)))
            {
                var handler = _mergeContext.FindMergeHandler(listDifference.GetType().GetGenericArguments()[0]);

                MethodInfo deleteMethod = handler.GetType().GetMethod(nameof(DefaultMergeHandler<T>.DeleteListDifferencesAsync))!;
                dynamic deleteAwaitable = deleteMethod.Invoke(handler, new object?[] { listDifference, childrenInCurrentSnapshot, cancellationToken })!;
                await deleteAwaitable;
            }
            else
            {
                throw new InvalidDifferenceStateException($"Encountered child IListDifference without any generic model type!");
            }
        }
    }
    /// <summary>
    /// Called after the insert of a parent entity, this inserts the new parent entity's children (if any)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="parentSnapshot"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task HandleCreateChildrenAsync(
        T parentSnapshot,
        CancellationToken cancellationToken)
    {
        foreach (var childName in parentSnapshot.Children.Keys)
        {
            _mergeContext.Progress.Report(new ProgressStatus(0, $"Inserting {childName} children for {parentSnapshot.EntityType.ShortDisplayName()} '{parentSnapshot.GetId()}'"));
            _mergeContext.Logger.LogInformation($"Inserting {childName} children for {parentSnapshot.EntityType.ShortDisplayName()} '{parentSnapshot.GetId()}'");

            Type childType = parentSnapshot.Children[childName].GetType().GetGenericArguments()[0];
            if (childType.IsAssignableToGenericType(typeof(IModelSnapshot<>)) && childType.GetGenericArguments().Any())
            {
                var t = typeof(IModelSnapshot<>);
                Type[] typeArgs = { childType.GetGenericArguments()[0] };
                childType = t.MakeGenericType(typeArgs);
            }

            var handler = _mergeContext.FindMergeHandler(childType);

            MethodInfo createMethod = handler.GetType().GetMethod(nameof(DefaultMergeHandler<T>.CreateListItemsAsync), BindingFlags.Instance | BindingFlags.Public)!;
            dynamic createAwaitable = createMethod.Invoke(handler, new object?[] { parentSnapshot.Children[childName], cancellationToken })!;
            var insertCount = await createAwaitable;

            _mergeContext.Logger.LogInformation($"Completed inserting {insertCount} {childName} children for {parentSnapshot.EntityType.ShortDisplayName()} '{parentSnapshot.GetId()}'");
        }
    }

    public async Task<int> CreateListItemsAsync(IEnumerable<T> snapshotList, CancellationToken cancellationToken = default)
    {
        var insertCount = 0;

        var firstChild = snapshotList.FirstOrDefault();

        if (firstChild is not null)
        {
            HandleCreateStart(firstChild);
            foreach (var modelSnapshot in snapshotList)
            {
                await HandleCreateAsync(modelSnapshot, cancellationToken);
                insertCount++;
            }
            await HandleCreateComplete(firstChild, cancellationToken);
        }

        foreach (var modelSnapshot in snapshotList)
        {
            await HandleCreateChildrenAsync(modelSnapshot, cancellationToken);
        }

        return insertCount;
    }

    public async Task MergeListDifferenceGroup(
        IListDifference listDifference,
        IEnumerable<IModelDistinguishable>? childrenInCurrentSnapshot,
        IEnumerable<IModelDistinguishable>? childrenInTargetCommitSnapshot,
        CancellationToken cancellationToken
        )
    {
        if (!listDifference.GetType().IsAssignableTo(typeof(IListDifference<T>)))
        {
            throw new InvalidDifferenceStateException($"Encountered child IListDifference without any generic model type!");
        }

        if (childrenInCurrentSnapshot is not null && !childrenInCurrentSnapshot.GetType().IsAssignableTo(typeof(IEnumerable<T>)))
        {
            throw new InvalidDifferenceStateException($"Encountered non-IModelSnapshot children in current snapshot list!");
        }

        if (childrenInTargetCommitSnapshot is not null && !childrenInTargetCommitSnapshot.GetType().IsAssignableTo(typeof(IEnumerable<T>)))
        {
            throw new InvalidDifferenceStateException($"Encountered non-IModelSnapshot children in target commit snapshot list!");
        }

        var difference = (IListDifference<T>)listDifference;
        var currentChildren = (IEnumerable<T>?)childrenInCurrentSnapshot;
        var targetCommitChildren = (IEnumerable<T>?)childrenInTargetCommitSnapshot;

        await DeleteListDifferencesAsync(difference, currentChildren, cancellationToken);
        await CreateListDifferencesAsync(difference, currentChildren, cancellationToken);
        await ModifyListDifferencesAsync(difference, currentChildren, targetCommitChildren, cancellationToken);
    }
}

public class DefaultMergeHandler
{
    protected readonly MergeContext _mergeContext;
    public DefaultMergeHandler(MergeContext mergeContext)
    {
        _mergeContext = mergeContext;
    }

    protected bool CheckMergePropertyValueDifferences(IModelDifference modelDifference, IReadOnlyDictionary<string, object?> propertyValues)
    {
        var hasChange = false;

        foreach (var (propertyName, propertyValueDifference) in modelDifference.PropertyValueDifferences)
        {
            if (propertyValues.TryGetValue(propertyName, out var currentValue))
            {
                if (propertyValueDifference.EqualsValue1(currentValue))
                {
                    _mergeContext.Logger.LogInformation($"Updating property '{propertyName}' current value '{currentValue}' to new value '{propertyValueDifference.Value2AsObject}'");
                    hasChange = true;
                }
                else if (!propertyValueDifference.EqualsValue2(currentValue))
                {
                    _mergeContext.Logger.LogInformation($"Conflict with property {propertyName} current value '{currentValue}' not matching last merge value '{propertyValueDifference.Value1AsObject}' or new value '{propertyValueDifference.Value2AsObject}'");
                    hasChange = true;
                    propertyValueDifference.ConflictValue = currentValue;
                }
            }
            else
            {
                throw new InvalidDifferenceStateException($"Property name '{propertyName}' from PropertyDifferences does not exist in the current snapshot");
            }
        }

        return hasChange;
    }

    protected bool CheckMergePropertyModelDifferences<T>(IModelDifference modelDifference, T itemToModify)
        where T : IModelDistinguishable
    {
        var hasChange = false;

        foreach (var (propertyName, propertyModelDifference) in modelDifference.PropertyModelDifferences)
        {
            if (itemToModify.PropertyValues.TryGetValue(propertyName, out var v))
            {
                if (v is IModelDistinguishable)
                {
                    hasChange = CheckMergePropertyModelDifferences(propertyModelDifference, (IModelDistinguishable)v) || hasChange;
                }
                else if (v is IDictionary<string, object>)
                {
                    var propertyValues = ((IDictionary<string, object>)v)
                        .ToDictionary(e => e.Key, e => (object?)e.Value).AsReadOnly();
                    hasChange = CheckMergePropertyValueDifferences(propertyModelDifference, propertyValues) || hasChange;
                }
                else
                {
                    throw new InvalidDifferenceStateException($"Unable to CheckMerge for property '{propertyName}' having item type {v?.GetType()?.ShortDisplayName()}");
                }
            }
            else
            {
                throw new InvalidDifferenceStateException($"Property name '{propertyName}' from ProjectDifferences snapshots does not exist in the current snapshot");
            }
        }

        return hasChange;
    }

    protected virtual ModelMergeResult CheckMerge<T>(IModelDifference<T> modelDifference, T itemToModify)
        where T : IModelDistinguishable
    {
        var hasChange = false;
        if (modelDifference.HasDifferences)
        {
            _mergeContext.Logger.LogDebug($"Checking merge of: '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");

            hasChange = CheckMergePropertyValueDifferences(modelDifference, itemToModify.PropertyValues);
            hasChange = CheckMergePropertyModelDifferences(modelDifference, itemToModify) || hasChange;
        }

        if (modelDifference.HasMergeConflict)
        {
            string conflictMessage = $"Merge conflict(s) detected:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'";

            if (_mergeContext.MergeMode == MergeMode.RequiresConflictResolution)
            {
                _mergeContext.Progress.Report(new ProgressStatus(0, conflictMessage));
                _mergeContext.Logger.LogInformation(conflictMessage);

                throw new MergeConflictException(modelDifference);
            }
            else if (_mergeContext.MergeMode == MergeMode.LocalOverridesRemote)
            {
                conflictMessage += ", but MergeMode is LocalOverridesRemote, so leaving local as-is";
                _mergeContext.Progress.Report(new ProgressStatus(0, conflictMessage));
                _mergeContext.Logger.LogInformation(conflictMessage);

                return ModelMergeResult.NoChange;
            }
            else
            {
                conflictMessage += ", and MergeMode is RemoteOverridesLocal, so merging in remote changes";
                _mergeContext.Progress.Report(new ProgressStatus(0, conflictMessage));
                _mergeContext.Logger.LogInformation(conflictMessage);

                return ModelMergeResult.ShouldMerge;
            }
        }
        else if (hasChange)
        {
            _mergeContext.Progress.Report(new ProgressStatus(0, $"Change(s) detected:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'"));
            _mergeContext.Logger.LogInformation($"Change(s) detected:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");
            return ModelMergeResult.ShouldMerge;
        }

        _mergeContext.Logger.LogInformation($"No change:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");
        return ModelMergeResult.NoChange;
    }

    protected static string GetModelSnapshotDisplayName(IModelSnapshot modelSnapshot)
    {
        // GeneralModelSnapshot version of ToString() (used here for log output)
        modelSnapshot.TryGetPropertyValue("DisplayName", out object? displayName);
        if (string.IsNullOrEmpty((string?)displayName))
        {
            modelSnapshot.TryGetPropertyValue("Name", out displayName);
        }
        if (string.IsNullOrEmpty((string?)displayName))
        {
            modelSnapshot.TryGetPropertyValue(key: "Text", out displayName);
        }
        if (string.IsNullOrEmpty((string?)displayName))
        {
            modelSnapshot.TryGetPropertyValue(key: "FirstName", out displayName);
        }

        return string.IsNullOrEmpty((string?)displayName) ? "Unknown" : (string)displayName;
    }

    public static Guid LookupTokenComponent(ProjectDbContext projectDbContext, Guid tokenizedCorpusId, string engineTokenId, MergeCache cache)
    {
        if (!cache.TryLookupCacheEntry((typeof(Models.TokenizedCorpus), tokenizedCorpusId.ToString()),
            engineTokenId, out var id))
        {
            var tokenComponentId = projectDbContext.TokenComponents
                .Where(e => e.EngineTokenId == engineTokenId!)
                .Where(e => e.TokenizedCorpusId == tokenizedCorpusId!)
                .Select(e => e.Id)
                .FirstOrDefault();

            if (tokenComponentId == default(Guid)) throw new InvalidModelStateException($"Invalid EngineTokenId '{engineTokenId}' + TokenizedCorpusId '{tokenizedCorpusId}' - TokenComponentId not found");
            return tokenComponentId;
        }

        return (Guid)id!;
    }

    public static void LoadTokenizedCorpusLocationsIntoCache(ProjectDbContext projectDbContext, Guid tokenizedCorpusId, MergeCache cache)
    {
        if (!cache.ContainsCacheKey((typeof(Models.TokenizedCorpus), tokenizedCorpusId.ToString())))
        {
            var tokenComponentsByLocationId = projectDbContext.TokenComponents
                .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                .Where(e => e.EngineTokenId != null)
                .ToDictionary(e => e.EngineTokenId!, e => e);

            var tokenLocationIds = tokenComponentsByLocationId.ToDictionary(e => e.Key, e => (object?)e.Value.Id);
            var tokenLocationTrainingTexts = tokenComponentsByLocationId.ToDictionary(e => e.Key, e => (object?)e.Value.TrainingText);

            cache.AddCacheEntrySet((typeof(Models.TokenizedCorpus), tokenizedCorpusId.ToString()), tokenLocationIds);
            cache.AddCacheEntrySet((typeof(Models.AlignmentSetDenormalizationTask), tokenizedCorpusId.ToString()), tokenLocationTrainingTexts);
        }
    }
}
