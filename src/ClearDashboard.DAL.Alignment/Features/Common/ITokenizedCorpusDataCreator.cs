using Microsoft.EntityFrameworkCore.Metadata;
using System.Data.Common;
using SIL.Machine.Corpora;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common;

public interface ITokenizedCorpusDataCreator
{
    Task<(int VerseRowCount, int TokenComponentCount)> BulkWriteTokenizedCorpusToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel, 
        Models.TokenizedCorpus tokenizedCorpus, 
        ITextCorpus textCorpus, 
        CancellationToken cancellationToken);

    Task<int> BulkWriteVerseRowsToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel,
        IDictionary<string, IEnumerable<Models.VerseRow>> verseRowsByBook,
        CancellationToken cancellationToken);

    Task<int> BulkWriteTokensToDatabaseAsync(
        DbConnection connection, 
        IModel metadataModel,
        ITextCorpus textCorpus,
        IDictionary<string, IEnumerable<Models.VerseRow>> verseRowsByBook,
        string corpusDisplayName,
        CancellationToken cancellationToken);
}


