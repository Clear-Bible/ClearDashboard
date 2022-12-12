using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data.Common;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class UpdateOrAddVersesInTokenizedCorpusCommandHandler : ProjectDbContextCommandHandler<
        UpdateOrAddVersesInTokenizedCorpusCommand, 
        RequestResult<IEnumerable<string>>,
        IEnumerable<string>>
    {
        private readonly IMediator _mediator;

        public UpdateOrAddVersesInTokenizedCorpusCommandHandler(
            IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<UpdateOrAddVersesInTokenizedCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IEnumerable<string>>> SaveDataAsync(UpdateOrAddVersesInTokenizedCorpusCommand request, CancellationToken cancellationToken)
        {
            var tokenizedCorpusGuid = request.TokenizedTextCorpusId.Id;
            var corpusId = request.TokenizedTextCorpusId.CorpusId!;

            IEnumerable<string> targetTextIds = request.TextCorpus.Texts.Select(t => t.Id);

            var bookIdsToUpdate = targetTextIds.Intersect(request.ExistingBookIds);
            var bookIdsToInsert = targetTextIds.Except(request.ExistingBookIds);

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                using var verseRowInsertCommand = TokenizedCorpusDataUtil.CreateVerseRowInsertCommand(connection);
                using var tokenComponentInsertCommand = TokenizedCorpusDataUtil.CreateTokenComponentInsertCommand(connection);

                var alignmentsRemoving = new List<Models.Alignment>();

                if (bookIdsToInsert.Any())
                {
                    foreach (var bookId in bookIdsToInsert)
                    {
                        var tokensTextRows = TokenizedCorpusDataUtil.ExtractValidateBook(request.TextCorpus, bookId, corpusId);
                        var (verseRows, btTokenCount) = TokenizedCorpusDataUtil.BuildVerseRowModel(tokensTextRows, tokenizedCorpusGuid);

                        foreach (var verseRow in verseRows)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await TokenizedCorpusDataUtil.InsertVerseRowAsync(verseRow, verseRowInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                            await TokenizedCorpusDataUtil.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, cancellationToken);
                        }
                    }
                }

                if (bookIdsToUpdate.Any())
                {
                    // In case we want to only soft-delete VerseRows and/or TokenComponents that have
                    // Notes associated with them and otherwise hard-delete all others:
                    //var noteDomainEntityIIdsByGenericTypeName = ProjectDbContext.NoteDomainEntityAssociations
                    //    .Select(nd => nd.DomainEntityIdName!.CreateInstanceByNameAndSetId((Guid)nd.DomainEntityIdGuid!)).ToHashSet()
                    //    .GroupBy(e => e.GetType().FindEntityIdGenericType()?.Name ?? $"__{e.GetType().Name}")
                    //    .ToDictionary(g => g.Key, g => g.Select(g => g.Id));

                    var utcNow = DateTimeOffset.UtcNow;
                    var deletedDateTime = utcNow.AddTicks(-(utcNow.Ticks % TimeSpan.TicksPerMillisecond)); // Remove any fractions of a millisecond

                    using var tokenSD = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.TokenComponent));
                    using var transSD = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.Translation));
                    using var alignSD = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.Alignment));
                    using var tvaSD   = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.TokenVerseAssociation));
                    using var tcIdUpdate = TokenizedCorpusDataUtil.CreateTokenCompositeIdUpdateCommand(connection);

                    async Task del(Guid id, DbCommand cmd) => await DataUtil.SoftDeleteByIdAsync(deletedDateTime, id, cmd, cancellationToken);
                    async Task tokenCompositeIdNull(Guid id) => await TokenizedCorpusDataUtil.SetTokenCompositeIdAsync(null, id, tcIdUpdate, cancellationToken);

                    foreach (var bookId in bookIdsToUpdate)
                    {
                        var bookNumberAsPaddedString = $"{ModelHelper.GetBookNumberForSILAbbreviation(bookId):000}";

                        var tokensTextRows = TokenizedCorpusDataUtil.ExtractValidateBook(request.TextCorpus, bookId, corpusId);
                        var (verseRows, btTokenCount) = TokenizedCorpusDataUtil.BuildVerseRowModel(tokensTextRows, tokenizedCorpusGuid);

                        var bookVerseRowsDb = ProjectDbContext.VerseRows
                            .Where(vr => vr.TokenizedCorpusId == tokenizedCorpusGuid)
                            .Where(vr => vr.BookChapterVerse!.Substring(0, 3) == bookNumberAsPaddedString)
                            .ToList();

                        foreach (var verseRow in verseRows)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var verseRowDb = bookVerseRowsDb.Where(vr => vr.BookChapterVerse == verseRow.BookChapterVerse).FirstOrDefault();
                            if (verseRowDb is not null)
                            {
                                if (verseRowDb.OriginalText != verseRow.OriginalText)
                                {
                                    // delete(soft delete so notes references work ?) all tokens and alignments.
                                    var tokensToSoftDelete = ProjectDbContext.TokenComponents
                                        .Include(tc => tc.TokenVerseAssociations.Where(tva => tva.Deleted == null))
                                        .Include(tc => tc.Translations.Where(t => t.Deleted == null))
                                        .Include(tc => tc.SourceAlignments.Where(a => a.Deleted == null))
                                        .Include(tc => tc.TargetAlignments.Where(a => a.Deleted == null))
                                        .Where(tc => tc.Deleted == null)
                                        .Where(tc => tc.VerseRowId == verseRowDb.Id);

                                    var referencedTokenCompositeIds = new List<Guid>();
                                    foreach (var tc in tokensToSoftDelete)
                                    {
                                        await del(tc.Id, tokenSD); ;
                                        foreach (var e in tc.TokenVerseAssociations) { await del(e.Id, tvaSD); }
                                        foreach (var e in tc.Translations) { await del(e.Id, transSD); }
                                        foreach (var e in tc.SourceAlignments) { await del(e.Id, alignSD); }
                                        foreach (var e in tc.TargetAlignments) { await del(e.Id, alignSD); }

                                        alignmentsRemoving.AddRange(tc.SourceAlignments);
                                        alignmentsRemoving.AddRange(tc.TargetAlignments);

                                        if (tc.GetType() == typeof(Models.Token) && ((Models.Token)tc).TokenCompositeId is not null)
                                        {
                                            referencedTokenCompositeIds.Add((Guid)((Models.Token)tc).TokenCompositeId!);
                                        }
                                    }

                                    // TokenComposites that were referenced by the TokenComponents soft deleted above,
                                    // but themselves weren't soft deleted (because they weren't associated with the
                                    // VerseRowId...i.e. they were ParallelCorpusId TokenComposites:
                                    ProjectDbContext.TokenComposites
                                        .Include(tc => tc.Tokens)
                                        .Where(tc => tc.Deleted == null)
                                        .Where(tc => referencedTokenCompositeIds.Contains(tc.Id))
                                        .ToList()
                                        .ForEach(async e => {
                                            e.Tokens.Where(t => t.Deleted == null).ToList().ForEach(async et => await tokenCompositeIdNull(et.Id));
                                            await del(e.Id, tokenSD);
                                        });

                                    // add the new Tokens
                                    await TokenizedCorpusDataUtil.InsertVerseRowAsync(verseRow, verseRowInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                                    await TokenizedCorpusDataUtil.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, cancellationToken);
                                }
                            }
                            else
                            {
                                await TokenizedCorpusDataUtil.InsertVerseRowAsync(verseRow, verseRowInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                                await TokenizedCorpusDataUtil.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, cancellationToken);
                            }
                        }
                    }
                }

                if (alignmentsRemoving.Any())
                {
                    // Explicitly setting the DatabaseFacade transaction to match
                    // the underlying DbConnection transaction in case any event handlers
                    // need to alter data as part of the current transaction:
                    ProjectDbContext.Database.UseTransaction(transaction);
                    await _mediator.Publish(new AlignmentAddingRemovingEvent(alignmentsRemoving, null, ProjectDbContext), cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    ProjectDbContext.Database.UseTransaction(null);
                } 
                else
                {
                    await transaction.CommitAsync(cancellationToken);
                }

                return new RequestResult<IEnumerable<string>>(bookIdsToInsert);
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }
    }
}
