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

    protected virtual async Task<Guid> HandleCreateAsync<T>(T itemToCreate, CancellationToken cancellationToken)
        where T : IModelSnapshot
    {
        _mergeContext.MergeBehavior.StartInsertModelCommand(itemToCreate);
        var id = await _mergeContext.MergeBehavior.RunInsertModelCommand(itemToCreate, cancellationToken);
        _mergeContext.MergeBehavior.CompleteInsertModelCommand(itemToCreate.EntityType);

        return id;
    }

    protected virtual async Task HandleChildListDifferenceAsync<T>(string childName,
        IListDifference listDifference,
        T? parentItemInCurrentSnapshot,
        T? parentItemInTargetCommitSnapshot,
        CancellationToken cancellationToken)
        where T : IModelSnapshot
    {
        // Gives any non-default handler the chance to handle changes for
        // an entire child list (if overridden), otherwise, calls the default
        // ChildListDifference method to handle individual list item deletes,
        // creates and modifications.  
        await ChildListDifferenceAsync(childName, listDifference, parentItemInCurrentSnapshot, parentItemInTargetCommitSnapshot, cancellationToken);
    }

    protected virtual ModelMergeResult CheckMerge<T>(IModelDifference modelDifference, T itemToModify)
        where T : IModelDistinguishable
    {
        var modelMergeResult = ModelMergeResult.Unset;
        if (modelDifference.HasDifferences)
        {
            _mergeContext.Logger.LogInformation($"Checking merge of: '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");

            foreach (var pd in modelDifference.PropertyDifferences
                .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ValueDifference))))
            {
                if (itemToModify.PropertyValues.TryGetValue(pd.PropertyName, out var currentValue))
                {
                    var vd = (ValueDifference)pd.PropertyValueDifference;
                    if (vd.EqualsValue1(currentValue))
                    {
                        _mergeContext.Logger.LogInformation($"Updating property '{pd.PropertyName}' current value '{currentValue}' to new value '{vd.Value2AsObject}'");
                        modelMergeResult = ModelMergeResult.ShouldMerge;
                    }
                    else if (!vd.EqualsValue2(currentValue))
                    {
                        _mergeContext.Logger.LogInformation($"Conflict with property {pd.PropertyName} current value '{currentValue}' not matching last merge value '{vd.Value1AsObject}' or new value '{vd.Value2AsObject}'");
                        modelMergeResult = ModelMergeResult.Conflict;
                        break;  // FIXME:  maybe throw a ModelMergeConflictException so the caller has details
                    }
                }
                else
                {
                    throw new InvalidDifferenceStateException($"Property name '{pd.PropertyName}' from PropertyDifferences does not exist in the current snapshot");
                }
            }

            foreach (var pd in modelDifference.PropertyDifferences
                .Where(pd => pd.PropertyValueDifference.GetType().IsAssignableTo(typeof(ModelDifference))))
            {
                if (itemToModify.PropertyValues.TryGetValue(pd.PropertyName, out var v))
                {
                    if (v is IModelDistinguishable)
                    {
                        var md = (IModelDifference)pd.PropertyValueDifference;
                        var currentValue = (IModelDistinguishable)v;

                        var mergeResult = CheckMerge(md, currentValue);

                        if (mergeResult == ModelMergeResult.Conflict || mergeResult == ModelMergeResult.ShouldMerge)
                        {
                            modelMergeResult = mergeResult;
                        }
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
        }

        if (modelMergeResult == ModelMergeResult.Unset)
        {
            _mergeContext.Logger.LogInformation($"No change:  '{itemToModify.GetType().ShortDisplayName()}' item '{itemToModify}'");
            modelMergeResult = ModelMergeResult.NoChange;
        }

        return modelMergeResult;
    }

    public virtual async Task HandleModifyPropertiesAsync<T>(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
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
            }
        }
        else
        {
            throw new NotImplementedException($"Derived merge handler with '{typeof(T).ShortDisplayName()}' model-specific HandleModifyProperties functionality");
        }
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
                    await handler.HandleCreateAsync(onlyIn2, cancellationToken);
                }
                else
                {
                    var modelDifference = (IModelDifference<T>)onlyIn2.GetModelDifference((IModelDistinguishable<T>)itemInCurrentSnapshot);
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
                        await handler.HandleCreateAsync(itemInTargetCommitSnapshot, cancellationToken);
                    }
                    else
                    {
                        throw new InvalidDifferenceStateException($"Model difference of type '{modelDifference.GetType().ShortDisplayName()}' exists for id {modelDifference.Id}, but IModelSnapshot not found in target commit snapshot.");
                    }
                }

                if (modelDifference.ChildListDifferences.Any())
                {
                    foreach (var kvp in modelDifference.ChildListDifferences)
                    {
                        await handler.HandleChildListDifferenceAsync(kvp.Key, kvp.Value, itemInCurrentSnapshot, itemInTargetCommitSnapshot, cancellationToken);
                    }
                }
            }
        }
    }

    protected async Task ChildListDifferenceAsync<T>(
        string childName,
        IListDifference listDifference,
        T? parentItemInCurrentSnapshot,
        T? parentItemInTargetCommitSnapshot,
        CancellationToken cancellationToken)
        where T: IModelSnapshot
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

    public static Guid LookupTokenComponent(ProjectDbContext projectDbContext, Guid tokenizedCorpusId, string engineTokenId, MergeCache cache)
    {
        var tokenComponentId = projectDbContext.TokenComponents
            .Where(e => e.EngineTokenId == engineTokenId!)
            .Where(e => e.TokenizedCorpusId == tokenizedCorpusId!)
            .Select(e => e.Id)
            .FirstOrDefault();

        if (tokenComponentId == default(Guid)) throw new InvalidModelStateException($"Invalid EngineTokenId '{engineTokenId}' + TokenizedCorpusId '{tokenizedCorpusId}' - TokenComponentId not found");

        return tokenComponentId!;
    }
}
