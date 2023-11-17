using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Threading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Corpora;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using SIL.Machine.Corpora;


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

                ///////// await UpdateAlignments();////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //Get the Parallel Corpus////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var allParallelCorpusIds =
                    ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext)
                        .Select(pc => ModelHelper.BuildParallelCorpusId(pc));

                var parallelCorpusIds = allParallelCorpusIds.Where(pc =>
                    pc.SourceTokenizedCorpusId == request.TokenizedTextCorpusId ||
                    pc.TargetTokenizedCorpusId == request.TokenizedTextCorpusId);

                var parallelCorpusId = parallelCorpusIds.ToList().FirstOrDefault();//do loop here for each parallel corpus

                var parallelCorpus = await ParallelCorpus.Get(_mediator, parallelCorpusId, cancellationToken);

                var oldParallelTextRows = parallelCorpus.Select(v => (EngineParallelTextRow)v).ToList();//what if we just just the new text for the alignment managers? SLOW

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //Get Alignments/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //do loop here for each alignmentSet
                var alignmentSetIds = await AlignmentSet.GetAllAlignmentSetIds(_mediator, parallelCorpusId);
                var alignmentSet = await AlignmentSet.Get(alignmentSetIds.FirstOrDefault(), _mediator);
                var oldAlignments = await alignmentSet.GetAlignments(oldParallelTextRows);
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //Train//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                var isTrainedSymmetrizedModel = false;

                var symmetrizationHeuristic = isTrainedSymmetrizedModel
                    ? SymmetrizationHeuristic.GrowDiagFinalAnd
                    : SymmetrizationHeuristic.None;

                TrainSmtModelSet? trainSmtModelSet = null;

                //static 
                SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
                await semaphoreSlim.WaitAsync(cancellationToken);
                var smtModelType = SmtModelType.FastAlign;
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
                    var generateAlignedTokenPairs = false;
                    if (generateAlignedTokenPairs)
                    {
                        alignedTokenPairs =
                            _translationCommands.PredictAllAlignedTokenIdPairs(wordAlignmentModel, parallelCorpus)
                        .ToList();

                        //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        //    cancellationToken, $"Generated AlignedTokenPairs '{smtModelType}'.");
                    }

                    trainSmtModelSet = new TrainSmtModelSet(wordAlignmentModel, smtModelType, isTrainedSymmetrizedModel, alignedTokenPairs);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                //Align//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (trainSmtModelSet.AlignedTokenPairs == null)
                {
                    //await semaphoreSlim.WaitAsync(cancellationToken);
                    //try
                    //{
                    // Accessing alignedTokenPairs later, during alignedTokenPairs.Create (i.e. without
                    // doing a ToList() here) would periodically throw an exception when creating alignment
                    // sets from multiple threads:
                    //  Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
                    //  at System.Collections.Generic.List`1.get_Item(Int32 index)
                    //  at ClearBible.Engine.Translation.Extensions.GetAlignedTokenPairs(EngineParallelTextRow engineParallelTextRow, WordAlignmentMatrix alignment) + MoveNext() in D:\dev\Clients\ClearBible\ClearEngine\src\ClearBible.Engine\Translation\Extensions.cs:line 15
                    //  at System.Linq.Enumerable.SelectManySingleSelectorIterator`2.MoveNext()
                    //  at System.Linq.Enumerable.SelectEnumerableIterator`2.GetCount(Boolean onlyIfCheap)
                    //  at System.Linq.Enumerable.Count[TSource](IEnumerable`1 source)
                    // So, doing a ToList() here to manifest the result and inside of a Monitor.Lock
                    // to hopefully prevent the exception above.  
                    trainSmtModelSet.AlignedTokenPairs =
                        _translationCommands.PredictAllAlignedTokenIdPairs(trainSmtModelSet.WordAlignmentModel, parallelCorpus)
                            .ToList();
                    //}
                    //finally
                    //{
                    //    semaphoreSlim.Release();
                    //}
                }

                cancellationToken.ThrowIfCancellationRequested();
                var displayName = "Temp Alignment";
                AlignmentSet newAlignmentSet = await trainSmtModelSet.AlignedTokenPairs.Create(displayName,
                    smtModel: smtModelType.ToString(),
                    isSyntaxTreeAlignerRefined: false,
                    isSymmetrized: isTrainedSymmetrizedModel,
                    metadata: new(),
                    parallelCorpusId: parallelCorpus.ParallelCorpusId,
                    _mediator,
                    cancellationToken);
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                var verificationTypes = new Dictionary<string, Models.AlignmentVerification>();
                var originatedTypes = new Dictionary<string, Models.AlignmentOriginatedFrom>();

                var redoneAlignments = trainSmtModelSet.AlignedTokenPairs.Select(a =>
                    new Alignment.Translation.Alignment(a, "Unverified", "FromAlignmentModel"));

                var alignmentSetId = Guid.NewGuid();
                var alignmentSetModel = new Models.AlignmentSet
                {
                    Id = alignmentSetId,
                    ParallelCorpusId = newAlignmentSet.AlignmentSetId.ParallelCorpusId.Id,
                    DisplayName = displayName,
                    SmtModel = newAlignmentSet.AlignmentSetId.SmtModel,
                    IsSyntaxTreeAlignerRefined = newAlignmentSet.AlignmentSetId.IsSyntaxTreeAlignerRefined,
                    IsSymmetrized = newAlignmentSet.AlignmentSetId.IsSymmetrized,
                    Metadata = newAlignmentSet.AlignmentSetId.Metadata,
                    //DerivedFrom = ,
                    //EngineWordAlignment = ,
                    Alignments = redoneAlignments
                        .Select(al => new Models.Alignment
                        {
                            SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                            TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                            Score = al.AlignedTokenPair.Score,
                            AlignmentVerification = verificationTypes[al.Verification],
                            AlignmentOriginatedFrom = originatedTypes[al.OriginatedFrom]
                        }).ToList()
                };

                cancellationToken.ThrowIfCancellationRequested();

                using var alignmentSetInsertCommand = AlignmentUtil.CreateAlignmentSetInsertCommand(connection);

                await AlignmentUtil.PrepareInsertAlignmentSetAsync(
                    alignmentSetModel,
                    alignmentSetInsertCommand,
                    ProjectDbContext.UserProvider!.CurrentUser!.Id,
                    cancellationToken);

                

                //Get alignments to be replaced
                var alignmentsToReplace = oldAlignments.Where(x => x.OriginatedFrom == "FromAlignmentModel");
                //Delete alignments to be replaced (we could do this before making the alignment)
                    //Make Delete Utils

                //Get Replacement alignments
                var tokenIdsToReplace = alignmentsToReplace.Select(oa => oa.AlignedTokenPair.SourceToken.TokenId).ToList();
                var replacementAlignments = redoneAlignments.Where(na => tokenIdsToReplace.Contains(na.AlignedTokenPair.SourceToken.TokenId));
                //Insert Replacement Alignments

                //Get New Alignments
                var oldTokenIds = oldAlignments.Select(oa => oa.AlignedTokenPair.SourceToken.TokenId).ToList();
                var newAlignments = redoneAlignments.Where(ra => !oldTokenIds.Contains(ra.AlignedTokenPair.SourceToken.TokenId));
                //Insert New Alignments

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


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

        private async Task UpdateAlignments()
        {
           
            //Delete

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
