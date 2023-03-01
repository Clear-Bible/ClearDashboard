using System;
using System.Linq.Expressions;
using ClearDashboard.Collaboration.DifferenceModel;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DataAccessLayer.Data;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Translation;

namespace ClearDashboard.Collaboration.Merge;

public class VerseRowHandler : DefaultMergeHandler
{
    public List<(string bookChapterVerse, string text, bool isSentenceStart)> VerseRowsForTokenization = new();
    public Dictionary<(string bookChapterVerse, Guid tokenizedCorpusId), Guid> VerseRowLookup = new();

    public VerseRowHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.VerseRow), nameof(Models.VerseRow.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.VerseRow>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.VerseRow>");
                }

                if (modelSnapshot.EntityPropertyValues.TryGetValue("BookChapterVerse", out var bookChapterVerse) &&
                    modelSnapshot.EntityPropertyValues.TryGetValue("TokenizedCorpusId", out var tokenizedCorpusId))
                {
                    var verseRowId = projectDbContext.VerseRows
                        .Where(e => (Guid)e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                        .Where(e => (string)e.BookChapterVerse! == (string)bookChapterVerse!)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    logger.LogInformation($"Converted VerseRow having TokenizedCorpusId ('{tokenizedCorpusId}') / BookChapterVerse ('{bookChapterVerse}') to VerseRowId ('{verseRowId}')");
                    return verseRowId;
                }
                else
                {
                    throw new PropertyResolutionException($"VerseRow snapshot does not have both TokenizedCorpusId+VerseRowId, which are required for VerseRowId resolution.");
                }
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.VerseRow), "BookChapterVerse"),
            new[] { nameof(Models.VerseRow.Id) });
    }

    protected void AddVerseRowForTokenization(Guid id, IModelSnapshot<Models.VerseRow> modelSnapshot, IModelDifference<IModelSnapshot<Models.VerseRow>>? modelDifference)
    {
        Guid tokenizedCorpusId = (Guid)modelSnapshot.PropertyValues[nameof(Models.VerseRow.TokenizedCorpusId)]!;
        string bookChapterVerse = (string)modelSnapshot.PropertyValues[nameof(Models.VerseRow.BookChapterVerse)]!;
        string? originalText = (string?)modelSnapshot.PropertyValues[nameof(Models.VerseRow.OriginalText)];
        bool isSentenceStart = (bool)modelSnapshot.PropertyValues[nameof(Models.VerseRow.IsSentenceStart)]!;

        if (modelDifference is not null)
        {
            // This method shouldn't get passed a modelDifference unless the original text changed
            foreach (var d in modelDifference.PropertyDifferences.Where(pd => pd.PropertyName == nameof(Models.VerseRow.OriginalText)))
            {
                var newValue = ((ValueDifference)d.PropertyValueDifference).Value2AsObject;
                if (d.PropertyName == nameof(Models.VerseRow.OriginalText))
                {
                    originalText = (string)newValue!;
                }
                else if (d.PropertyName == nameof(Models.VerseRow.IsSentenceStart))
                {
                    isSentenceStart = (bool)newValue!;
                }
                else if (d.PropertyName == nameof(Models.VerseRow.BookChapterVerse))
                {
                    bookChapterVerse = (string)newValue!;
                }
            }
        }

        if (string.IsNullOrEmpty(originalText))
        {
            throw new InvalidModelStateException($"VerseRow model snaphot having Id '{modelSnapshot.GetId()}' has a null or empty original text.  Is this valid?");
        }

        VerseRowsForTokenization.Add((bookChapterVerse, originalText!, isSentenceStart));
        VerseRowLookup.Add((bookChapterVerse, tokenizedCorpusId), id);
    }

    protected override async Task HandleDeleteAsync<T>(T itemToDelete, CancellationToken cancellationToken)
    {
        if (typeof(T).IsAssignableTo(typeof(IModelSnapshot<Models.VerseRow>)))
        {
            // We need to manually delete any TokenComposites that are associated
            // with the token(s) that are about to get deleted (might be that not
            // all of a TokenComposite's tokens get cascade deleted, but the
            // TokenComposite would no longer be valid):
            await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
                $"Deleting any TokenComposites related by Token to VerseRow '{itemToDelete.GetId()}'",
                TokenCompositeHandler.GetDeleteCompositesByVerseRowIdQueryAsync((Guid)itemToDelete.GetId()),
                cancellationToken);
        }

        // VerseRow has a Cascade Delete foreign key relationship in Sqlite with
        // TokenComponent, so related TokenComponents should get deleted
        // automatically, and they in turn have a Cascade Delete relationship
        // with Translations and Alignments, which should also get deleted.
        await base.HandleDeleteAsync(itemToDelete, cancellationToken);
    }

    protected override async Task<Guid> HandleCreateAsync<T>(T itemToCreate, CancellationToken cancellationToken)
    {
        var id = await base.HandleCreateAsync(itemToCreate, cancellationToken);

        if (typeof(T).IsAssignableTo(typeof(IModelSnapshot<Models.VerseRow>)))
        {
            AddVerseRowForTokenization((Guid)id!, (IModelSnapshot<Models.VerseRow>)itemToCreate, null);
        }

        return id;
    }

    public override async Task HandleModifyPropertiesAsync<T>(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
    {
        if (typeof(T).IsAssignableTo(typeof(IModelSnapshot<Models.VerseRow>)))
        {
            var modelSnapshotToModify = (IModelSnapshot<Models.VerseRow>)itemToModify;
            var modelSnapshotDifference = (IModelDifference<IModelSnapshot<Models.VerseRow>>)modelDifference;

            var modelMergeResult = CheckMerge<T>(modelDifference, itemToModify);

            if (modelMergeResult == ModelMergeResult.ShouldMerge)
            {
                var where = new Dictionary<string, object>() { { modelSnapshotToModify.IdentityKey, modelSnapshotToModify.EntityPropertyValues[modelSnapshotToModify.IdentityKey]! } };
                var resolvedWhereClause = await _mergeContext.MergeBehavior.ModifyModelAsync(modelDifference, modelSnapshotToModify, where, cancellationToken);

                if (modelDifference.PropertyDifferences.Where(pd => pd.PropertyName == nameof(Models.VerseRow.OriginalText)).Any())
                {
                    var verseRowId = (Guid)resolvedWhereClause[nameof(Models.VerseRow.Id)];

                    await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
                        $"Deleting any TokenComposites related by Token to VerseRow '{modelSnapshotToModify.GetId()}'",
                        TokenCompositeHandler.GetDeleteCompositesByVerseRowIdQueryAsync(verseRowId),
                        cancellationToken);

                    await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
                        $"Deleting any TokenComponents related by VerseRow '{modelSnapshotToModify.GetId()}'",
                        TokenCompositeHandler.GetDeleteTokenComponentsByVerseRowIdQueryAsync(verseRowId),
                        cancellationToken);

                    AddVerseRowForTokenization(verseRowId, modelSnapshotToModify, modelSnapshotDifference);
                }
            }
        }
        else
        {
            throw new NotImplementedException($"Derived merge handler with '{typeof(T).ShortDisplayName()}' model-specific HandleModifyProperties functionality");
        }

    }

    public Expression<Func<Models.VerseRow, bool>> BuildVerseRowLookupWhereExpression(IModelSnapshot<Models.VerseRow> snapshot)
    {
        var bookChapterVerse = (string)snapshot.EntityPropertyValues["BookChapterVerse"]!;
        var tokenizedCorpusId = (Guid)snapshot.EntityPropertyValues["TokenizedCorpusId"]!;
        return vr => vr.BookChapterVerse == bookChapterVerse && vr.TokenizedCorpusId == tokenizedCorpusId;
    }
}

