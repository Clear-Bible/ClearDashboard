﻿using System;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Alignment.Features.Denormalization;
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

    public async Task MergeAsync(ProjectDifferences differencesToApply, ProjectSnapshot currentSnapshot, ProjectSnapshot targetCommitSnapshot, CancellationToken cancellationToken = default)
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
            new Phase("Merging create/modify activity ... Users, Corpora, and Tokenized Corpora"),
            new Phase("Merging create/modify activity ... Parallel Corpora, Alignment Sets, and Translation Sets"),
            new Phase("Merging create/modify activity ... Notes and Labels"));

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // We do deletes in reverse order for obvious reasons:
            await DeleteListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, cancellationToken);
            await DeleteListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // Order these are done is important since later model types
            // are dependent upon earlier model types:
            await CreateListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, targetCommitSnapshot.Users, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, targetCommitSnapshot.Corpora, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, targetCommitSnapshot.TokenizedCorpora, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            await CreateListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, targetCommitSnapshot.ParallelCorpora, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, targetCommitSnapshot.AlignmentSets, cancellationToken);

            await CreateListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, targetCommitSnapshot.TranslationSets, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            await CreateListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, targetCommitSnapshot.Notes, cancellationToken);
            await CreateListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, cancellationToken);
            await ModifyListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, targetCommitSnapshot.Labels, cancellationToken);
        }
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

    private async Task ModifyListDifferencesAsync<T>(IListDifference<T> listDifference, IEnumerable<T>? currentSnapshotList, IEnumerable<T>? targetCommitSnapshotList, CancellationToken cancellationToken = default)
        where T : IModelSnapshot
    {
        var handler = MergeContext.FindMergeHandler<T>();
        await handler.ModifyListDifferencesAsync(listDifference, currentSnapshotList, targetCommitSnapshotList, cancellationToken);
    }
}
