using System;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Macula.PropertiesSources.Tokenization;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Linq;
using SIL.Machine.Tokenization;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DAL.Alignment.Features.Common;
using Microsoft.EntityFrameworkCore;
using ClearBible.Engine.Persistence;
using ClearDashboard.Collaboration.Factory;
using MediatR;
using System.Threading;
using SIL.Machine.Utils;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class TokenizedCorpusHandler : DefaultMergeHandler
{
    public TokenizedCorpusHandler(MergeContext mergeContext) : base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenizedCorpus), nameof(Models.TokenizedCorpus.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.TokenizedCorpus>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.TokenizedCorpus>");
                }

                var tokenizedCorpusId = (Guid)modelSnapshot.GetId();

                if (TokenizedCorpusBuilder.TokenizedCorpusManuscriptIds.ContainsValue(tokenizedCorpusId) &&
                    !projectDbContext.TokenizedCorpora.Any(e => e.Id == tokenizedCorpusId))
                {
                    var corpusType = TokenizedCorpusBuilder.TokenizedCorpusManuscriptIds
                        .First(x => x.Value == tokenizedCorpusId)
                        .Key;

                    var id = projectDbContext.TokenizedCorpora
                        .Include(e => e.Corpus)
                        .Where(e => e.Corpus!.CorpusType == corpusType)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    if (id != default)
                    {
                        tokenizedCorpusId = id;
                    }
                }

                return tokenizedCorpusId;
            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.TokenizedCorpus), nameof(Models.TokenizedCorpus.CorpusId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.TokenizedCorpus>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.TokenizedCorpus>");
                }

                if (modelSnapshot.TryGetPropertyValue(nameof(Models.TokenizedCorpus.CorpusId), out var value))
                {
                    var corpusId = (Guid)value!;

                    if (CorpusBuilder.CorpusManuscriptIds.ContainsValue(corpusId) &&
                        !projectDbContext.Corpa.Any(e => e.Id == corpusId))
                    {
                        var corpusType = CorpusBuilder.CorpusManuscriptIds
                            .First(x => x.Value == corpusId)
                            .Key;

                        var id = projectDbContext.Corpa
                            .Where(e => e.CorpusType == corpusType)
                            .Select(e => e.Id)
                            .FirstOrDefault();

                        if (id != default)
                        {
                            corpusId = id;
                        }
                        else
                        {
                            throw new PropertyResolutionException($"Incoming TokenizedCorpus '{modelSnapshot.GetId()}' has a manuscript corpus type '{corpusType}' but no matching Corpus can be found by CorpusType or Id");
                        }
                    }

                    return corpusId;
                }
                else
                {
                    throw new PropertyResolutionException($"Incoming TokenizedCorpus '{modelSnapshot.GetId()}' does not have a CorpusId");
                }
            });
    }

    public override async Task<bool> HandleModifyPropertiesAsync<T>(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
    {
        var modified = await base.HandleModifyPropertiesAsync(modelDifference, itemToModify, cancellationToken);

        if (modified)
        {
            var tokenizationDifference = modelDifference.PropertyDifferences
                .Where(pd => pd.PropertyName == nameof(Models.TokenizedCorpus.TokenizationFunction))
                .Select(pd => (ValueDifference<string>)pd.PropertyValueDifference)
                .FirstOrDefault();

            if (tokenizationDifference is not null)
            {
                // FIXME:  delete all existing tokens for this tokenized corpus and retokenize?

            }
        }

        return modified;
    }

    protected override async Task CreateChildrenAsync<T>(T parentSnapshot, CancellationToken cancellationToken)
    {
        // Sequence is important here, which is why the base class method is
        // overridden:

        var verseRowChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.VerseRow)].childName;
        if (parentSnapshot.Children.ContainsKey(verseRowChildName))
        {
            var verseRowHandler = (VerseRowHandler)_mergeContext.FindMergeHandler<IModelSnapshot<Models.VerseRow>>();

            // Clear out verseRowHandler collections prior to processing:
            verseRowHandler.VerseRowsForTokenization.Clear();
            verseRowHandler.VerseRowLookup.Clear();

            _mergeContext.Logger.LogInformation($"Inserting verse row children for tokenized corpus  '{parentSnapshot.GetId()}'");
            var insertCount = await CreateListItemsAsync<IModelSnapshot<Models.VerseRow>>(
                (IEnumerable<IModelSnapshot<Models.VerseRow>>)parentSnapshot.Children[verseRowChildName],
                cancellationToken);
            _mergeContext.Logger.LogInformation($"Completed inserting {insertCount} verse row children for tokenized corpus '{parentSnapshot.GetId()}'");

            _mergeContext.Logger.LogInformation($"Inserting tokens for imported verse row children for tokenized corpus '{parentSnapshot.GetId()}'");
            await InsertTokens((IModelSnapshot<Models.TokenizedCorpus>)parentSnapshot, verseRowHandler, cancellationToken);
            _mergeContext.Logger.LogInformation($"Completed inserting tokens for imported verse row children for tokenized corpus '{parentSnapshot.GetId()}'");
        }
        else
        {
            if (parentSnapshot is not null)
            {
                _mergeContext.Logger.LogInformation($"Inserting tokens for manuscript tokenized corpus '{parentSnapshot.GetId()}'");
                await ImportManuscriptVerseRowsTokens((IModelSnapshot<Models.TokenizedCorpus>)parentSnapshot!, cancellationToken);
                _mergeContext.Logger.LogInformation($"Completed inserting tokens for manuscript tokenized corpus '{parentSnapshot.GetId()}'");
            }
        }

        var compositeChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.TokenComposite)].childName;
        if (parentSnapshot!.Children.ContainsKey(compositeChildName))
        {
            _mergeContext.Logger.LogInformation($"Inserting composite token children for tokenized corpus '{parentSnapshot.GetId()}'");
            var insertCount = await CreateListItemsAsync<IModelSnapshot<Models.TokenComposite>>(
                (IEnumerable<IModelSnapshot<Models.TokenComposite>>)parentSnapshot.Children[compositeChildName],
                cancellationToken);
            _mergeContext.Logger.LogInformation($"Completed inserting {insertCount} composite token children for tokenized corpus '{parentSnapshot.GetId()}'");
        }
    }

    protected override async Task ChildListDifferenceAsync<T>(
        IReadOnlyDictionary<string, IListDifference> childListDifferences,
        T? parentItemInCurrentSnapshot,
        T? parentItemInTargetCommitSnapshot,
        CancellationToken cancellationToken) where T : default
    {
        var verseRowChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.VerseRow)].childName;
        if (childListDifferences.ContainsKey(verseRowChildName))
        {
            var verseRowHandler = (VerseRowHandler)_mergeContext.FindMergeHandler<IModelSnapshot<Models.VerseRow>>();

            // Clear out verseRowHandler collections prior to processing:
            verseRowHandler.VerseRowsForTokenization.Clear();
            verseRowHandler.VerseRowLookup.Clear();

            _mergeContext.Logger.LogInformation($"Starting handle verse row child list differences for tokenized corpus");
            await HandleListDifferenceGroup<IModelSnapshot<Models.VerseRow>>(
                childListDifferences[verseRowChildName],
                parentItemInCurrentSnapshot?.Children[verseRowChildName],
                parentItemInTargetCommitSnapshot?.Children[verseRowChildName],
                cancellationToken);
            _mergeContext.Logger.LogInformation($"Completed handle verse row child list differences for tokenized corpus");

            if (parentItemInCurrentSnapshot is not null)
            {
                _mergeContext.Logger.LogInformation($"Inserting tokens for list difference verse row children for tokenized corpus '{parentItemInCurrentSnapshot.GetId()}'");
                await InsertTokens((IModelSnapshot<Models.TokenizedCorpus>)parentItemInCurrentSnapshot, verseRowHandler, cancellationToken);
                _mergeContext.Logger.LogInformation($"Compelted inserting tokens for list difference verse row children for tokenized corpus '{parentItemInCurrentSnapshot.GetId()}'");
            }
        }

        var compositeChildName = ProjectSnapshotFactoryCommon.childFolderNameMappings[typeof(Models.TokenComposite)].childName;
        if (childListDifferences.ContainsKey(compositeChildName))
        {
            _mergeContext.Logger.LogInformation($"Starting handle composite token child list differences for tokenized corpus");
            await HandleListDifferenceGroup<IModelSnapshot<Models.TokenComposite>>(
                childListDifferences[compositeChildName],
                parentItemInCurrentSnapshot?.Children[compositeChildName],
                parentItemInTargetCommitSnapshot?.Children[compositeChildName],
                cancellationToken);
            _mergeContext.Logger.LogInformation($"Completed handle composite token child list differences for tokenized corpus");
        }
    }

    private async Task InsertTokens(IModelSnapshot<Models.TokenizedCorpus> tokenizedCorpusSnapshot, VerseRowHandler verseRowHandler, CancellationToken cancellationToken)
    {
        if (!verseRowHandler.VerseRowsForTokenization.Any())
        {
            return;
        }

        var tokenizedCorpusId = (Guid)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.Id)]!;
        var tokenizedCorpusName = GetModelSnapshotDisplayName(tokenizedCorpusSnapshot);
        int scrVersType = (int)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.ScrVersType)]!;
        string? customVersData = (string?)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.CustomVersData)];
        string? tokenizationFunction = (string?)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.TokenizationFunction)];

        var versification = ExtractVersification(scrVersType, customVersData);
        var textCorpus = ExtractITextCorpus(tokenizedCorpusId, tokenizationFunction, versification, verseRowHandler.VerseRowsForTokenization);
        var bookIds = textCorpus.Texts.Select(t => t.Id).ToList();

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Inserting VerseRow TokenComponents for TokenizedCorpusId '{tokenizedCorpusId}'",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                progress.Report(new ProgressStatus(0, $"Inserting VerseRow tokens for tokenized corpus '{tokenizedCorpusName}' '{tokenizedCorpusId}'"));

                var tokenInsertCount = 0;
                var connection = projectDbContext.Database.GetDbConnection();

                using var tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection);
                using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection);

                foreach (var bookId in bookIds)
                {
                    var tokensTextRows = TokenizedCorpusDataBuilder.ExtractValidateBook(
                        textCorpus,
                        bookId,
                        GetModelSnapshotDisplayName(tokenizedCorpusSnapshot));

                    // This method currently doesn't have any way to use the real
                    // VerseRowIds when building VerseRows.  So we correct each
                    // token's VerseRowId before doing its insert
                    var (verseRows, btTokenCount) = TokenizedCorpusDataBuilder.BuildVerseRowModel(tokensTextRows, tokenizedCorpusId);

                    foreach (var verseRow in verseRows)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (verseRowHandler.VerseRowLookup.TryGetValue((verseRow.BookChapterVerse!, verseRow.TokenizedCorpusId), out (Guid verseRowId, Guid userId) verseRowUser))
                        {
                            var tokenComponents = verseRow.TokenComponents.Select(t => {
                                t.VerseRowId = verseRowUser.verseRowId;
                                return t;
                            }).ToList();

                            await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(
                                tokenComponents,
                                tokenComponentInsertCommand,
                                tokenCompositeTokenAssociationInsertCommand,
                                cancellationToken);

                            tokenInsertCount += tokenComponents.Count;

                            //logger.LogDebug($"Inserted {tokenComponents.Count} TokenComponents for VerseRow '{verseRowUser.verseRowId}'");

                        }
                        else
                        {
                            throw new InvalidModelStateException($"VerseRowId lookup failed for bookChapterVerse '{verseRow.BookChapterVerse}' and tokenizedCorpus '{verseRow.TokenizedCorpusId}'");
                        }
                    }
                }

                progress.Report(new ProgressStatus(0, $"Inserted {tokenInsertCount} tokens"));
                logger.LogInformation($"Inserted {tokenInsertCount} tokens");
            },
            cancellationToken);

    }

    public static ScrVers ExtractVersification(int scrVersType, string? customVersData)
    {
        ScrVers versification = new ScrVers((ScrVersType)scrVersType);
        if (!string.IsNullOrEmpty(customVersData))
        {
            using (var reader = new StringReader(customVersData))
            {
                Versification.Table.Implementation.RemoveAllUnknownVersifications();
                versification = Versification.Table.Implementation.Load(reader, "not a file", versification, "custom");
            }
        }

        return versification;
    }

    public static ITextCorpus ExtractITextCorpus(
        Guid tokenizedCorpusId,
        string? tokenizationFunction,
        ScrVers versification,
        IEnumerable<(string bookChapterVerse, string text, bool isSentenceStart)> verseRowsForTokenization)
    {
        if (string.IsNullOrEmpty(tokenizationFunction))
        {
            throw new InvalidModelStateException($"TokenizedCorpus model snaphot having Id '{tokenizedCorpusId}' has a null or empty TokenizationFunction.  Is this valid?");
        }

        var bookNumbersToAbbreviations =
            FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum),
                x => x.silCannonBookAbbrev);

        IEnumerable<VerseRowText> verseRowTexts = verseRowsForTokenization
            .OrderBy(v => v.bookChapterVerse)
            .GroupBy(v => v.bookChapterVerse.Substring(0, 3))
            .Select(g =>
            {
                if (bookNumbersToAbbreviations.TryGetValue(int.Parse(g.Key), out var bookId))
                {
                    return new VerseRowText(bookId, versification, g.Select(v =>
                        (
                            chapter: v.bookChapterVerse.Substring(3, 3),
                            verse: v.bookChapterVerse.Substring(6, 3),
                            text: v.text,
                            isSentenceStart: v.isSentenceStart
                        )
                    ));
                }
                else
                {
                    throw new InvalidModelStateException($"Invalid book number '{g.Key}' contained in VerseRow BCV for TokenizedCorpusId '{tokenizedCorpusId}' ");
                }
            })
            .ToList();

        var verseRowTextCorpus = new VerseRowTextCorpus(versification, verseRowTexts);
        var tokenizer = tokenizationFunction!.ToTokenizer();

        var textCorpus = verseRowTextCorpus
            .Tokenize(tokenizer)
            .Transform<IntoTokensTextRowProcessor>()
            .Transform<SetTrainingBySurfaceLowercase>();

        return textCorpus;
    }

    private async Task ImportManuscriptVerseRowsTokens(IModelSnapshot<Models.TokenizedCorpus> tokenizedCorpusSnapshot, CancellationToken cancellationToken)
    {
        var tokenizedCorpusId = (Guid)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.Id)]!;
        var tokenizedCorpusName = GetModelSnapshotDisplayName(tokenizedCorpusSnapshot);
        var userId = (Guid)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.UserId)]!;

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Inserting VerseRows and TokenComponents for manuscript TokenizedCorpusId '{tokenizedCorpusId}'",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, IProgress<ProgressStatus> progress, CancellationToken cancellationToken) => {

                progress.Report(new ProgressStatus(0, $"Inserting VerseRows and tokens for manuscript tokenized corpus '{tokenizedCorpusName}' '{tokenizedCorpusId}'"));

                var tokenInsertCount = 0;
                var connection = projectDbContext.Database.GetDbConnection();

                var corpusType = projectDbContext.TokenizedCorpora.Include(e => e.Corpus)
                    .Where(e => e.Id == tokenizedCorpusId)
                    .Select(e => e.Corpus!.CorpusType)
                    .First();

                if (corpusType != Models.CorpusType.ManuscriptHebrew &&
                    corpusType != Models.CorpusType.ManuscriptGreek)
                {
                    return;
                }

                var textCorpus = GetMaculaCorpus(corpusType); 

                using var verseRowInsertCommand = TokenizedCorpusDataBuilder.CreateVerseRowInsertCommand(connection);
                using var tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection);
                using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection);

                var bookIds = textCorpus.Texts.Select(t => t.Id).ToList();

                foreach (var bookId in bookIds)
                {
                    var tokensTextRows = TokenizedCorpusDataBuilder.ExtractValidateBook(
                        textCorpus,
                        bookId,
                        GetModelSnapshotDisplayName(tokenizedCorpusSnapshot));

                    // This method currently doesn't have any way to use the real
                    // VerseRowIds when building VerseRows.  So we correct each
                    // token's VerseRowId before doing its insert
                    var (verseRows, btTokenCount) = TokenizedCorpusDataBuilder.BuildVerseRowModel(tokensTextRows, tokenizedCorpusId);

                    foreach (var verseRow in verseRows)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        verseRow.UserId = userId;
                        var tokenComponents = verseRow.TokenComponents.ToList();

                        await TokenizedCorpusDataBuilder.InsertVerseRowAsync(
                            verseRow, verseRowInsertCommand,
                            projectDbContext.UserProvider!, cancellationToken);
                        await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(
                            tokenComponents, tokenComponentInsertCommand,
                            tokenCompositeTokenAssociationInsertCommand,
                            cancellationToken);

                        tokenInsertCount += tokenComponents.Count;
                    }
                }

                progress.Report(new ProgressStatus(0, $"Inserted {tokenInsertCount} tokens"));
                logger.LogInformation($"Inserted {tokenInsertCount} tokens");
            },
            cancellationToken);
    }

    private ITextCorpus GetMaculaCorpus(Models.CorpusType corpusType)
    {
        var syntaxTree = new SyntaxTrees();

        if (corpusType == Models.CorpusType.ManuscriptHebrew)
        {
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.H)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();
            return sourceCorpus;
        }
        else if (corpusType == Models.CorpusType.ManuscriptGreek)
        {
            var sourceCorpus = new SyntaxTreeFileTextCorpus(syntaxTree, ClearBible.Engine.Persistence.FileGetBookIds.LanguageCodeEnum.G)
                .Transform<SetTrainingByTrainingLowercase>()
                .Transform<AddPronominalReferencesToTokens>();
            return sourceCorpus;
        }
        else
        {
            throw new Exception($"Unknown corpus type '{corpusType}'");
        }
    }
}

