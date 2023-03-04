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

namespace ClearDashboard.Collaboration.Merge;

public class TokenizedCorpusHandler : DefaultMergeHandler
{
    public TokenizedCorpusHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    public override async Task HandleModifyPropertiesAsync<T>(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
    {
        await base.HandleModifyPropertiesAsync(modelDifference, itemToModify, cancellationToken);

        // FIXME:  if the last tokenized date is greater than in this db,
        // and the tokenizer function is different, tokenize?  
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

            _mergeContext.Logger.LogInformation($"Inserting verse row children for tokenized corpus '{parentSnapshot.GetId()}'");
            var insertCount = await CreateListItemsAsync<IModelSnapshot<Models.VerseRow>>(
                (IEnumerable<IModelSnapshot<Models.VerseRow>>)parentSnapshot.Children[verseRowChildName],
                cancellationToken);
            _mergeContext.Logger.LogInformation($"Completed inserting {insertCount} verse row children for tokenized corpus '{parentSnapshot.GetId()}'");

            _mergeContext.Logger.LogInformation($"Inserting tokens for imported verse row children for tokenized corpus '{parentSnapshot.GetId()}'");
            await InsertTokens((IModelSnapshot<Models.TokenizedCorpus>)parentSnapshot, verseRowHandler);
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
                await InsertTokens((IModelSnapshot<Models.TokenizedCorpus>)parentItemInCurrentSnapshot, verseRowHandler);
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

    private async Task InsertTokens(IModelSnapshot<Models.TokenizedCorpus> tokenizedCorpusSnapshot, VerseRowHandler verseRowHandler)
    {
        if (!verseRowHandler.VerseRowsForTokenization.Any())
        {
            return;
        }

        var tokenizedCorpusId = (Guid)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.Id)]!;
        var textCorpus = ExtractITextCorpus(tokenizedCorpusSnapshot, verseRowHandler);
        var bookIds = textCorpus.Texts.Select(t => t.Id).ToList();

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Inserting VerseRow TokenComponents for TokenizedCorpusId '{tokenizedCorpusId}'",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, CancellationToken cancellationToken) => {

                var tokenInsertCount = 0;
                var connection = projectDbContext.Database.GetDbConnection();

                using var tokenComponentInsertCommand = TokenizedCorpusDataUtil.CreateTokenComponentInsertCommand(connection);
                using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataUtil.CreateTokenCompositeTokenAssociationInsertCommand(connection);

                foreach (var bookId in bookIds)
                {
                    var tokensTextRows = TokenizedCorpusDataUtil.ExtractValidateBook(
                        textCorpus,
                        bookId,
                        (string)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.DisplayName)]!);

                    // This method currently doesn't have any way to use the real
                    // VerseRowIds when building VerseRows.  So we correct each
                    // token's VerseRowId before doing its insert
                    var (verseRows, btTokenCount) = TokenizedCorpusDataUtil.BuildVerseRowModel(tokensTextRows, tokenizedCorpusId);

                    foreach (var verseRow in verseRows)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (verseRowHandler.VerseRowLookup.TryGetValue((verseRow.BookChapterVerse!, verseRow.TokenizedCorpusId), out (Guid verseRowId, Guid userId) verseRowUser))
                        {
                            var tokenComponents = verseRow.TokenComponents.Select(t => {
                                t.VerseRowId = verseRowUser.verseRowId;
                                t.UserId = verseRowUser.userId;
                                return t;
                            }).ToList();

                            await TokenizedCorpusDataUtil.InsertTokenComponentsAsync(
                                tokenComponents,
                                tokenComponentInsertCommand,
                                tokenCompositeTokenAssociationInsertCommand,
                                projectDbContext.UserProvider!,
                                cancellationToken);

                            tokenInsertCount += tokenComponents.Count;

                            logger.LogDebug($"Inserted {tokenComponents.Count} TokenComponents for VerseRow '{verseRowUser.verseRowId}'");

                        }
                        else
                        {
                            throw new InvalidModelStateException($"VerseRowId lookup failed for bookChapterVerse '{verseRow.BookChapterVerse}' and tokenizedCorpusId '{verseRow.TokenizedCorpusId}'");
                        }
                    }
                }

                logger.LogInformation($"Inserted {tokenInsertCount} tokens");
            });

    }

    private ITextCorpus ExtractITextCorpus(IModelSnapshot<Models.TokenizedCorpus> tokenizedCorpusSnapshot, VerseRowHandler verseRowHandler)
    {
        int srcVersType = (int)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.ScrVersType)]!;
        string? customVersData = (string?)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.CustomVersData)];
        string? tokenizationFunction = (string?)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.TokenizationFunction)];

        ScrVers versification = new ScrVers((ScrVersType)srcVersType);
        if (!string.IsNullOrEmpty(customVersData))
        {
            using (var reader = new StringReader(customVersData))
            {
                versification = Versification.Table.Implementation.Load(reader, "not a file");
            }
        }

        if (string.IsNullOrEmpty(tokenizationFunction))
        {
            throw new InvalidModelStateException($"TokenizedCorpus model snaphot having Id '{tokenizedCorpusSnapshot.GetId()}' has a null or empty TokenizationFunction.  Is this valid?");
        }

        var bookNumbersToAbbreviations =
            FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum),
                x => x.silCannonBookAbbrev);

        IEnumerable<VerseRowText> verseRowTexts = verseRowHandler.VerseRowsForTokenization
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
                    throw new InvalidModelStateException($"Invalid book number '{g.Key}' contained in VerseRow BCV for TokenizedCorpusId '{tokenizedCorpusSnapshot.GetId()}' ");
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
        var userId = (Guid)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.UserId)]!;

        await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
            $"Inserting VerseRows and TokenComponents for TokenizedCorpusId '{tokenizedCorpusId}'",
            async (ProjectDbContext projectDbContext, MergeCache cache, ILogger logger, CancellationToken cancellationToken) => {

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

                using var verseRowInsertCommand = TokenizedCorpusDataUtil.CreateVerseRowInsertCommand(connection);
                using var tokenComponentInsertCommand = TokenizedCorpusDataUtil.CreateTokenComponentInsertCommand(connection);
                using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataUtil.CreateTokenCompositeTokenAssociationInsertCommand(connection);

                var bookIds = textCorpus.Texts.Select(t => t.Id).ToList();

                foreach (var bookId in bookIds)
                {
                    var tokensTextRows = TokenizedCorpusDataUtil.ExtractValidateBook(
                        textCorpus,
                        bookId,
                        (string)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.DisplayName)]!);

                    // This method currently doesn't have any way to use the real
                    // VerseRowIds when building VerseRows.  So we correct each
                    // token's VerseRowId before doing its insert
                    var (verseRows, btTokenCount) = TokenizedCorpusDataUtil.BuildVerseRowModel(tokensTextRows, tokenizedCorpusId);

                    foreach (var verseRow in verseRows)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        verseRow.UserId = userId;
                        var tokenComponents = verseRow.TokenComponents.Select(t => {
                            t.UserId = userId;
                            return t;
                        }).ToList();

                        await TokenizedCorpusDataUtil.InsertVerseRowAsync(
                            verseRow, verseRowInsertCommand,
                            projectDbContext.UserProvider!, cancellationToken);
                        await TokenizedCorpusDataUtil.InsertTokenComponentsAsync(
                            tokenComponents, tokenComponentInsertCommand,
                            tokenCompositeTokenAssociationInsertCommand,
                            projectDbContext.UserProvider!, cancellationToken);

                        tokenInsertCount += tokenComponents.Count;
                    }
                }

                logger.LogInformation($"Inserted {tokenInsertCount} tokens");
            });


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

