using System;
using System.Diagnostics;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data.Migrations;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;
namespace ClearDashboard.Collaboration.Merge;

public class Merger
{
	public MergeContext MergeContext { get; private set; }
	public Merger(MergeContext mergeContext)
	{
		MergeContext = mergeContext;
	}

    public async Task MergeAsync(ProjectDifferences differencesToApply, ProjectSnapshot currentSnapshot, ProjectSnapshot targetCommitSnapshot, ProjectSnapshot previousCommitSnapshot, CancellationToken cancellationToken = default)
    {
        // Here is where we start the merge process.  'differencesToApply' are differences between two commits (the commit
        // we are updating to is 'targetCommitSnapshot').  'currentSnapshot' is the current state of our database.  

        var handler = MergeContext.DefaultMergeHandler;

        if (differencesToApply.Project.HasDifferences)
        {
            var projectHandler = MergeContext.FindMergeHandler<IModelSnapshot<Models.Project>>();
            await projectHandler.HandleModifyPropertiesAsync(differencesToApply.Project, currentSnapshot.Project, cancellationToken);
        }

        var reporter = new PhasedProgressReporter(MergeContext.Progress,
            new Phase("Merging delete activity ..."),
            new Phase("Merging create/modify activity ... Users, Lexicon, Corpora, and Tokenized Corpora"),
            new Phase("Merging create/modify activity ... Parallel Corpora, Alignment Sets, and Translation Sets"),
            new Phase("Merging create/modify activity ... Notes and Labels"));

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // We do deletes in reverse order for obvious reasons:
            await DeleteListDifferencesAsync(differencesToApply.LabelGroups, currentSnapshot.LabelGroups, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.LexiconSemanticDomains, currentSnapshot.LexiconSemanticDomains, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.LexiconLexemes, currentSnapshot.LexiconLexemes, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // Order these are done is important since later model types
            // are dependent upon earlier model types:
            await CreateListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, targetCommitSnapshot.Users, previousCommitSnapshot.Users, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.LexiconLexemes, currentSnapshot.LexiconLexemes, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.LexiconLexemes, currentSnapshot.LexiconLexemes, targetCommitSnapshot.LexiconLexemes, previousCommitSnapshot.LexiconLexemes, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.LexiconSemanticDomains, currentSnapshot.LexiconSemanticDomains, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.LexiconSemanticDomains, currentSnapshot.LexiconSemanticDomains, targetCommitSnapshot.LexiconSemanticDomains, previousCommitSnapshot.LexiconSemanticDomains, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, targetCommitSnapshot.Corpora, previousCommitSnapshot.Corpora, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, targetCommitSnapshot.TokenizedCorpora, previousCommitSnapshot.TokenizedCorpora, cancellationToken);

            // After Tokens are added
            AddDifferencesFromTokenComponentSoftDeleteChanges(differencesToApply, currentSnapshot, targetCommitSnapshot, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            await CreateListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, targetCommitSnapshot.ParallelCorpora, previousCommitSnapshot.ParallelCorpora, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, targetCommitSnapshot.AlignmentSets, previousCommitSnapshot.AlignmentSets, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, targetCommitSnapshot.TranslationSets, previousCommitSnapshot.TranslationSets, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            await CreateListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, targetCommitSnapshot.Notes, previousCommitSnapshot.Notes, cancellationToken);
            await CreateListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, targetCommitSnapshot.Labels, previousCommitSnapshot.Labels, cancellationToken);
            await CreateListDifferencesAsync(differencesToApply.LabelGroups, currentSnapshot.LabelGroups, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.LabelGroups, currentSnapshot.LabelGroups, targetCommitSnapshot.LabelGroups, previousCommitSnapshot.LabelGroups, cancellationToken);
        }

        await TriggerDenormalizationIfNeeded(currentSnapshot, cancellationToken);
    }

    private async Task DeleteListDifferencesAsync<T>(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        var handler = MergeContext.FindMergeHandler<T>();
        await handler.DeleteListDifferencesAsync(listDifference, currentSnapshotList, cancellationToken);
    }

    private async Task CreateListDifferencesAsync<T>(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        var handler = MergeContext.FindMergeHandler<T>();
        await handler.CreateListDifferencesAsync(listDifference, currentSnapshotList, cancellationToken);
    }

    private async Task ModifyListDifferencesAsync<T>(
        IListDifference<T> listDifference, 
        IEnumerable<T>? currentSnapshotList, 
        IEnumerable<T>? targetCommitSnapshotList,
        IEnumerable<T>? previousCommitSnapshotList, 
        CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        var handler = MergeContext.FindMergeHandler<T>();
        await handler.ModifyListDifferencesAsync(listDifference, currentSnapshotList, targetCommitSnapshotList, previousCommitSnapshotList, cancellationToken);
    }

    private async Task TriggerDenormalizationIfNeeded(ProjectSnapshot projectSnapshot, CancellationToken cancellationToken)
    {
        var pctc = projectSnapshot.ParallelCorpora.Select(e =>
        {
            e.TryGetGuidPropertyValue(nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId);
            e.TryGetGuidPropertyValue(nameof(Models.ParallelCorpus.TargetTokenizedCorpusId), out var targetTokenizedCorpusId);
            return (ParallelCorpusId: (Guid)e.GetId(), sourceTokenizedCorpusId, targetTokenizedCorpusId);
        })
        .ToDictionary(e => e.ParallelCorpusId, e => (e.sourceTokenizedCorpusId, e.targetTokenizedCorpusId));

        // Source and Target TokenizedCorpusIds pairs mapped to AlignmentSet snapshots:
        var alignmentSetIdsByTokenizedCorpusId = projectSnapshot.AlignmentSets.SelectMany(e =>
        {
            e.TryGetGuidPropertyValue(nameof(Models.AlignmentSet.ParallelCorpusId), out var parallelCorpusId);
            if (pctc.TryGetValue(parallelCorpusId, out var tcs))
            {
                return new (Guid, Guid)[] { ((Guid)e.GetId(), tcs.sourceTokenizedCorpusId), ((Guid)e.GetId(), tcs.targetTokenizedCorpusId) };
            }
            throw new PropertyResolutionException($"AlignmentSet '{e.GetId()}' references ParallelCorpus '{parallelCorpusId}', which was not found in target commit snapshot");
        })
        .GroupBy(e => e.Item2)
        .ToDictionary(g => g.Key, g => g.Select(e => e.Item1));

        var alignmentSetIds = new List<Guid>();
        foreach (var (EntityType, DatabaseId) in MergeContext.MergeBehavior.MergeCache.IdsToDenormalize)
        {
            if (EntityType.IsAssignableTo(typeof(IModelSnapshot<Models.TokenizedCorpus>)))
            {
                if (alignmentSetIdsByTokenizedCorpusId.TryGetValue(DatabaseId, out var ids))
                {
                    alignmentSetIds.AddRange(ids);
                }
            }
            else
            {
                alignmentSetIds.Add(DatabaseId);
            }
        }

        List<GeneralModel<Models.AlignmentSetDenormalizationTask>> alignmentSetDenormalizationTasks = new();
        foreach (var alignmentSetId in alignmentSetIds.Distinct())
        {
            var t = new GeneralModel<Models.AlignmentSetDenormalizationTask>(nameof(Models.AlignmentSetDenormalizationTask.Id), Guid.NewGuid());
            t.Add(nameof(Models.AlignmentSetDenormalizationTask.AlignmentSetId), alignmentSetId);
            t.Add(nameof(Models.AlignmentSetDenormalizationTask.SourceText), (string?)null);

            alignmentSetDenormalizationTasks.Add(t);
        }

        if (alignmentSetDenormalizationTasks.Any())
        {
            MergeContext.MergeBehavior.StartInsertModelCommand(alignmentSetDenormalizationTasks.First());
            foreach (var child in alignmentSetDenormalizationTasks)
            {
                _ = await MergeContext.MergeBehavior.RunInsertModelCommand((IModelSnapshot)child, cancellationToken);
            }
            MergeContext.MergeBehavior.CompleteInsertModelCommand(typeof(Models.AlignmentSetDenormalizationTask));

            MergeContext.FireAlignmentDenormalizationEvent = true;
        }
    }

    private void AddDifferencesFromTokenComponentSoftDeleteChanges(ProjectDifferences differencesToApply, ProjectSnapshot currentSnapshot, ProjectSnapshot targetCommitSnapshot, CancellationToken cancellationToken = default)
    {
        var cache = MergeContext.MergeBehavior.MergeCache;

        var tokenizedCorpusIds = cache.GetCacheEntrySetEntityIds(typeof(Models.TokenizedCorpus), DefaultMergeHandler.TOKEN_DELETE_CHANGES_CACHE_SET_NAME);
        if (!tokenizedCorpusIds.Any())
        {
            return;
        }

        ProcessAlignments(differencesToApply, targetCommitSnapshot, cache);
        ProcessTranslations(differencesToApply, targetCommitSnapshot, cache);
    }

    private static void ProcessAlignments(ProjectDifferences differencesToApply, ProjectSnapshot targetCommitSnapshot, MergeCache cache)
    {
        var astcs =
            GetAlignmentSetTokenDeleteSourcesTargets(targetCommitSnapshot, cache);

        foreach (var astc in astcs)
        {
            var alignmentSetId = (Guid)astc.alignmentSetSnapshot.GetId();

            var sourceTokenLocations = astc.sources.Keys;
            var targetTokenLocations = astc.targets.Keys;

            if (sourceTokenLocations.Any() || targetTokenLocations.Any())
            {
                if (astc.alignmentSetSnapshot.Children.TryGetValue(AlignmentSetBuilder.ALIGNMENTS_CHILD_NAME, out var children))
                {
                    var alignmentSnapshots = (IEnumerable<IModelSnapshot<Models.Alignment>>)children;

                    ListDifference<IModelSnapshot<Models.Alignment>>? alignmentListDifference = null;
                    ModelDifference<IModelSnapshot<Models.AlignmentSet>>? alignmentSetModelDifference = null;

                    foreach (var alignmentSnapshot in alignmentSnapshots)
                    {
                        ModelDifference<IModelSnapshot<Models.Alignment>>? alignmentModelDifference = null;

                        alignmentSetModelDifference ??= differencesToApply.AlignmentSets.ListMemberModelDifferences
                            .Where(e => (Guid)e.Id! == alignmentSetId)
                            .FirstOrDefault() as ModelDifference<IModelSnapshot<Models.AlignmentSet>>;

                        if (alignmentSetModelDifference is not null && alignmentSetModelDifference.ChildListDifferences.TryGetValue(AlignmentSetBuilder.ALIGNMENTS_CHILD_NAME, out var childDiffs))
                        {
                            alignmentListDifference = (ListDifference<IModelSnapshot<Models.Alignment>>)childDiffs;
                            alignmentModelDifference = alignmentListDifference.ListMemberModelDifferences
                                .Where(e => (string)e.Id! == (string)alignmentSnapshot.GetId())
                                .FirstOrDefault()
                                as ModelDifference<IModelSnapshot<Models.Alignment>>;
                        }

                        var (sourceTokenLocation, targetTokenLocation) = AlignmentNeedToAdd(alignmentModelDifference, alignmentSnapshot, astc.sources, astc.targets);

                        if (sourceTokenLocation is not null || targetTokenLocation is not null)
                        {
                            if (alignmentModelDifference is null)
                            {
                                alignmentModelDifference = new ModelDifference<IModelSnapshot<Models.Alignment>>(typeof(Models.Alignment), (string)alignmentSnapshot.GetId());

                                alignmentListDifference ??= new(
                                    new ListMembershipDifference<IModelSnapshot<Models.Alignment>>(Enumerable.Empty<IModelSnapshot<Models.Alignment>>(), Enumerable.Empty<IModelSnapshot<Models.Alignment>>()),
                                    Enumerable.Empty<IModelDifference<IModelSnapshot<Models.Alignment>>>());
                                alignmentListDifference.AddListMemberModelDifference(alignmentModelDifference);

                                if (alignmentSetModelDifference is null)
                                {
                                    alignmentSetModelDifference = new ModelDifference<IModelSnapshot<Models.AlignmentSet>>(typeof(Models.AlignmentSet), alignmentSetId);
                                    alignmentSetModelDifference.AddChildListDifference(AlignmentSetBuilder.ALIGNMENTS_CHILD_NAME, alignmentListDifference);
                                    ((ListDifference<IModelSnapshot<Models.AlignmentSet>>)differencesToApply.AlignmentSets).AddListMemberModelDifference(alignmentSetModelDifference);
                                }
                            }
                        }

                        AlignmentAddIfNeeded(sourceTokenLocation, targetTokenLocation, alignmentModelDifference!);
                    }
                }
            }
        }

    }

    private static void ProcessTranslations(ProjectDifferences differencesToApply, ProjectSnapshot targetCommitSnapshot, MergeCache cache)
    {
        var astcs =
            GetTranslationSetTokenDeleteSourcesTargets(targetCommitSnapshot, cache);

        foreach (var astc in astcs)
        {
            var translationSetId = (Guid)astc.translationSetSnapshot.GetId();

            var sourceTokenLocations = astc.sources.Keys;

            if (sourceTokenLocations.Any())
            {
                if (astc.translationSetSnapshot.Children.TryGetValue(TranslationSetBuilder.TRANSLATIONS_CHILD_NAME, out var children))
                {
                    var translationSnapshots = (IEnumerable<IModelSnapshot<Models.Translation>>)children;

                    ListDifference<IModelSnapshot<Models.Translation>>? translationListDifference = null;
                    ModelDifference<IModelSnapshot<Models.TranslationSet>>? translationSetModelDifference = null;

                    foreach (var translationSnapshot in translationSnapshots)
                    {
                        ModelDifference<IModelSnapshot<Models.Translation>>? translationModelDifference = null;

                        translationSetModelDifference ??= differencesToApply.TranslationSets.ListMemberModelDifferences
                            .Where(e => (Guid)e.Id! == translationSetId)
                            .FirstOrDefault() as ModelDifference<IModelSnapshot<Models.TranslationSet>>;

                        if (translationSetModelDifference is not null && translationSetModelDifference.ChildListDifferences.TryGetValue(TranslationSetBuilder.TRANSLATIONS_CHILD_NAME, out var childDiffs))
                        {
                            translationListDifference = (ListDifference<IModelSnapshot<Models.Translation>>)childDiffs;
                            translationModelDifference = translationListDifference.ListMemberModelDifferences
                                .Where(e => (string)e.Id! == (string)translationSnapshot.GetId())
                                .FirstOrDefault()
                                as ModelDifference<IModelSnapshot<Models.Translation>>;
                        }

                        var sourceTokenLocation = TranslationNeedToAdd(translationModelDifference, translationSnapshot, astc.sources);

                        if (sourceTokenLocation is not null)
                        {
                            if (translationModelDifference is null)
                            {
                                translationModelDifference = new ModelDifference<IModelSnapshot<Models.Translation>>(typeof(Models.Translation), (string)translationSnapshot.GetId());

                                translationListDifference ??= new(
                                    new ListMembershipDifference<IModelSnapshot<Models.Translation>>(Enumerable.Empty<IModelSnapshot<Models.Translation>>(), Enumerable.Empty<IModelSnapshot<Models.Translation>>()),
                                    Enumerable.Empty<IModelDifference<IModelSnapshot<Models.Translation>>>());
                                translationListDifference.AddListMemberModelDifference(translationModelDifference);

                                if (translationSetModelDifference is null)
                                {
                                    translationSetModelDifference = new ModelDifference<IModelSnapshot<Models.TranslationSet>>(typeof(Models.TranslationSet), translationSetId);
                                    translationSetModelDifference.AddChildListDifference(TranslationSetBuilder.TRANSLATIONS_CHILD_NAME, translationListDifference);
                                    ((ListDifference<IModelSnapshot<Models.TranslationSet>>)differencesToApply.TranslationSets).AddListMemberModelDifference(translationSetModelDifference);
                                }
                            }
                        }

                        TranslationAddIfNeeded(sourceTokenLocation, translationModelDifference!);
                    }
                }
            }
        }

    }

    private static void AlignmentAddIfNeeded(string? sourceTokenLocation, string? targetTokenLocation, ModelDifference<IModelSnapshot<Models.Alignment>> alignmentModelDifference)
    {
        if (sourceTokenLocation is not null)
        {
            alignmentModelDifference!.AddPropertyDifference(new PropertyDifference(
                AlignmentBuilder.SOURCE_TOKEN_LOCATION,
                new ValueDifference<string>(null, sourceTokenLocation)));
        }

        if (targetTokenLocation is not null)
        {
            alignmentModelDifference!.AddPropertyDifference(new PropertyDifference(
                AlignmentBuilder.TARGET_TOKEN_LOCATION,
                new ValueDifference<string>(null, targetTokenLocation)));
        }
    }

    private static void TranslationAddIfNeeded(string? sourceTokenLocation, ModelDifference<IModelSnapshot<Models.Translation>> translationModelDifference)
    {
        if (sourceTokenLocation is not null)
        {
            translationModelDifference!.AddPropertyDifference(new PropertyDifference(
                AlignmentBuilder.SOURCE_TOKEN_LOCATION,
                new ValueDifference<string>(null, sourceTokenLocation)));
        }
    }

    private static (string? sourceTokenLocation, string? targetTokenLocation) AlignmentNeedToAdd(ModelDifference<IModelSnapshot<Models.Alignment>>? alignmentModelDifference, IModelSnapshot<Models.Alignment> alignmentSnapshot, Dictionary<string, object?> sources, Dictionary<string, object?> targets)
    {
        alignmentSnapshot.TryGetStringPropertyValue(AlignmentBuilder.SOURCE_TOKEN_LOCATION, out var sourceTokenLocation);
        alignmentSnapshot.TryGetStringPropertyValue(AlignmentBuilder.TARGET_TOKEN_LOCATION, out var targetTokenLocation);
        alignmentSnapshot.TryGetPropertyValue(AlignmentBuilder.SOURCE_TOKEN_DELETED, out var sourceTokenDeleted);
        alignmentSnapshot.TryGetPropertyValue(AlignmentBuilder.TARGET_TOKEN_DELETED, out var targetTokenDeleted);

        var needToAddSource = false;
        if (sources.ContainsKey(sourceTokenLocation) && (bool)sources[sourceTokenLocation]! != (bool)sourceTokenDeleted!)
        {
            needToAddSource = true;
            if (alignmentModelDifference is not null && alignmentModelDifference.PropertyDifferences.Any(
                    e => e.PropertyName == AlignmentBuilder.SOURCE_TOKEN_LOCATION ||
                    e.PropertyName == AlignmentBuilder.SOURCE_TOKEN_DELETED))
            {
                needToAddSource = false;
            }
        }

        var needToAddTarget = false;
        if (targets.ContainsKey(targetTokenLocation) && (bool)targets[targetTokenLocation]! != (bool)targetTokenDeleted!)
        {
            needToAddTarget = true;
            if (alignmentModelDifference is not null && alignmentModelDifference.PropertyDifferences.Any(
                    e => e.PropertyName == AlignmentBuilder.TARGET_TOKEN_LOCATION ||
                    e.PropertyName == AlignmentBuilder.TARGET_TOKEN_DELETED))
            {
                needToAddTarget = false;
            }
        }

        return (needToAddSource ? sourceTokenLocation : null, needToAddTarget ? targetTokenLocation : null);
    }

    private static string? TranslationNeedToAdd(ModelDifference<IModelSnapshot<Models.Translation>>? translationModelDifference, IModelSnapshot<Models.Translation> translationSnapshot, Dictionary<string, object?> sources)
    {
        translationSnapshot.TryGetStringPropertyValue(AlignmentBuilder.SOURCE_TOKEN_LOCATION, out var sourceTokenLocation);
        translationSnapshot.TryGetPropertyValue(AlignmentBuilder.SOURCE_TOKEN_DELETED, out var sourceTokenDeleted);

        var needToAddSource = false;
        if (sources.ContainsKey(sourceTokenLocation) && (bool)sources[sourceTokenLocation]! != (bool)sourceTokenDeleted!)
        {
            needToAddSource = true;
            if (translationModelDifference is not null && translationModelDifference.PropertyDifferences.Any(
                    e => e.PropertyName == AlignmentBuilder.SOURCE_TOKEN_LOCATION ||
                    e.PropertyName == AlignmentBuilder.SOURCE_TOKEN_DELETED))
            {
                needToAddSource = false;
            }
        }

        return needToAddSource ? sourceTokenLocation : null;
    }

    private static IEnumerable<(IModelSnapshot<Models.AlignmentSet> alignmentSetSnapshot, Dictionary<string, object?> sources, Dictionary<string, object?> targets)> GetAlignmentSetTokenDeleteSourcesTargets(ProjectSnapshot projectSnapshot, MergeCache cache)
    {
        // ParallelCorpusIds mapped to Source and Target TokenizedCorpusIds:
        var pctc = projectSnapshot.ParallelCorpora.Select(e =>
        {
            e.TryGetGuidPropertyValue(nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId);
            e.TryGetGuidPropertyValue(nameof(Models.ParallelCorpus.TargetTokenizedCorpusId), out var targetTokenizedCorpusId);
            return (ParallelCorpusId: (Guid)e.GetId(), sourceTokenizedCorpusId, targetTokenizedCorpusId);
        })
        .ToDictionary(e => e.ParallelCorpusId, e => (e.sourceTokenizedCorpusId, e.targetTokenizedCorpusId));

        // Source and Target TokenizedCorpusIds pairs mapped to AlignmentSet snapshots:
        var astcs = projectSnapshot.AlignmentSets.Select(e =>
        {
            e.TryGetGuidPropertyValue(nameof(Models.AlignmentSet.ParallelCorpusId), out var parallelCorpusId);
            if (pctc.TryGetValue(parallelCorpusId, out var tcs))
            {
                cache.TryLookupCacheEntrySet(DefaultMergeHandler.TokenDeleteChangesCacheKey(tcs.sourceTokenizedCorpusId.ToString()), out var sourceEngineTokenIdDeleteChanges);
                cache.TryLookupCacheEntrySet(DefaultMergeHandler.TokenDeleteChangesCacheKey(tcs.targetTokenizedCorpusId.ToString()), out var targetEngineTokenIdDeleteChanges);

                return (alignmentSetSnapshot: e, sources: sourceEngineTokenIdDeleteChanges ?? new(), targets: targetEngineTokenIdDeleteChanges ?? new());
            }
            throw new PropertyResolutionException($"AlignmentSet '{e.GetId()}' references ParallelCorpus '{parallelCorpusId}', which was not found in target commit snapshot");
        });

        return astcs.Where(e => e.sources.Any() || e.targets.Any());
    }

    private static IEnumerable<(IModelSnapshot<Models.TranslationSet> translationSetSnapshot, Dictionary<string, object?> sources)> GetTranslationSetTokenDeleteSourcesTargets(ProjectSnapshot projectSnapshot, MergeCache cache)
    {
        // ParallelCorpusIds mapped to Source and Target TokenizedCorpusIds:
        var pctc = projectSnapshot.ParallelCorpora.Select(e =>
        {
            e.TryGetGuidPropertyValue(nameof(Models.ParallelCorpus.SourceTokenizedCorpusId), out var sourceTokenizedCorpusId);
            return (ParallelCorpusId: (Guid)e.GetId(), sourceTokenizedCorpusId);
        })
        .ToDictionary(e => e.ParallelCorpusId, e => e.sourceTokenizedCorpusId);

        // Source and Target TokenizedCorpusIds pairs mapped to TranslationSet snapshots:
        var astcs = projectSnapshot.TranslationSets.Select(e =>
        {
            e.TryGetGuidPropertyValue(nameof(Models.TranslationSet.ParallelCorpusId), out var parallelCorpusId);
            if (pctc.TryGetValue(parallelCorpusId, out var sourceTokenizedCorpusId))
            {
                cache.TryLookupCacheEntrySet(DefaultMergeHandler.TokenDeleteChangesCacheKey(sourceTokenizedCorpusId.ToString()), out var sourceEngineTokenIdDeleteChanges);
                return (translationSetSnapshot: e, sources: sourceEngineTokenIdDeleteChanges ?? new());
            }
            throw new PropertyResolutionException($"TranslationSet '{e.GetId()}' references ParallelCorpus '{parallelCorpusId}', which was not found in target commit snapshot");
        });

        return astcs.Where(e => e.sources.Any());
    }
}

