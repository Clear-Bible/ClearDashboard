using System;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Linq;
using ClearBible.Engine.Tokenization;
using SIL.Machine.Tokenization;
using ClearBible.Engine.Corpora;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DAL.Alignment.Features.Common;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using ClearBible.Engine.Persistence;

namespace ClearDashboard.Collaboration.Merge;

public class TokenizedCorpusHandler : DefaultMergeHandler
{
    public TokenizedCorpusHandler(MergeContext mergeContext) : base(mergeContext)
    {
    }

    protected override async Task HandleDeleteAsync<T>(T itemToDelete, CancellationToken cancellationToken)
    {
        await base.HandleDeleteAsync(itemToDelete, cancellationToken);
    }

    protected override async Task<Guid> HandleCreateAsync<T>(T itemToCreate, CancellationToken cancellationToken)
    {
        return await base.HandleCreateAsync(itemToCreate, cancellationToken);
    }

    public override async Task HandleModifyPropertiesAsync<T>(IModelDifference<T> modelDifference, T itemToModify, CancellationToken cancellationToken = default)
    {
        await base.HandleModifyPropertiesAsync(modelDifference, itemToModify, cancellationToken);

        // FIXME:  if the last tokenized date is greater than in this db,
        // and the tokenizer function is different, tokenize?  
    }

    protected override async Task HandleChildListDifferenceAsync<T>(
        string childName,
        IListDifference listDifference,
        T? parentItemInCurrentSnapshot,
        T? parentItemInTargetCommitSnapshot,
        CancellationToken cancellationToken) where T : default
    {
        if (childName != "VerseRows")
        {
            await ChildListDifferenceAsync(childName, listDifference, parentItemInCurrentSnapshot, parentItemInTargetCommitSnapshot, cancellationToken);
            return;
        }

        if (parentItemInCurrentSnapshot is not null && ((IModelSnapshot)parentItemInCurrentSnapshot).Children.ContainsKey(childName))
        {
            var tokenizedCorpusSnapshot = (IModelSnapshot<Models.TokenizedCorpus>)parentItemInCurrentSnapshot;
            var tokenizedCorpusId = (Guid)tokenizedCorpusSnapshot.PropertyValues[nameof(Models.TokenizedCorpus.Id)]!;

            var verseRowHandler = (VerseRowHandler)_mergeContext.FindMergeHandler<IModelSnapshot<Models.VerseRow>>();

            // Clear out verseRowHandler collections prior to processing:
            verseRowHandler.VerseRowsForTokenization.Clear();
            verseRowHandler.VerseRowLookup.Clear();

            // This should use the VerseRowHandler to delete VerseRows and
            // their downstream entities:  Tokens, Alignments, Translations,
            // and TokenComposites (if doing deletes).    
            await ChildListDifferenceAsync(
                childName,
                listDifference,
                parentItemInCurrentSnapshot,
                parentItemInTargetCommitSnapshot,
                cancellationToken);

            if (!verseRowHandler.VerseRowsForTokenization.Any())
            {
                return;
            }

            var textCorpus = ExtractITextCorpus(tokenizedCorpusSnapshot, verseRowHandler);
            var bookIds = textCorpus.Texts.Select(t => t.Id).ToList();

            await _mergeContext.MergeBehavior.RunProjectDbContextQueryAsync(
                $"Inserting VerseRow TokenComponents for TokenizedCorpusId '{tokenizedCorpusId}'",
                async (ProjectDbContext projectDbContext, ILogger logger, CancellationToken cancellationToken) => {

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

                            if (verseRowHandler.VerseRowLookup.TryGetValue((verseRow.BookChapterVerse!, verseRow.TokenizedCorpusId), out var verseRowId))
                            {
                                var tokenComponents = verseRow.TokenComponents.Select(t => { t.VerseRowId = verseRowId; return t; }).ToList();

                                await TokenizedCorpusDataUtil.InsertTokenComponentsAsync(
                                    tokenComponents,
                                    tokenComponentInsertCommand,
                                    tokenCompositeTokenAssociationInsertCommand,
                                    projectDbContext.UserProvider!,
                                    cancellationToken);

                                logger.LogInformation($"Inserted {tokenComponents.Count} TokenComponents for VerseRow '{verseRowId}'");

                            }
                            else
                            {
                                throw new InvalidModelStateException($"VerseRowId lookup failed for bookChapterVerse '{verseRow.BookChapterVerse}' and tokenizedCorpusId '{verseRow.TokenizedCorpusId}'");
                            }
                        }
                    }
                });
        }
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
}

