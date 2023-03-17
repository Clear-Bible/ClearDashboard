using System;
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
            await handler.HandleModifyPropertiesAsync<IModelSnapshot<Models.Project>>(differencesToApply.Project, currentSnapshot.Project, cancellationToken);
        }

        var reporter = new PhasedProgressReporter(MergeContext.Progress,
            new Phase("Applying Deletes"),
            new Phase("Applying Creates"),
            new Phase("Applying Modifications"));

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // We do deletes in reverse order for obvious reasons:
            await handler.DeleteListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, cancellationToken);
            await handler.DeleteListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // Order these are done is important since later model types
            // are dependent upon earlier model types:
            await handler.CreateListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, cancellationToken);
            await handler.CreateListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, cancellationToken);
        }

        using (PhaseProgress phaseProgress = reporter.StartNextPhase())
        {
            // Order these are done is important since later model types
            // are dependent upon earlier model types:
            await handler.ModifyListDifferencesAsync(differencesToApply.Users, currentSnapshot.Users, targetCommitSnapshot.Users, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.Corpora, currentSnapshot.Corpora, targetCommitSnapshot.Corpora, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.TokenizedCorpora, currentSnapshot.TokenizedCorpora, targetCommitSnapshot.TokenizedCorpora, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.ParallelCorpora, currentSnapshot.ParallelCorpora, targetCommitSnapshot.ParallelCorpora, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.AlignmentSets, currentSnapshot.AlignmentSets, targetCommitSnapshot.AlignmentSets, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.TranslationSets, currentSnapshot.TranslationSets, targetCommitSnapshot.TranslationSets, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.Notes, currentSnapshot.Notes, targetCommitSnapshot.Notes, cancellationToken);
            await handler.ModifyListDifferencesAsync(differencesToApply.Labels, currentSnapshot.Labels, targetCommitSnapshot.Labels, cancellationToken);
        }
    }
}

