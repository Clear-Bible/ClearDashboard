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
        public PutCompositeTokenCommandHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutCompositeTokenCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
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
                .Where(tc => tc.Id == request.CompositeToken.TokenId.Id)
                .FirstOrDefault();

            var compositeCandiateGuids = request.CompositeToken.Select(t => t.TokenId.Id);

            if (existingTokenComposite is not null)
            {
                if (!compositeCandiateGuids.Any())
                {
                    ProjectDbContext.TokenComposites.Remove(existingTokenComposite);
                    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                    return new RequestResult<Unit>(Unit.Value);
                }
                else if (existingTokenComposite.Tokens.Count == compositeCandiateGuids.Count())
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

            if (request.ParallelCorpusId is not null)
            {
                // In this section we are trying to find all VerseMappings that relate to any of the
                // candidate composite child tokens.  Either by Verse+TokenVerseAssociation or Verse Book/Chapter/Verse.
                // Then, we validate that all these child tokens are relate either these two ways with all
                // resulting VerseMapping candidates.  

                var bcvGroups = compositeCandidatesDb.Values
                    .GroupBy(tc => tc.VerseRow!.BookChapterVerse!)
                    .Select(g => new
                    {
                        Ids = g.Select(t => t.Id),
                        B = int.Parse(g.Key[..3]),
                        C = int.Parse(g.Key[3..^3]),
                        V = int.Parse(g.Key[6..^0])
                    });

                // Find all VerseMappings that relate to any of the composite child tokens by Verse+TokenVerseAssociation
                var matchingVerseMappingsDb = ProjectDbContext.Verses
                    .Include(v => v.TokenVerseAssociations)
                    .Include(v => v.VerseMapping)
                        .ThenInclude(vm => vm!.Verses)
                            .ThenInclude(v => v.TokenVerseAssociations)
                    .Where(v => v.VerseMapping!.ParallelCorpusId == request.ParallelCorpusId.Id)
                    .Where(v => v.CorpusId == compositeCorpusId)
                    .Where(v => v.TokenVerseAssociations.Any(tva => compositeCandiateGuids.Contains(tva.TokenComponentId)))
                    .Select(v => v.VerseMapping!)
                    .ToList();

                // Find all VerseMappings that relate to any of the composite child tokens by Verse book/chapter/verse
                foreach (var bcvGroup in bcvGroups)
                {
                    matchingVerseMappingsDb.AddRange(ProjectDbContext.Verses
                        .Include(v => v.VerseMapping)
                            .ThenInclude(vm => vm!.Verses)
                                .ThenInclude(v => v.TokenVerseAssociations)
                        .Where(v => v.VerseMapping!.ParallelCorpusId == request.ParallelCorpusId.Id)
                        .Where(v => v.CorpusId == compositeCorpusId)
                        .Where(v => v.BookNumber == bcvGroup.B && v.ChapterNumber == bcvGroup.C && v.VerseNumber == bcvGroup.V)
                        .Select(v => v.VerseMapping!));
                }

                var idBcvs = bcvGroups
                    .SelectMany(g => g.Ids, (g, id) => new { id, g.B, g.C, g.V })
                    .ToDictionary(i => i.id, i => i);

                foreach (var matchingVerseMappingDb in matchingVerseMappingsDb.DistinctBy(vm => vm.Id))
                {
                    var tokensInVerseMappingCount = 0;
                    foreach (var compositeCandidateDbId in compositeCandidatesDb.Keys)
                    {
                        if (matchingVerseMappingDb!.Verses
                            .Any(v => v.TokenVerseAssociations
                                .Any(tva => compositeCandidateDbId == tva.TokenComponentId)))
                        {
                            tokensInVerseMappingCount++;
                            continue;
                        }

                        var bcv = idBcvs[compositeCandidateDbId];
                        if (matchingVerseMappingDb!.Verses
                            .Where(v => v.BookNumber == bcv.B && v.ChapterNumber == bcv.C && v.VerseNumber == bcv.V)
                            .Any())
                        {
                            tokensInVerseMappingCount++;
                            continue;
                        }
                    }

                    // If any of the composite child tokens does not relate to this VerseMapping
                    // using the criteria above, then return an error:
                    if (tokensInVerseMappingCount != compositeCandidatesDb.Count)
                    {
                        return new RequestResult<Unit>
                        (
                            success: false,
                            message: $"CompositeToken '{request.CompositeToken.TokenId}' only has some child tokens present in VerseMapping candidate '{matchingVerseMappingDb.Id}'"
                        );
                    }
                }
            }
            else
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
            }
            else
            {
                var tokenComposite = new Models.TokenComposite
                {
                    Id = request.CompositeToken.TokenId.Id,
                    VerseRowId = (request.ParallelCorpusId == null) ? compositeCandidatesDb.Values.First().VerseRowId : null,
                    ParallelCorpusId = request.ParallelCorpusId?.Id,
                    TokenizedCorpusId = compositeCandidatesDb.Values.First().TokenizedCorpusId,
                    TrainingText = request.CompositeToken.TrainingText,
                    ExtendedProperties = request.CompositeToken.ExtendedProperties,
                    EngineTokenId = request.CompositeToken.TokenId.ToString()
                };

                foreach (var compositeCandidateDb in compositeCandidatesDb.Values)
                {
                    tokenComposite.Tokens.Add(compositeCandidateDb);
                }

                ProjectDbContext.TokenComposites.Add(tokenComposite);
            }

            // FIXME!  
            //  - Be sure to soft delete alignments + translations when moving individual tokens into a composite,
            //    changing/deleting a composite
            //  - Trigger denormalization when Composites change (either TrainingText change or new composite)
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}