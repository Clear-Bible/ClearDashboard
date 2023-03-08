using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using System.Threading;
using ClearDashboard.DAL.Alignment.Translation;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;

namespace ClearDashboard.Collaboration.Merge;

/// <summary>
/// FIXME:  need logging - everywhere
/// </summary>
public class DefaultMergeHandler
{
    protected readonly MergeContext _mergeContext;
    public DefaultMergeHandler(MergeContext mergeContext)
    {
        _mergeContext = mergeContext;
    }

    public Expression<Func<T, bool>> BuildIdLookupWhereExpression<T>(IModelSnapshot<T> snapshot)
        where T : Models.IdentifiableEntity
    {
        var id = (Guid)snapshot.EntityPropertyValues["Id"]!;
        return e => e.Id == id;
    }

    protected virtual async Task HandleDeleteAsync<T>(T itemToDelete, CancellationToken cancellationToken)
        where T : IModelSnapshot
    {
        var modelSnapshot = (IModelSnapshot)itemToDelete;

        var where = new Dictionary<string, object>() { { modelSnapshot.IdentityKey, modelSnapshot.EntityPropertyValues[modelSnapshot.IdentityKey]! } };
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

    protected virtual void HandleCreateStart<T>(T itemToCreate) where T : IModelSnapshot
    {
        _mergeContext.MergeBehavior.StartInsertModelCommand(itemToCreate);
    }
    protected virtual async Task<Guid> HandleCreateAsync<T>(T itemToCreate, CancellationToken cancellationToken) where T : IModelSnapshot
    {
        var id = await _mergeContext.MergeBehavior.RunInsertModelCommand(itemToCreate, cancellationToken);
        return id;
    }
    protected virtual void HandleCreateComplete<T>(T itemToCreate) where T : IModelSnapshot
    {
        _mergeContext.MergeBehavior.CompleteInsertModelCommand(itemToCreate.EntityType);
    }

    protected bool CheckMergePropertyValueDifferences(IModelDifference modelDifference, IReadOnlyDictionary<string, object?> propertyValues)
    {
        var hasChange = false;

        foreach (var pd in modelDifference.PropertyDifferences
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ValueDifference))))
        {
            if (propertyValues.TryGetValue(pd.PropertyName, out var currentValue))
            {
                var vd = (ValueDifference)pd.PropertyValueDifference;
                if (vd.EqualsValue1(currentValue))
                {
                    _mergeContext.Logger.LogInformation($"Updating property '{pd.PropertyName}' current value '{currentValue}' to new value '{vd.Value2AsObject}'");
                    hasChange = true;
                }
                else if (!vd.EqualsValue2(currentValue))
                {
                    _mergeContext.Logger.LogInformation($"Conflict with property {pd.PropertyName} current value '{currentValue}' not matching last merge value '{vd.Value1AsObject}' or new value '{vd.Value2AsObject}'");
                    hasChange = true;
                    vd.ConflictValue = currentValue;
                }
            }
            else
            {
                throw new InvalidDifferenceStateException($"Property name '{pd.PropertyName}' from PropertyDifferences does not exist in the current snapshot");
            }
        }

        return hasChange;
    }

    protected bool CheckMergePropertyModelDifferences(IModelDifference modelDifference, IModelDistinguishable itemToModify)
    {
        var hasChange = false;

        foreach (var pd in modelDifference.PropertyDifferences
            .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ModelDifference))))
        {
            if (itemToModify.PropertyValues.TryGetValue(pd.PropertyName, out var v))
            {
                var md = (IModelDifference)pd.PropertyValueDifference;

                if (v is IModelDistinguishable)
                {
                    hasChange =  CheckMergePropertyModelDifferences(md, (IModelDistinguishable)v) || hasChange;
                }
                else if (v is IDictionary<string, object>)
                {
                    var propertyValues = ((IDictionary<string, object>)v)
                        .ToDictionary(e => e.Key, e => (object?)e.Value).AsReadOnly();
                    hasChange = CheckMergePropertyValueDifferences(md, propertyValues) || hasChange;
                }
                else
                {
                    throw new InvalidDifferenceStateException($"Unable to CheckMerge for property '{pd.PropertyName}' having item type {v?.GetType()?.ShortDisplayName()}");
                }
            }
            else
            {
                throw new InvalidDifferenceStateException($"Property name '{pd.PropertyName}' from ProjectDifferences snapshots does not exist in the current snapshot");
            }
        }

        return hasChange;
    }

    protected virtual ModelMergeResult CheckMerge<T>(IModelDifference modelDifference, T itemToModify)
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
            _mergeContext.Logger.LogInformation($"Merge conflict(s) detected:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");
            if (!_mergeContext.RemoteOverridesLocal)
            {
                throw new MergeConflictException(modelDifference);
            }
            return ModelMergeResult.ShouldMerge;
        }
        else if (hasChange)
        {
            _mergeContext.Logger.LogInformation($"Change(s) detected:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");
            return ModelMergeResult.ShouldMerge;
        }
        
        _mergeContext.Logger.LogInformation($"No change:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");
        return ModelMergeResult.NoChange;
    }

    public virtual async Task<bool> HandleModifyPropertiesAsync<T>(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
        where T : IModelDistinguishable
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

                var where = new Dictionary<string, object>() { { modelSnapshotToModify.IdentityKey, modelSnapshotToModify.EntityPropertyValues[modelSnapshotToModify.IdentityKey]! } };
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

    public async Task DeleteListDifferencesAsync<T>(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        if (listDifference.HasMembershipDeletes)
        {
            var handler = _mergeContext.FindMergeHandler<T>();

            // OnlyIn1: part of last commit merged, then deleted at some point on some remote
            //      if not in current:   they were deleted both here in some remote
            //      if in current:  delete (hard)
            foreach (var onlyIn1 in listDifference.OnlyIn1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var itemInCurrentSnapshot = currentSnapshotList.FindById<T>(onlyIn1.GetId());
                if (itemInCurrentSnapshot is not null)
                {
                    await handler.HandleDeleteAsync(itemInCurrentSnapshot, cancellationToken);
                }
                else
                {
                    _mergeContext.Logger.LogInformation($"Unable to delete - model item of type '{typeof(T).ShortDisplayName()}' having id '{onlyIn1}' not found in current snapshot");
                }
            }
        }
    }

    public async Task CreateListDifferencesAsync<T>(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        if (listDifference.HasMembershipCreates)
        {
            var handler = _mergeContext.FindMergeHandler<T>();

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
                    handler.HandleCreateStart(onlyIn2);
                    await handler.HandleCreateAsync(onlyIn2, cancellationToken);
                    handler.HandleCreateComplete(onlyIn2);

                    _mergeContext.Logger.LogInformation($"Inserted {onlyIn2.EntityType.ShortDisplayName()} having id '{onlyIn2.GetId()}'");

                    await handler.CreateChildrenAsync(onlyIn2, cancellationToken);
                }
                else
                {
                    var modelDifference = (IModelDifference<T>)itemInCurrentSnapshot.GetModelDifference((IModelDistinguishable<T>)onlyIn2);
                    await handler.HandleModifyPropertiesAsync<T>(modelDifference, itemInCurrentSnapshot, cancellationToken);
                }
            }
        }
    }

    public async Task ModifyListDifferencesAsync<T>(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, IEnumerable<T>? targetCommitSnapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        if (listDifference.HasListMemberModelDifferences)
        {
            var handler = _mergeContext.FindMergeHandler<T>();

            foreach (var modelDifference in listDifference.ListMemberModelDifferences)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Since this is a ModelDifference of an IModelSnapshot, its Id should
                // never be null:
                var itemInCurrentSnapshot = currentSnapshotList.FindById<T>(modelDifference.Id!);
                var itemInTargetCommitSnapshot = targetCommitSnapshotList.FindById<T>(modelDifference.Id!);

                if (itemInCurrentSnapshot is not null)
                {
                    await handler.HandleModifyPropertiesAsync(modelDifference, itemInCurrentSnapshot, cancellationToken);
                }
                else
                {
                    if (itemInTargetCommitSnapshot is not null)
                    {
                        // FIXME:  does this mean it got deleted locally but in the commits it
                        // was never deleted but modified instead?
                        handler.HandleCreateStart(itemInTargetCommitSnapshot);
                        await handler.HandleCreateAsync(itemInTargetCommitSnapshot, cancellationToken);
                        handler.HandleCreateComplete(itemInTargetCommitSnapshot);

                        _mergeContext.Logger.LogInformation($"Inserted {itemInTargetCommitSnapshot.EntityType.ShortDisplayName()} having id '{itemInTargetCommitSnapshot.GetId()}'");
                    }
                    else
                    {
                        throw new InvalidDifferenceStateException($"Model difference of type '{modelDifference.GetType().ShortDisplayName()}' exists for id {modelDifference.Id}, but IModelSnapshot not found in target commit snapshot.");
                    }
                }

                if (modelDifference.ChildListDifferences.Any())
                {
                    await handler.ChildListDifferenceAsync(modelDifference.ChildListDifferences, itemInCurrentSnapshot, itemInTargetCommitSnapshot, cancellationToken);
                }
            }
        }
    }

    public async Task<int> CreateListItemsAsync<T>(IEnumerable<T> snapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        var insertCount = 0;

        var handler = _mergeContext.FindMergeHandler<T>();
        var firstChild = snapshotList.FirstOrDefault();

        if (firstChild is not null)
        {
            handler.HandleCreateStart(firstChild);
            foreach (var modelSnapshot in snapshotList)
            {
                await handler.HandleCreateAsync(modelSnapshot, cancellationToken);
                insertCount++;
            }
            handler.HandleCreateComplete(firstChild);
        }

        return insertCount;
    }

    protected virtual async Task ChildListDifferenceAsync<T>(
        IReadOnlyDictionary<string, IListDifference> childListDifferences,
        T? parentItemInCurrentSnapshot,
        T? parentItemInTargetCommitSnapshot,
        CancellationToken cancellationToken)
        where T: IModelSnapshot
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
                MethodInfo deleteMethod = typeof(DefaultMergeHandler).GetMethod(nameof(DefaultMergeHandler.DeleteListDifferencesAsync))!;
                MethodInfo deleteMethodGeneric = deleteMethod.MakeGenericMethod(listDifference.GetType().GetGenericArguments()[0]);
                dynamic deleteAwaitable = deleteMethodGeneric.Invoke(this, new object?[] { listDifference, childrenInCurrentSnapshot, cancellationToken })!;
                await deleteAwaitable;

                MethodInfo createMethod = typeof(DefaultMergeHandler).GetMethod(nameof(DefaultMergeHandler.CreateListDifferencesAsync))!;
                MethodInfo createMethodGeneric = createMethod.MakeGenericMethod(listDifference.GetType().GetGenericArguments()[0]);
                dynamic createAwaitable = createMethodGeneric.Invoke(this, new object?[] { listDifference, childrenInCurrentSnapshot, cancellationToken })!;
                await createAwaitable;

                MethodInfo modifyMethod = typeof(DefaultMergeHandler).GetMethod(nameof(DefaultMergeHandler.ModifyListDifferencesAsync))!;
                MethodInfo modifyMethodGeneric = modifyMethod.MakeGenericMethod(listDifference.GetType().GetGenericArguments()[0]);
                dynamic modifyAwaitable = modifyMethodGeneric.Invoke(this, new object?[] { listDifference, childrenInCurrentSnapshot, childrenInTargetCommitSnapshot, cancellationToken })!;
                await modifyAwaitable;
            }
            else
            {
                throw new InvalidDifferenceStateException($"Encountered child IListDifference without any generic model type!");
            }
        }
    }

    protected async Task HandleListDifferenceGroup<T>(
        IListDifference listDifference,
        IEnumerable<IModelDistinguishable>? childrenInCurrentSnapshot,
        IEnumerable<IModelDistinguishable>? childrenInTargetCommitSnapshot,
        CancellationToken cancellationToken
        ) where T: IModelSnapshot
    {
        if (!listDifference.GetType().IsAssignableToGenericType(typeof(IListDifference<>)))
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

    protected virtual async Task CreateChildrenAsync<T>(
        T parentSnapshot,
        CancellationToken cancellationToken)
        where T : IModelSnapshot
    {
        foreach (var childName in parentSnapshot.Children.Keys)
        {
            _mergeContext.Logger.LogInformation($"Inserting {childName} children for {parentSnapshot.EntityType.ShortDisplayName()} '{parentSnapshot.GetId()}'");

            Type childType = parentSnapshot.Children[childName].GetType().GetGenericArguments()[0];
            if (childType.IsAssignableToGenericType(typeof(IModelSnapshot<>)) && childType.GetGenericArguments().Any())
            {
                var t = typeof(IModelSnapshot<>);
                Type[] typeArgs = { childType.GetGenericArguments()[0] };
                childType = t.MakeGenericType(typeArgs);
            }
            MethodInfo createMethod = typeof(DefaultMergeHandler).GetMethod(nameof(DefaultMergeHandler.CreateListItemsAsync), BindingFlags.Instance | BindingFlags.Public)!;
            MethodInfo createMethodGeneric = createMethod.MakeGenericMethod(childType);
            dynamic createAwaitable = createMethodGeneric.Invoke(this, new object?[] { parentSnapshot.Children[childName], cancellationToken })!;
            var insertCount = await createAwaitable;

            _mergeContext.Logger.LogInformation($"Completed inserting {insertCount} {childName} children for {parentSnapshot.EntityType.ShortDisplayName()} '{parentSnapshot.GetId()}'");
        }
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
        if (!cache.ContainsKey((typeof(Models.TokenizedCorpus), tokenizedCorpusId.ToString())))
        {
            var tokenLocationIds = projectDbContext.TokenComponents
                .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                .Where(e => e.EngineTokenId != null)
                .ToDictionary(e => e.EngineTokenId!, e => (object?)e.Id);

            cache.AddCacheEntrySet((typeof(Models.TokenizedCorpus), tokenizedCorpusId.ToString()), tokenLocationIds);
        }
    }
}
