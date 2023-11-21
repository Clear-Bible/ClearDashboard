using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using System.Data.Common;
using System.Threading.Tasks;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using SIL.Machine.SequenceAlignment;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class UpdateOrAddVersesInTokenizedCorpusCommandHandler : ProjectDbContextCommandHandler<
        UpdateOrAddVersesInTokenizedCorpusCommand, 
        RequestResult<IEnumerable<string>>,
        IEnumerable<string>>
    {
        private readonly IMediator _mediator;
        private readonly TranslationCommands _translationCommands;

        public UpdateOrAddVersesInTokenizedCorpusCommandHandler(
            IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            TranslationCommands translationCommands,
            ILogger<UpdateOrAddVersesInTokenizedCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
            _mediator = mediator;
            _translationCommands = translationCommands;
        }

        protected override async Task<RequestResult<IEnumerable<string>>> SaveDataAsync(UpdateOrAddVersesInTokenizedCorpusCommand request, CancellationToken cancellationToken)
        {
            var tokenizedCorpusGuid = request.TokenizedTextCorpusId.Id;
            var corpusId = request.TokenizedTextCorpusId.CorpusId!;

            IEnumerable<string> targetTextIds = request.TextCorpus.Texts.Select(t => t.Id);

            var bookIdsToUpdate = targetTextIds.Intersect(request.ExistingBookIds);
            var bookIdsToInsert = targetTextIds.Except(request.ExistingBookIds);

            var alignmentsRemoving = new List<Models.Alignment>();

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                using var verseRowInsertCommand = TokenizedCorpusDataBuilder.CreateVerseRowInsertCommand(connection);
                using var tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection);
                using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection);

                if (bookIdsToInsert.Any())
                {
                    foreach (var bookId in bookIdsToInsert)
                    {
                        var tokensTextRows = TokenizedCorpusDataBuilder.ExtractValidateBook(request.TextCorpus, bookId, corpusId.Name);
                        var (verseRows, btTokenCount) = TokenizedCorpusDataBuilder.BuildVerseRowModel(tokensTextRows, tokenizedCorpusGuid);

                        foreach (var verseRow in verseRows)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            await TokenizedCorpusDataBuilder.InsertVerseRowAsync(verseRow, verseRowInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                            await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, tokenCompositeTokenAssociationInsertCommand, cancellationToken);
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

                    var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

                    using var tokenSD = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.TokenComponent));
                    using var taSD    = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.TokenCompositeTokenAssociation));
                    using var transSD = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.Translation));
                    using var alignSD = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.Alignment));
                    using var tvaSD   = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(Models.TokenVerseAssociation));
                    using var verseRowUpdate = TokenizedCorpusDataBuilder.CreateVerseRowUpdateCommand(connection);

                    async Task del(Guid id, DbCommand cmd) => await DataUtil.SoftDeleteByIdAsync(currentDateTime, id, cmd, cancellationToken);

                    foreach (var bookId in bookIdsToUpdate)
                    {
                        var bookNumberAsPaddedString = $"{ModelHelper.GetBookNumberForSILAbbreviation(bookId):000}";

                        var tokensTextRows = TokenizedCorpusDataBuilder.ExtractValidateBook(request.TextCorpus, bookId, corpusId.Name);
                        var (verseRows, btTokenCount) = TokenizedCorpusDataBuilder.BuildVerseRowModel(tokensTextRows, tokenizedCorpusGuid);

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
                                    // delete(soft delete so notes references work ?) all Tokens and associated Alignments:
                                    var tokensToSoftDelete = ProjectDbContext.Tokens
                                        .Include(tc => tc.TokenCompositeTokenAssociations.Where(ta => ta.Deleted == null))
                                        .Include(tc => tc.TokenVerseAssociations.Where(tva => tva.Deleted == null))
                                        .Include(tc => tc.Translations.Where(t => t.Deleted == null))
                                        .Include(tc => tc.SourceAlignments.Where(a => a.Deleted == null))
                                        .Include(tc => tc.TargetAlignments.Where(a => a.Deleted == null))
                                        .Where(tc => tc.Deleted == null)
                                        .Where(tc => tc.VerseRowId == verseRowDb.Id);

                                    var referencedTokenCompositeIds = new List<Guid>();
                                    foreach (var tc in tokensToSoftDelete)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();

                                        await del(tc.Id, tokenSD);
                                        referencedTokenCompositeIds.AddRange(tc.TokenCompositeTokenAssociations.Select(ta => ta.TokenCompositeId));

                                        foreach (var e in tc.TokenCompositeTokenAssociations) { await del(e.Id, taSD); }
                                        foreach (var e in tc.TokenVerseAssociations) { await del(e.Id, tvaSD); }
                                        foreach (var e in tc.Translations) { await del(e.Id, transSD); }
                                        foreach (var e in tc.SourceAlignments) { await del(e.Id, alignSD); } 
                                        foreach (var e in tc.TargetAlignments) { await del(e.Id, alignSD); }

                                        alignmentsRemoving.AddRange(tc.SourceAlignments);
                                        alignmentsRemoving.AddRange(tc.TargetAlignments);
                                    }

                                    // delete (soft) TokenComposites that were referenced by the Tokens deleted above
                                    // as well as their associated Alignments:
                                    ProjectDbContext.TokenComposites
                                        .Include(tc => tc.TokenCompositeTokenAssociations.Where(ta => ta.Deleted == null))
                                        .Include(tc => tc.TokenVerseAssociations.Where(tva => tva.Deleted == null))
                                        .Include(tc => tc.Translations.Where(t => t.Deleted == null))
                                        .Include(tc => tc.SourceAlignments.Where(a => a.Deleted == null))
                                        .Include(tc => tc.TargetAlignments.Where(a => a.Deleted == null))
                                        .Where(tc => tc.Deleted == null)
                                        .Where(tc => referencedTokenCompositeIds.Contains(tc.Id))
                                        .ToList()
                                        .ForEach(async tc => {
                                            foreach (var e in tc.TokenCompositeTokenAssociations) { await del(e.Id, taSD); }
                                            foreach (var e in tc.TokenVerseAssociations) { await del(e.Id, tvaSD); }
                                            foreach (var e in tc.Translations) { await del(e.Id, transSD); }
                                            foreach (var e in tc.SourceAlignments) { await del(e.Id, alignSD); }
                                            foreach (var e in tc.TargetAlignments) { await del(e.Id, alignSD); }

                                            alignmentsRemoving.AddRange(tc.SourceAlignments);
                                            alignmentsRemoving.AddRange(tc.TargetAlignments);
                                        });

                                    // update the verse row and add the new Tokens:
                                    verseRow.Id = verseRowDb.Id;
                                    verseRow.Modified = currentDateTime;
                                    foreach (var tc in verseRow.TokenComponents)
                                    {
                                        tc.VerseRowId = verseRowDb.Id;
                                    }

                                    await TokenizedCorpusDataBuilder.UpdateVerseRowAsync(verseRow, verseRowUpdate, cancellationToken);
                                    await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, tokenCompositeTokenAssociationInsertCommand, cancellationToken);
                                }
                            }
                            else
                            {
                                await TokenizedCorpusDataBuilder.InsertVerseRowAsync(verseRow, verseRowInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                                await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, tokenCompositeTokenAssociationInsertCommand, cancellationToken);
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
                        
                    await UpdateAlignments(request, connection, cancellationToken);//Make it it's own transaction
                }
                else
                {
                    await transaction.CommitAsync(cancellationToken);
                    
                    await UpdateAlignments(request, connection, cancellationToken);//Make it it's own transaction
                }
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }

            if (alignmentsRemoving.Any())
            {
                await _mediator.Publish(new AlignmentAddedRemovedEvent(alignmentsRemoving, null), cancellationToken);
            }

            return new RequestResult<IEnumerable<string>>(bookIdsToInsert);
        }

        private async Task<RequestResult<AlignmentSet>> UpdateAlignments(UpdateOrAddVersesInTokenizedCorpusCommand request, DbConnection connection, CancellationToken cancellationToken)
        {
            foreach (var alignmentSetIdToRedo in request.AlignmentSetsToRedo)
            {
                //Get the Parallel Corpus
                var parallelCorpusId = alignmentSetIdToRedo.ParallelCorpusId;
                var parallelCorpus = await ParallelCorpus.Get(_mediator, parallelCorpusId, cancellationToken);

              
                //Train and Align//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var isTrainedSymmetrizedModel = false;

                var symmetrizationHeuristic = isTrainedSymmetrizedModel
                    ? SymmetrizationHeuristic.GrowDiagFinalAnd
                    : SymmetrizationHeuristic.None;

                //static 
                SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
                await semaphoreSlim.WaitAsync(cancellationToken);
                var smtModelType = SmtModelType.FastAlign;
                TrainSmtModelSet? trainSmtModelSet = null;
                try
                {
                    var wordAlignmentModel = await _translationCommands.TrainSmtModel(
                        smtModelType,
                        parallelCorpus,
                    new ClearBible.Engine.Utils.DelegateProgress(async status =>
                    {
                        var message =
                                $"Training symmetrized {smtModelType} model: {status.PercentCompleted:P}";
                        //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                        //    message);
                        Logger!.LogInformation(message);
                    }), symmetrizationHeuristic);

                    //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                    //    cancellationToken, $"Completed SMT Model '{smtModelType}'.");

                    IEnumerable<AlignedTokenPairs>? alignedTokenPairs = null;
                    
                    
                    alignedTokenPairs =
                        _translationCommands.PredictAllAlignedTokenIdPairs(wordAlignmentModel, parallelCorpus)
                            .ToList();
                    

                    trainSmtModelSet = new TrainSmtModelSet(wordAlignmentModel, smtModelType, isTrainedSymmetrizedModel, alignedTokenPairs);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                
                cancellationToken.ThrowIfCancellationRequested();

                var verificationTypes = new Dictionary<string, Models.AlignmentVerification>();
                var originatedTypes = new Dictionary<string, Models.AlignmentOriginatedFrom>();

                var redoneAlignments = trainSmtModelSet.AlignedTokenPairs.Select(a =>
                    new Alignment.Translation.Alignment(a, "Unverified", "FromAlignmentModel"));

                foreach (var al in redoneAlignments)
                {
                    if (Enum.TryParse(al.Verification, out Models.AlignmentVerification verificationType))
                    {
                        verificationTypes[al.Verification] = verificationType;
                    }
                    else
                    {
                        return new RequestResult<AlignmentSet>
                        (
                            success: false,
                            message: $"Invalid alignment verification type '{al.Verification}' found in request"
                        );
                    }

                    if (Enum.TryParse(al.OriginatedFrom, out Models.AlignmentOriginatedFrom originatedType))
                    {
                        originatedTypes[al.OriginatedFrom] = originatedType;
                    }
                    else
                    {
                        return new RequestResult<AlignmentSet>
                        (
                            success: false,
                            message: $"Invalid alignment originated from type '{al.OriginatedFrom}' found in request"
                        );
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                var oldParallelTextRows = parallelCorpus.Select(v => (EngineParallelTextRow)v).ToList();//SLOW, what if we just just the new text for the alignment managers? SLOW
                var oldAlignmentSet = await AlignmentSet.Get(alignmentSetIdToRedo, _mediator);
                var oldAlignments = await oldAlignmentSet.GetAlignments(oldParallelTextRows);

                //Get alignments to be replaced
                var alignmentsToReplace = oldAlignments.Where(x => x.OriginatedFrom == "FromAlignmentModel");
                var alignmentsToReplaceModel = alignmentsToReplace
                    .Select(al => new Models.Alignment
                    {
                        SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                        TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                        Score = al.AlignedTokenPair.Score,
                        AlignmentVerification = verificationTypes[al.Verification],
                        AlignmentOriginatedFrom = originatedTypes[al.OriginatedFrom]
                    }).ToList();

                var tokenIdsToReplace = alignmentsToReplace.Select(oa => oa.AlignedTokenPair.SourceToken.TokenId.Id).ToList();
                
                var dbAlignmentsToReplace = ProjectDbContext!.Alignments
                    .Where(dba => tokenIdsToReplace.Contains(dba.SourceTokenComponentId));

                foreach (var dbAlignmentToReplace in dbAlignmentsToReplace)
                {
                    if (dbAlignmentToReplace != null && dbAlignmentToReplace.Deleted is null)
                    {
                        dbAlignmentToReplace.Deleted = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

                        using (var transaction = ProjectDbContext.Database.BeginTransaction())
                        {
                            dbAlignmentToReplace.AddDomainEvent(new AlignmentAddingRemovingEvent(
                                alignmentsToReplaceModel,
                                null,
                                ProjectDbContext));

                            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                            await transaction.CommitAsync(cancellationToken);
                        }

                        await _mediator.Publish(new AlignmentAddedRemovedEvent(alignmentsToReplaceModel, dbAlignmentToReplace), cancellationToken);
                    }
                }

                //Get Replacement alignments
                var redoneAlignmentsModel = redoneAlignments
                    .Select(al => new Models.Alignment
                    {
                        SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                        TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                        Score = al.AlignedTokenPair.Score,
                        AlignmentVerification = verificationTypes[al.Verification],
                        AlignmentOriginatedFrom = originatedTypes[al.OriginatedFrom]
                    }).ToList();

                var displayName = "Temp Alignment";

                var newAlignmentSetId = Guid.NewGuid();
                var redoneAlignmentSetModel = new Models.AlignmentSet
                {
                    Id = newAlignmentSetId,
                    ParallelCorpusId = parallelCorpus.ParallelCorpusId.Id,
                    DisplayName = displayName,
                    SmtModel = smtModelType.ToString(),
                    IsSyntaxTreeAlignerRefined = false,//how do I know this is false?
                    IsSymmetrized = isTrainedSymmetrizedModel,
                    Metadata = new(),
                    Alignments = redoneAlignmentsModel
                };

                var replacementAlignmentsModel = redoneAlignmentsModel.Where(na => tokenIdsToReplace.Contains(na.SourceTokenComponentId));
                

                //Insert Replacement Alignments
                using var replacementAlignmentsInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
                await AlignmentUtil.InsertAlignmentsAsync(
                    replacementAlignmentsModel,
                    alignmentSetIdToRedo.Id,
                    replacementAlignmentsInsertCommand,
                    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                    cancellationToken);

                //Get New Alignments
                var oldTokenIds = oldAlignments.Select(oa => oa.AlignedTokenPair.SourceToken.TokenId.Id).ToList();
                var newAlignments = redoneAlignmentSetModel.Alignments.Where(ra => !oldTokenIds.Contains(ra.SourceTokenComponentId));

                //Insert New Alignments
                using var newAlignmentsInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
                await AlignmentUtil.InsertAlignmentsAsync(
                    newAlignments,
                    alignmentSetIdToRedo.Id,
                    newAlignmentsInsertCommand,
                    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                    cancellationToken);

                ProjectDbContext.AlignmentSetDenormalizationTasks.Add(new Models.AlignmentSetDenormalizationTask()
                {
                    AlignmentSetId = alignmentSetIdToRedo.Id
                });

                await ProjectDbContext.SaveChangesAsync();
            }

            return new RequestResult<AlignmentSet>
            (
                success: true,
                message: $"Alignments Updated!"
            );
        }

        public record TrainSmtModelSet
        {
            public TrainSmtModelSet(IWordAlignmentModel wordAlignmentModel, SmtModelType smtModelType, bool isTrainedSymmetrizedModel, IEnumerable<AlignedTokenPairs>? alignedTokenPairs)
            {
                WordAlignmentModel = wordAlignmentModel;
                SmtModelType = smtModelType;
                IsTrainedSymmetrizedModel = isTrainedSymmetrizedModel;
                AlignedTokenPairs = alignedTokenPairs;
            }

            public IWordAlignmentModel WordAlignmentModel { get; set; }
            public SmtModelType SmtModelType { get; set; }
            public bool IsTrainedSymmetrizedModel { get; set; }
            public IEnumerable<AlignedTokenPairs>? AlignedTokenPairs { get; set; }
        }
    }
}
