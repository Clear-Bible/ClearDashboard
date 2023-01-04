using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class PutCompositeTokenCommandHandler : ProjectDbContextCommandHandler<PutCompositeTokenCommand,
        RequestResult<Unit>, Unit>
    {
        private readonly IMediator _mediator;

        public PutCompositeTokenCommandHandler(
            IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutCompositeTokenCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(PutCompositeTokenCommand request,
            CancellationToken cancellationToken)
        {
            if (request.CompositeToken.Tokens.Count() == 1)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains a single token"
                );
            }

            var existingTokenComposite = ProjectDbContext.TokenComposites
                .Include(tc => tc.Tokens)
                .Include(tc => tc.Translations.Where(t => t.Deleted == null))
                .Include(tc => tc.SourceAlignments.Where(a => a.Deleted == null))
                .Include(tc => tc.TargetAlignments.Where(a => a.Deleted == null))
                .Where(tc => tc.Id == request.CompositeToken.TokenId.Id)
                .FirstOrDefault();

            var compositeCandiateGuids = request.CompositeToken.Tokens
                .Union(request.CompositeToken.OtherTokens)
                .Select(t => t.TokenId.Id);

            if (existingTokenComposite is not null)
            {
                if (!compositeCandiateGuids.Any())
                {
                    ProjectDbContext.TokenComposites.Remove(existingTokenComposite);
                    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                    return new RequestResult<Unit>(Unit.Value);
                }
                
                if (existingTokenComposite.Tokens.Count == compositeCandiateGuids.Count() &&
                    existingTokenComposite.ParallelCorpusId == request.ParallelCorpusId?.Id)
                {
                    if (!compositeCandiateGuids
                        .Except(existingTokenComposite.Tokens.Select(t => t.Id))
                        .Any())
                    {
                        // No change!
                        return new RequestResult<Unit>(Unit.Value);
                    }
                }
            }

            // Validate the composite:
            var compositeCandidatesDb = ProjectDbContext.Tokens
                .Include(t => t.TokenizedCorpus)
                .Include(t => t.VerseRow)
                .Include(t => t.TokenComposite)
                .Include(t => t.Translations.Where(t => t.Deleted == null))
                .Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
                .Where(tc => compositeCandiateGuids.Contains(tc.Id))
                .ToDictionary(tc => tc.Id, tc => tc);

            if (compositeCandidatesDb.Count < compositeCandiateGuids.Count())
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains tokens not found in database"
                );
            }

            if (compositeCandidatesDb.Where(t => t.Value.Deleted != null).Any())
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains one or more deleted tokens"
                );
            }

            var tokenizedCorpora = compositeCandidatesDb.Values
                .GroupBy(t => t.TokenizedCorpus)
                .Select(t => t.Key);

            if (tokenizedCorpora.Count() > 1)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains tokens from more than one TokenizedCorpus"
                );
            }

            var compositeCorpusId = tokenizedCorpora.First()!.CorpusId;

            if (request.ParallelCorpusId is null)
            {
                var verseRowIds = compositeCandidatesDb.Values.GroupBy(t => t.VerseRowId).Select(g => g.Key);

                if (verseRowIds.Count() > 1)
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request (non ParallelCorpus composite) contains tokens from more than one VerseRow"
                    );
                }
            }

            var utcNow = DateTimeOffset.UtcNow;
            var deletedDateTime = utcNow.AddTicks(-(utcNow.Ticks % TimeSpan.TicksPerMillisecond)); // Remove any fractions of a millisecond

            var tokensAddedToComposite = new List<Models.Token>();
            var alignmentsRemoving = new List<Models.Alignment>();

            // Add or update, possibly remove remaining empty composite(s)
            if (existingTokenComposite is not null)
            {
                var childrenTokensToAdd = compositeCandidatesDb.Values
                    .ExceptBy(existingTokenComposite.Tokens.Select(t => t.Id), t => t.Id);
                
                foreach (var toAdd in childrenTokensToAdd)
                {
                    var previousTokenComposite = toAdd.TokenComposite;
                    previousTokenComposite?.Tokens.Remove(toAdd);

                    existingTokenComposite.Tokens.Add(toAdd);
                    tokensAddedToComposite.Add(toAdd);

                    if (previousTokenComposite is not null && !previousTokenComposite.Tokens.Any())
                    {
                        ProjectDbContext.TokenComposites.Remove(previousTokenComposite);
                    }
                }

                var childrenTokensToRemove = existingTokenComposite.Tokens
                    .ExceptBy(compositeCandiateGuids, t => t.Id);

                foreach (var toRemove in childrenTokensToRemove)
                {
                     existingTokenComposite.Tokens.Remove(toRemove);
                }

                foreach (var e in existingTokenComposite.Translations) { e.Deleted = deletedDateTime; }
                foreach (var e in existingTokenComposite.SourceAlignments) { e.Deleted = deletedDateTime; }
                foreach (var e in existingTokenComposite.TargetAlignments) { e.Deleted = deletedDateTime; }

                alignmentsRemoving.AddRange(existingTokenComposite.SourceAlignments);
                alignmentsRemoving.AddRange(existingTokenComposite.TargetAlignments);

            }
            else
            {
                existingTokenComposite = new Models.TokenComposite
                {
                    Id = request.CompositeToken.TokenId.Id,
                };

                foreach (var compositeCandidateDb in compositeCandidatesDb.Values)
                {
                    existingTokenComposite.Tokens.Add(compositeCandidateDb);
                    tokensAddedToComposite.Add(compositeCandidateDb);
                }

                ProjectDbContext.TokenComposites.Add(existingTokenComposite);
            }

            existingTokenComposite.TokenizedCorpusId = compositeCandidatesDb.Values.First().TokenizedCorpusId;
            existingTokenComposite.ParallelCorpusId = request.ParallelCorpusId?.Id;
            existingTokenComposite.TrainingText = request.CompositeToken.TrainingText;
            existingTokenComposite.ExtendedProperties = request.CompositeToken.ExtendedProperties;
            existingTokenComposite.EngineTokenId = request.CompositeToken.TokenId.ToString();
            existingTokenComposite.Deleted = null;

            if (request.ParallelCorpusId == null && compositeCandidatesDb.GroupBy(e => e.Value.VerseRowId).Count() == 1)
            {
                existingTokenComposite.VerseRowId = compositeCandidatesDb.Values.First().VerseRowId;
            }
            else
            {
                existingTokenComposite.VerseRowId = null;
            }
            
            // Adding Tokens to a Composite effectively removes them from
            // the pool of regular Tokens, so if there are any Translations/Alignments
            // associated, soft delete them:
            tokensAddedToComposite.ForEach(tc =>
            {
                foreach (var e in tc.Translations) { e.Deleted = deletedDateTime; }
                foreach (var e in tc.SourceAlignments) { e.Deleted = deletedDateTime; }
                foreach (var e in tc.TargetAlignments) { e.Deleted = deletedDateTime; }

                alignmentsRemoving.AddRange(tc.SourceAlignments);
                alignmentsRemoving.AddRange(tc.TargetAlignments);
            });

            if (alignmentsRemoving.Any())
            {
                using (var transaction = ProjectDbContext.Database.BeginTransaction())
                {
                    await _mediator.Publish(new AlignmentAddingRemovingEvent(alignmentsRemoving, null, ProjectDbContext), cancellationToken);
                    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                    transaction.Commit();
                }

                await _mediator.Publish(new AlignmentAddedRemovedEvent(alignmentsRemoving, null), cancellationToken);
            }
            else
            {
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}