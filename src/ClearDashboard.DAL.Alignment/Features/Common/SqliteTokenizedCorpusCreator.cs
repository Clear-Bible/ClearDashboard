using Microsoft.EntityFrameworkCore;
using SIL.Machine.Corpora;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClearDashboard.DAL.Alignment.Features.Common;

public class SqliteTokenizedCorpusCreator : ITokenizedCorpusDataCreator
{
    private readonly IUserProvider _userProvider;
    public SqliteTokenizedCorpusCreator(IUserProvider userProvider)
    {
        _userProvider = userProvider;
    }

    public async Task<(int VerseRowCount, int TokenComponentCount)> BulkWriteTokenizedCorpusToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel, 
        Models.TokenizedCorpus tokenizedCorpus, 
        ITextCorpus textCorpus, 
        CancellationToken cancellationToken)
    {
        using var tokenizedCorpusInsertCommand = TokenizedCorpusDataBuilder.CreateTokenizedCorpusInsertCommand(connection, metadataModel);

        await TokenizedCorpusDataBuilder.InsertTokenizedCorpusAsync(tokenizedCorpus, tokenizedCorpusInsertCommand, _userProvider, cancellationToken);
        var tokenizedCorpusId = (Guid)tokenizedCorpusInsertCommand.Parameters[tokenizedCorpusInsertCommand.ToParameterName(nameof(Models.TokenizedCorpus.Id))].Value!;

        (Guid VerseRowId, Guid UserId) GetVerseRowIdFunc((string BookChapterVerse, Guid TokenizedCorpusId) verseRowContext)
        {
            return (Guid.NewGuid(), _userProvider.CurrentUser!.Id);
        }

        var verseRowsByBook = TokenizedCorpusDataBuilder.BuildCorpusVerseRowModels(
            textCorpus, 
            tokenizedCorpusId, 
            GetVerseRowIdFunc, 
            tokenizedCorpus.DisplayName ?? string.Empty);

        var verseRowCount = await BulkWriteVerseRowsToDatabaseAsync(
            connection, 
            metadataModel, 
            verseRowsByBook, 
            cancellationToken);

        var tokenComponentCount = await BulkWriteTokensToDatabaseAsync(
            connection, 
            metadataModel, 
            textCorpus, 
            verseRowsByBook, 
            tokenizedCorpus.DisplayName ?? string.Empty, 
            cancellationToken);

        return (verseRowCount, tokenComponentCount);
    }

    public async Task<int> BulkWriteVerseRowsToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel,
        IDictionary<string, IEnumerable<Models.VerseRow>> verseRowsByBook,
        CancellationToken cancellationToken
    )
    {
        var verseRowCount = 0;

        using var verseRowInsertCommand = TokenizedCorpusDataBuilder.CreateVerseRowInsertCommand(connection, metadataModel);

        foreach (var kvp in verseRowsByBook)
        {
            foreach (var verseRow in kvp.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await TokenizedCorpusDataBuilder.InsertVerseRowAsync(verseRow, verseRowInsertCommand, _userProvider, cancellationToken);
                verseRowCount++;
            }
        }

        return verseRowCount;
    }

    public async Task<int> BulkWriteTokensToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel,
        ITextCorpus textCorpus,
        IDictionary<string, IEnumerable<Models.VerseRow>> verseRowsByBook,
        string corpusDisplayName,
        CancellationToken cancellationToken)
    {
        var tokenCount = 0;

        using var tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection, metadataModel);
        using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection, metadataModel);

        foreach (var kvp in verseRowsByBook)
        {
            foreach (var verseRow in kvp.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(
                    verseRow.TokenComponents,
                    tokenComponentInsertCommand,
                    tokenCompositeTokenAssociationInsertCommand,
                    cancellationToken);

                var compositeChildren = verseRow.TokenComponents
                    .Where(t => t is Models.TokenComposite)
                    .Select(t => ((Models.TokenComposite)t).Tokens.Count)
                    .Sum();
                tokenCount += verseRow.TokenComponents.Count + compositeChildren;
            }
        }

        return tokenCount;
    }
}
