using Microsoft.EntityFrameworkCore;
using SIL.Machine.Corpora;
using ClearDashboard.DAL.Interfaces;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;
using ClearDashboard.DataAccessLayer.Models;
using NpgsqlTypes;
using Npgsql;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Common;

public class NpgsqlTokenizedCorpusCreator : ITokenizedCorpusDataCreator
{
    private readonly IUserProvider _userProvider;
    private readonly ILogger _logger;
    public NpgsqlTokenizedCorpusCreator(IUserProvider userProvider, ILogger<NpgsqlTokenizedCorpusCreator> logger)
    {
        _userProvider = userProvider;
        _logger = logger;
    }

    public async Task<(int VerseRowCount, int TokenComponentCount)> BulkWriteTokenizedCorpusToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel, 
        TokenizedCorpus tokenizedCorpus, 
        ITextCorpus textCorpus, 
        CancellationToken cancellationToken)
    {
        if (tokenizedCorpus.Id == default)
            tokenizedCorpus.Id = Guid.NewGuid();

        if (tokenizedCorpus.UserId == default)
            tokenizedCorpus.UserId = _userProvider.CurrentUser!.Id;

        if (tokenizedCorpus.Created == default)
            tokenizedCorpus.Created = DateTimeOffset.Now;

        (Guid VerseRowId, Guid UserId) GetVerseRowIdFunc((string BookChapterVerse, Guid TokenizedCorpusId) verseRowContext)
        {
            return (Guid.NewGuid(), _userProvider.CurrentUser!.Id);
        }

        var tokenizedCorpusEntityType = metadataModel.ToEntityType(typeof(Models.TokenizedCorpus));
        var tokenziedCorpusPropertyInfos = tokenizedCorpusEntityType.GetProperties().ToDictionary(p => p.Name, p => p);
        using (var tokenizedCorpusWriter = await BeginTokenizedCorpusBinaryImporterAsync((NpgsqlConnection)connection, tokenizedCorpusEntityType, tokenziedCorpusPropertyInfos))
        {
            await InsertTokenizedCorpusAsync(tokenizedCorpus, tokenziedCorpusPropertyInfos, tokenizedCorpusWriter);
            await tokenizedCorpusWriter.CompleteAsync(cancellationToken);
        }

        var verseRowsByBook = TokenizedCorpusDataBuilder.BuildCorpusVerseRowModels(
            textCorpus, 
            tokenizedCorpus.Id, 
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

    public async Task<int> BulkWriteVerseRowsToDatabaseAsync(DbConnection connection, IModel metadataModel, IDictionary<string, IEnumerable<VerseRow>> verseRowsByBook, CancellationToken cancellationToken)
    {
        if (connection is not NpgsqlConnection)
        {
            throw new ArgumentException($"Must use {nameof(NpgsqlConnection)} when using {nameof(NpgsqlTokenizedCorpusCreator)}");
        }

        var npgqlConnection = (NpgsqlConnection)connection; 
        var verseRowCount = 0;

        var verseRowEntityType = metadataModel.ToEntityType(typeof(Models.VerseRow));
        var verseRowPropertyInfos = verseRowEntityType.GetProperties().ToDictionary(p => p.Name, p => p);

        using var verseRowWriter = await BeginVerseRowBinaryImporterAsync(npgqlConnection, verseRowEntityType, verseRowPropertyInfos);

        foreach (var kvp in verseRowsByBook)
        {
            foreach (var verseRow in kvp.Value)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await InsertVerseRowAsync(verseRow, verseRowPropertyInfos, verseRowWriter);
                verseRowCount++;
            }
        }

        await verseRowWriter.CompleteAsync(cancellationToken);

        return await Task.FromResult(verseRowCount);
    }

    private static async Task<NpgsqlBinaryImporter> BeginTokenizedCorpusBinaryImporterAsync(NpgsqlConnection npgqlConnection, IEntityType entityType, IDictionary<string, IProperty> propertyInfos)
    {
        var c01 = propertyInfos[nameof(Models.TokenizedCorpus.Id)].GetColumnName();
        var c02 = propertyInfos[nameof(Models.TokenizedCorpus.UserId)].GetColumnName();
        var c03 = propertyInfos[nameof(Models.TokenizedCorpus.CorpusId)].GetColumnName();
        var c04 = propertyInfos[nameof(Models.TokenizedCorpus.DisplayName)].GetColumnName();
        var c05 = propertyInfos[nameof(Models.TokenizedCorpus.TokenizationFunction)].GetColumnName();
        var c06 = propertyInfos[nameof(Models.TokenizedCorpus.ScrVersType)].GetColumnName();
        var c07 = propertyInfos[nameof(Models.TokenizedCorpus.CustomVersData)].GetColumnName();
        var c08 = propertyInfos[nameof(Models.TokenizedCorpus.Metadata)].GetColumnName();
        var c09 = propertyInfos[nameof(Models.TokenizedCorpus.Created)].GetColumnName();
        var c10 = propertyInfos[nameof(Models.TokenizedCorpus.LastTokenized)].GetColumnName();
        
        return await npgqlConnection.BeginBinaryImportAsync($"COPY {entityType.GetTableName()} ({c01}, {c02}, {c03}, {c04}, {c05}, {c06}, {c07}, {c08}, {c09}, {c10}) FROM STDIN (FORMAT BINARY)");
    }

    private static async Task InsertTokenizedCorpusAsync(Models.TokenizedCorpus tokenizedCorpus, IDictionary<string, IProperty> propertyInfos, NpgsqlBinaryImporter writer)
    {
        await writer.StartRowAsync();

        await writer.WriteAsync(tokenizedCorpus.Id, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenizedCorpus.UserId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenizedCorpus.CorpusId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenizedCorpus.DisplayName, NpgsqlDbType.Text);
        await writer.WriteAsync(tokenizedCorpus.TokenizationFunction, NpgsqlDbType.Text);
        await writer.WriteAsync(tokenizedCorpus.ScrVersType, NpgsqlDbType.Integer);
        await writer.WriteAsync(tokenizedCorpus.CustomVersData, NpgsqlDbType.Text);
        await writer.WriteAsync(JsonSerializer.Serialize(tokenizedCorpus.Metadata, default(JsonSerializerOptions)), NpgsqlDbType.Jsonb);
        await writer.WriteAsync(tokenizedCorpus.Created, NpgsqlDbType.TimestampTz);
        await writer.WriteAsync(tokenizedCorpus.LastTokenized, NpgsqlDbType.TimestampTz);
    }

    private static async Task<NpgsqlBinaryImporter> BeginVerseRowBinaryImporterAsync(NpgsqlConnection npgqlConnection, IEntityType entityType, IDictionary<string, IProperty> propertyInfos)
    {
        var c01 = propertyInfos[nameof(Models.VerseRow.Id)].GetColumnName();
        var c02 = propertyInfos[nameof(Models.VerseRow.UserId)].GetColumnName();
        var c03 = propertyInfos[nameof(Models.VerseRow.OriginalText)].GetColumnName();
        var c04 = propertyInfos[nameof(Models.VerseRow.BookChapterVerse)].GetColumnName();
        var c05 = propertyInfos[nameof(Models.VerseRow.IsSentenceStart)].GetColumnName();
        var c06 = propertyInfos[nameof(Models.VerseRow.IsInRange)].GetColumnName();
        var c07 = propertyInfos[nameof(Models.VerseRow.IsRangeStart)].GetColumnName();
        var c08 = propertyInfos[nameof(Models.VerseRow.IsEmpty)].GetColumnName();
        var c09 = propertyInfos[nameof(Models.VerseRow.TokenizedCorpusId)].GetColumnName();
        var c10 = propertyInfos[nameof(Models.VerseRow.Created)].GetColumnName();
        var c11 = propertyInfos[nameof(Models.VerseRow.Modified)].GetColumnName();
        
        return await npgqlConnection.BeginBinaryImportAsync($"COPY {entityType.GetTableName()} ({c01}, {c02}, {c03}, {c04}, {c05}, {c06}, {c07}, {c08}, {c09}, {c10}, {c11}) FROM STDIN (FORMAT BINARY)");
    }

    private static async Task InsertVerseRowAsync(Models.VerseRow verseRow, IDictionary<string, IProperty> propertyInfos, NpgsqlBinaryImporter writer)
    {
        await writer.StartRowAsync();

        await writer.WriteAsync(verseRow.Id, NpgsqlDbType.Uuid);
        await writer.WriteAsync(verseRow.UserId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(verseRow.OriginalText, NpgsqlDbType.Text);
        await writer.WriteAsync(verseRow.BookChapterVerse, NpgsqlDbType.Text);
        await writer.WriteAsync(verseRow.IsSentenceStart, NpgsqlDbType.Boolean);
        await writer.WriteAsync(verseRow.IsInRange, NpgsqlDbType.Boolean);
        await writer.WriteAsync(verseRow.IsRangeStart, NpgsqlDbType.Boolean);
        await writer.WriteAsync(verseRow.IsEmpty, NpgsqlDbType.Boolean);
        await writer.WriteAsync(verseRow.TokenizedCorpusId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(verseRow.Created, NpgsqlDbType.TimestampTz);
        await writer.WriteAsync(verseRow.Modified, NpgsqlDbType.TimestampTz);
    }

    public async Task<int> BulkWriteTokensToDatabaseAsync(DbConnection connection, IModel metadataModel, ITextCorpus textCorpus, IDictionary<string, IEnumerable<VerseRow>> verseRowsByBook, string corpusDisplayName, CancellationToken cancellationToken)
    {
        if (connection is not NpgsqlConnection)
        {
            throw new ArgumentException($"Must use {nameof(NpgsqlConnection)} when using {nameof(NpgsqlTokenizedCorpusCreator)}");
        }

        var npgqlConnection = (NpgsqlConnection)connection; 
        var tokenCount = 0;

        var tokenEntityType = metadataModel.ToEntityType(typeof(Models.Token));
        var tokenPropertyInfos = tokenEntityType.GetProperties().ToDictionary(p => p.Name, p => p);

        foreach (var kvp in verseRowsByBook)
        {
            var verseRowsForBook = kvp.Value.ToList();
            for (int i = 0; i < verseRowsForBook.Count; i+=100)
            {
                var verseRows = verseRowsForBook.Skip(i).Take(100).ToList();
                using (var tokenWriter = await BeginTokenBinaryImporterAsync(npgqlConnection, tokenEntityType, tokenPropertyInfos)) 
                {
//                    int tokensWritten = 0;
                    foreach (var verseRow in verseRows)
                    {
//                        _logger.LogDebug($"Bulk writing tokens from book {kvp.Key}, verse row {verseRow.BookChapterVerse}");
                
                        foreach (var tokenComponent in verseRow.TokenComponents)
                        {
                            if (tokenComponent is Models.TokenComposite)
                            {
                                var tokenComposite = (tokenComponent as Models.TokenComposite)!;
                                foreach (var token in tokenComposite.Tokens)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    await InsertTokenAsync(token, tokenPropertyInfos, tokenWriter);
//                                    tokensWritten++;
                                }
                            }
                            else
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                await InsertTokenAsync((tokenComponent as Models.Token)!, tokenPropertyInfos, tokenWriter);
//                                tokensWritten++;
                            }
                        }
                    }

//                    _logger.LogDebug($"   Bulk writing {tokensWritten} tokens for {verseRows.Count} verse rows from book {kvp.Key}");
                    await tokenWriter.CompleteAsync(cancellationToken);
                }
            }   
        }

        var tokenCompositeEntityType = metadataModel.ToEntityType(typeof(Models.TokenComposite));
        var tokenCompositePropertyInfos = tokenCompositeEntityType.GetProperties().ToDictionary(p => p.Name, p => p);

        using (var tokenCompositeWriter = await BeginTokenCompositeBinaryImporterAsync(npgqlConnection, tokenCompositeEntityType, tokenCompositePropertyInfos))
        {
            foreach (var kvp in verseRowsByBook)
            {
                foreach (var verseRow in kvp.Value)
                {
                    foreach (var tokenComponent in verseRow.TokenComponents)
                    {
                        if (tokenComponent is Models.TokenComposite)
                        {
                            var tokenComposite = (tokenComponent as Models.TokenComposite)!;
                            await InsertTokenCompositeAsync(tokenComposite, tokenPropertyInfos, tokenCompositeWriter);
                        }
                    }
                }
            }

            await tokenCompositeWriter.CompleteAsync(cancellationToken);
        }

        var tokenAssociationEntityType = metadataModel.ToEntityType(typeof(Models.TokenCompositeTokenAssociation));
        var tokenAssociationPropertyInfos = tokenAssociationEntityType.GetProperties().ToDictionary(p => p.Name, p => p);

        using (var tokenAssociationWriter = await BeginTokenCompositeTokenAssociationBinaryImporterAsync(npgqlConnection, tokenAssociationEntityType, tokenAssociationPropertyInfos))
        {
            foreach (var kvp in verseRowsByBook)
            {
                foreach (var verseRow in kvp.Value)
                {
                    foreach (var tokenComponent in verseRow.TokenComponents)
                    {
                        if (tokenComponent is Models.TokenComposite)
                        {
                            var tokenComposite = (tokenComponent as Models.TokenComposite)!;
                            foreach (var token in tokenComposite.Tokens)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                await InsertTokenCompositeTokenAssociationAsync(token.Id, tokenComposite.Id, tokenAssociationPropertyInfos, tokenAssociationWriter);
                            }
                        }
                    }
                }
            }   

            await tokenAssociationWriter.CompleteAsync(cancellationToken);
        }

        foreach (var kvp in verseRowsByBook)
        {
            foreach (var verseRow in kvp.Value)
            {
                var compositeChildren = verseRow.TokenComponents
                    .Where(t => t is Models.TokenComposite)
                    .Select(t => ((Models.TokenComposite)t).Tokens.Count)
                    .Sum();
                tokenCount += verseRow.TokenComponents.Count + compositeChildren;
            }
        }   

        return await Task.FromResult(tokenCount);;
    }

    private static async Task<NpgsqlBinaryImporter> BeginTokenBinaryImporterAsync(NpgsqlConnection npgqlConnection, IEntityType entityType, IDictionary<string, IProperty> propertyInfos)
    {
        var c01 = propertyInfos[nameof(Models.Token.Id)].GetColumnName();
        var c02 = propertyInfos[nameof(Models.Token.EngineTokenId)].GetColumnName();
        var c03 = propertyInfos[nameof(Models.Token.TrainingText)].GetColumnName();
        var c04 = propertyInfos[nameof(Models.Token.VerseRowId)].GetColumnName();
        var c05 = propertyInfos[nameof(Models.Token.TokenizedCorpusId)].GetColumnName();
        var c06 = propertyInfos[DbCommandExtensions.DISCRIMINATOR_COLUMN_NAME].GetColumnName();
        var c07 = propertyInfos[nameof(Models.Token.BookNumber)].GetColumnName();
        var c08 = propertyInfos[nameof(Models.Token.ChapterNumber)].GetColumnName();
        var c09 = propertyInfos[nameof(Models.Token.VerseNumber)].GetColumnName();
        var c10 = propertyInfos[nameof(Models.Token.WordNumber)].GetColumnName();
        var c11 = propertyInfos[nameof(Models.Token.SubwordNumber)].GetColumnName();
        var c12 = propertyInfos[nameof(Models.Token.SurfaceText)].GetColumnName();
        var c13 = propertyInfos[nameof(Models.Token.ExtendedProperties)].GetColumnName();
        var c14 = propertyInfos[nameof(Models.Token.Deleted)].GetColumnName();
        
        return await npgqlConnection.BeginBinaryImportAsync($"COPY {entityType.GetTableName()} ({c01}, {c02}, {c03}, {c04}, {c05}, {c06}, {c07}, {c08}, {c09}, {c10}, {c11}, {c12}, {c13}, {c14}) FROM STDIN (FORMAT BINARY)");
    }

    private static async Task InsertTokenAsync(Models.Token token, IDictionary<string, IProperty> propertyInfos, NpgsqlBinaryImporter writer)
    {
        await writer.StartRowAsync();

        await writer.WriteAsync(token.Id, NpgsqlDbType.Uuid);
        await writer.WriteAsync(token.EngineTokenId, NpgsqlDbType.Text);
        await writer.WriteAsync(token.TrainingText, NpgsqlDbType.Text);
        await writer.WriteAsync(token.VerseRowId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(token.TokenizedCorpusId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(token.GetType().Name, NpgsqlDbType.Varchar);
        await writer.WriteAsync(token.BookNumber, NpgsqlDbType.Integer);
        await writer.WriteAsync(token.ChapterNumber, NpgsqlDbType.Integer);
        await writer.WriteAsync(token.VerseNumber, NpgsqlDbType.Integer);
        await writer.WriteAsync(token.WordNumber, NpgsqlDbType.Integer);
        await writer.WriteAsync(token.SubwordNumber, NpgsqlDbType.Integer);
        await writer.WriteAsync(token.SurfaceText, NpgsqlDbType.Text);
        await writer.WriteAsync(token.ExtendedProperties, NpgsqlDbType.Text);
        await writer.WriteAsync(token.Deleted, NpgsqlDbType.TimestampTz);
    }

    private static async Task<NpgsqlBinaryImporter> BeginTokenCompositeBinaryImporterAsync(NpgsqlConnection npgqlConnection, IEntityType entityType, IDictionary<string, IProperty> propertyInfos)
    {
        var c01 = propertyInfos[nameof(Models.TokenComposite.Id)].GetColumnName();
        var c02 = propertyInfos[nameof(Models.TokenComposite.EngineTokenId)].GetColumnName();
        var c03 = propertyInfos[nameof(Models.TokenComposite.TrainingText)].GetColumnName();
        var c04 = propertyInfos[nameof(Models.TokenComposite.VerseRowId)].GetColumnName();
        var c05 = propertyInfos[nameof(Models.TokenComposite.TokenizedCorpusId)].GetColumnName();
        var c06 = propertyInfos[nameof(Models.TokenComposite.ParallelCorpusId)].GetColumnName();
        var c07 = propertyInfos[DbCommandExtensions.DISCRIMINATOR_COLUMN_NAME].GetColumnName();
        var c08 = propertyInfos[nameof(Models.TokenComposite.SurfaceText)].GetColumnName();
        var c09 = propertyInfos[nameof(Models.TokenComposite.ExtendedProperties)].GetColumnName();
        var c10 = propertyInfos[nameof(Models.TokenComposite.Deleted)].GetColumnName();
        
        return await npgqlConnection.BeginBinaryImportAsync($"COPY {entityType.GetTableName()} ({c01}, {c02}, {c03}, {c04}, {c05}, {c06}, {c07}, {c08}, {c09}, {c10}) FROM STDIN (FORMAT BINARY)");
    }

    public static async Task InsertTokenCompositeAsync(Models.TokenComposite tokenComposite, IDictionary<string, IProperty> propertyInfos, NpgsqlBinaryImporter writer)
    {
        await writer.StartRowAsync();

        await writer.WriteAsync(tokenComposite.Id, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenComposite.EngineTokenId, NpgsqlDbType.Text);
        await writer.WriteAsync(tokenComposite.TrainingText, NpgsqlDbType.Text);
        await writer.WriteAsync(tokenComposite.VerseRowId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenComposite.TokenizedCorpusId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenComposite.ParallelCorpusId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenComposite.GetType().Name, NpgsqlDbType.Varchar);
        await writer.WriteAsync(tokenComposite.SurfaceText, NpgsqlDbType.Text);
        await writer.WriteAsync(tokenComposite.ExtendedProperties, NpgsqlDbType.Text);
        await writer.WriteAsync(tokenComposite.Deleted, NpgsqlDbType.TimestampTz);
    }

    private static async Task<NpgsqlBinaryImporter> BeginTokenCompositeTokenAssociationBinaryImporterAsync(NpgsqlConnection npgqlConnection, IEntityType entityType, IDictionary<string, IProperty> propertyInfos)
    {
        var c01 = propertyInfos[nameof(Models.TokenCompositeTokenAssociation.Id)].GetColumnName();
        var c02 = propertyInfos[nameof(Models.TokenCompositeTokenAssociation.TokenId)].GetColumnName();
        var c03 = propertyInfos[nameof(Models.TokenCompositeTokenAssociation.TokenCompositeId)].GetColumnName();
        
        return await npgqlConnection.BeginBinaryImportAsync($"COPY {entityType.GetTableName()} ({c01}, {c02}, {c03}) FROM STDIN (FORMAT BINARY)");
    }

    private static async Task InsertTokenCompositeTokenAssociationAsync(Guid tokenId, Guid tokenCompositeId, IDictionary<string, IProperty> propertyInfos, NpgsqlBinaryImporter writer)
    {
        await writer.StartRowAsync();

        await writer.WriteAsync(Guid.NewGuid(), NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenId, NpgsqlDbType.Uuid);
        await writer.WriteAsync(tokenCompositeId, NpgsqlDbType.Uuid);
    }
}