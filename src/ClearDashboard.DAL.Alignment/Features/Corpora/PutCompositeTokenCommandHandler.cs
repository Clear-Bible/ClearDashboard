using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
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
            var existingTokenComposite = ProjectDbContext.TokenComposites
                .Include(tc => tc.Tokens)
                .Where(tc => tc.Id == request.CompositeToken.TokenId.Id)
                .FirstOrDefault();

            if (existingTokenComposite is not null && existingTokenComposite.Tokens.Count == request.CompositeToken.Tokens.Count())
            {
                if (!request.CompositeToken.Tokens
                    .Select(t => t.TokenId.Id)
                    .Except(existingTokenComposite.Tokens.Select(t => t.Id))
                    .Any())
                {
                    return new RequestResult<Unit>(Unit.Value);
                }
            }

            // Validate the composite:
            var compositeCandidatesDb = ProjectDbContext.Tokens
                .Include(t => t.TokenizedCorpus)
                .Where(tc => request.CompositeToken.Select(t => t.TokenId.Id).Contains(tc.Id))
                .ToDictionary(tc => tc.Id, tc => tc);

            if (compositeCandidatesDb.Count < request.CompositeToken.Tokens.Count())
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains tokens not found in database"
                );
            }

            var tokenizedCorpusIds = compositeCandidatesDb.Values.GroupBy(t => t.TokenizedCorpusId).Select(g => g.Key);

            if (tokenizedCorpusIds.Count() > 1)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains tokens from more than one TokenizedCorpus"
                );
            }

            if (request.ParallelCorpusId is not null)
            {
                var tokenIdToBCV = compositeCandidatesDb.Values
                    .ToDictionary(tc => tc.Id, tc => new
                    {
                        B = int.Parse(tc.VerseRow!.BookChapterVerse![..3]),
                        C = int.Parse(tc.VerseRow!.BookChapterVerse![3..3]),
                        V = int.Parse(tc.VerseRow!.BookChapterVerse![6..3])
                    });

                var firstBCV = tokenIdToBCV.First().Value;

                var candidateVerseMappingsDb = ProjectDbContext.Verses
                    .Include(v => v.VerseMapping)
                        .ThenInclude(vm => vm!.Verses)
                            .ThenInclude(v => v.TokenVerseAssociations)
                    .Include(v => v.TokenVerseAssociations)
                    .Where(v => v.VerseMapping!.ParallelCorpusId == request.ParallelCorpusId.Id)
                    .Where(v => 
                         v.TokenVerseAssociations.Any(tva => tokenIdToBCV.ContainsKey(tva.TokenComponentId)) ||
                        (v.BookNumber == firstBCV.B && v.ChapterNumber == firstBCV.C && v.VerseNumber == firstBCV.V))
                    .ToList()
                    .GroupBy(v => v.VerseMapping)
                    .Select(g => g.Key);

                var candidateVerseMappingTvas = candidateVerseMappingsDb
                    .Where(vm => vm!.Verses
                        .Any(v => v.TokenVerseAssociations
                            .Any(tva => compositeCandidatesDb.ContainsKey(tva.TokenComponentId))));

                if (candidateVerseMappingTvas.Count() > 1)
                {
                    return new RequestResult<Unit>
                    (
                        success: false,
                        message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains tokens more than one VerseMapping TokenVerseAssociation"
                    );
                }

                if (candidateVerseMappingTvas.Any())
                {
                    // If there is a VerseMapping from the database that has a TokenVerseAssociation
                    // pointing to one of the CompositeToken children, that's the only VerseMapping
                    // candidate left:
                    candidateVerseMappingsDb = candidateVerseMappingTvas;
                }

                foreach (var verseMappingCandidateDb in candidateVerseMappingsDb)
                {
                    var tokensInVerseMapCount = 0;
                    foreach (var compositeCandidateDbId in compositeCandidatesDb.Keys)
                    {
                        if (verseMappingCandidateDb!.Verses
                            .Any(v => v.TokenVerseAssociations
                                .Any(tva => compositeCandidateDbId == tva.TokenComponentId)))
                        {
                            tokensInVerseMapCount++;
                            continue;
                        }

                        var bcv = tokenIdToBCV[compositeCandidateDbId];
                        if (verseMappingCandidateDb!.Verses
                            .Where(v => v.BookNumber == bcv.B && v.ChapterNumber == bcv.C && v.VerseNumber == bcv.V)
                            .Any())
                        {
                            tokensInVerseMapCount++;
                            continue;
                        }
                    }

                    if (tokensInVerseMapCount != compositeCandidatesDb.Count)
                    {
                        return new RequestResult<Unit>
                        (
                            success: false,
                            message: $"CompositeToken '{request.CompositeToken.TokenId}' found in request contains tokens from multiple VerseMappings"
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

            // Add or update:
            if (existingTokenComposite is not null)
            {
                var childrenGuidsToAdd = request.CompositeToken.Tokens
                    .Select(t => t.TokenId.Id)
                    .Except(existingTokenComposite.Tokens.Select(t => t.Id));
                
                foreach (var guid in childrenGuidsToAdd)
                {
                    existingTokenComposite.Tokens.Add(compositeCandidatesDb[guid]);
                }

                var childrenGuidsToRemove = existingTokenComposite.Tokens
                    .Select(t => t.Id)
                    .Except(request.CompositeToken.Tokens.Select(t => t.TokenId.Id));

                foreach (var guid in childrenGuidsToRemove)
                {
                    var toRemove = existingTokenComposite.Tokens.Where(t => t.Id == guid).First();
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

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}