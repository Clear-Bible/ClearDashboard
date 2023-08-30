using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Migrations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Linq;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class SplitTokensCommandHandler : ProjectDbContextCommandHandler<SplitTokensCommand,
        RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>, (IEnumerable<CompositeToken>, IEnumerable<Token>)>
    {
        private readonly IMediator _mediator;

        public SplitTokensCommandHandler(
            IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<SplitTokensCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>> SaveDataAsync(SplitTokensCommand request,
            CancellationToken cancellationToken)
        {
            if (request.TrainingText3 is null && request.SurfaceTextIndex > 0)
            {
                return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
                (
                    success: false,
                    message: $"Request '{nameof(request.TrainingText3)}' can only be null if '{nameof(request.SurfaceTextIndex)}' == 0 (incoming value: [{request.SurfaceTextIndex}])"
                );
            }

            if (!request.TokenIdsWithSameSurfaceText.Any())
            {
                return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
                (
                    success: false,
                    message: $"Request '{nameof(request.TokenIdsWithSameSurfaceText)}' was empty"
                );
            }

            var requestTokenIdGuids = request.TokenIdsWithSameSurfaceText.Select(e => e.Id);

            var tokensDb = ProjectDbContext.Tokens
                .Include(t => t.TokenCompositeTokenAssociations)
                    .ThenInclude(ta => ta.TokenComposite)
                        .ThenInclude(tc => tc!.Tokens)
                .Include(t => t.TokenizedCorpus)
                    .ThenInclude(tc => tc!.SourceParallelCorpora)
                .Include(t => t.TokenizedCorpus)
                    .ThenInclude(tc => tc!.TargetParallelCorpora)
                .Include(t => t.Translations.Where(a => a.Deleted == null))
                .Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
                .Where(t => requestTokenIdGuids.Contains(t.Id))
                .ToList();

            var noteAssociationsDb = ProjectDbContext.NoteDomainEntityAssociations
                .Where(dea => dea.DomainEntityIdGuid != null)
                .Where(dea => requestTokenIdGuids.Contains((Guid)dea.DomainEntityIdGuid!))
                .ToList();

            var missingTokenIdGuids = requestTokenIdGuids.Except(tokensDb.Select(e => e.Id));
            if (missingTokenIdGuids.Any())
            {
                return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
                (
                    success: false,
                    message: $"Request TokenId(s) '{string.Join(",", missingTokenIdGuids)}' not found in database"
                );
            }

            var surfaceText = tokensDb.First().SurfaceText;
            if (string.IsNullOrEmpty(surfaceText))
            {
                return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
                (
                    success: false,
                    message: $"First matching Token in database has SurfaceText that is null or empty (TokenId: '{tokensDb.First().Id}')"
                );
            }

            if (!tokensDb.Select(e => e.SurfaceText).All(e => e == surfaceText))
            {
                return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
                (
                    success: false,
                    message: $"Tokens in database matching request TokenIds do not all having SurfaceText matching: '{surfaceText}'"
                );
            }

            if (request.SurfaceTextIndex < 0 ||
                request.SurfaceTextLength <= 0 ||
                (request.SurfaceTextIndex + request.SurfaceTextLength) >= surfaceText.Length)
            {
                return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
                (
                    success: false,
                    message: $"Request SurfaceTextIndex [{request.SurfaceTextIndex}] and/or SurfaceTextLength [{request.SurfaceTextLength}] is out of range for SurfaceText '{surfaceText}'"
                );
            }

            var newlyCreatedComposites = new List<CompositeToken>();
            var newlyCreatedTokens = new List<Token>();
            var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

            var idx = 0;
            var childTextPairs = new (
                string surfaceText,
                string trainingText)[(request.SurfaceTextIndex > 0) ? 3 : 2];

            if (request.SurfaceTextIndex > 0)
            {
                childTextPairs[idx++] = (
                    surfaceText: surfaceText[0..request.SurfaceTextIndex],
                    trainingText: request.TrainingText1);
            }
            childTextPairs[idx++] = (
                surfaceText: surfaceText[request.SurfaceTextIndex..(request.SurfaceTextIndex + request.SurfaceTextLength)],
                trainingText: (request.SurfaceTextIndex > 0) ? request.TrainingText2 : request.TrainingText1);
            childTextPairs[idx++] = (
                surfaceText: surfaceText[(request.SurfaceTextIndex + request.SurfaceTextLength)..^0],
                trainingText: (request.SurfaceTextIndex > 0) ? request.TrainingText3! : request.TrainingText2);

            foreach (var tokenDb in tokensDb)
            {
                var newChildTokens = new List<Token>();
                var newChildTokensDb = new List<Models.Token>();
                var nextSubwordNumber = 0;

                var tokenDbNoteAssociations = noteAssociationsDb
                    .Where(e => e.DomainEntityIdGuid == tokenDb.Id)
                    .ToList();

                for (int i = 0; i < childTextPairs.Length; i++)
                {
                    nextSubwordNumber = tokenDb.SubwordNumber + i;

                    var newToken = new Token(
                        new TokenId(
                            tokenDb.BookNumber,
                            tokenDb.ChapterNumber,
                            tokenDb.VerseNumber,
                            tokenDb.WordNumber,
                            nextSubwordNumber
                        )
                        {
                            Id = Guid.NewGuid()
                        },
                        childTextPairs[i].surfaceText,
                        childTextPairs[i].trainingText)
                    {
                        ExtendedProperties = tokenDb.ExtendedProperties
                    };

                    newChildTokens.Add(newToken);
                    newChildTokensDb.Add(new Models.Token
                    {
                        Id = newToken.TokenId.Id,
                        VerseRowId = tokenDb.VerseRowId,
                        TokenizedCorpusId = tokenDb.TokenizedCorpusId,
                        EngineTokenId = newToken.TokenId.ToString(),
                        BookNumber = newToken.TokenId.BookNumber,
                        ChapterNumber = newToken.TokenId.ChapterNumber,
                        VerseNumber = newToken.TokenId.VerseNumber,
                        WordNumber = newToken.TokenId.WordNumber,
                        SubwordNumber = newToken.TokenId.SubWordNumber,
                        SurfaceText = childTextPairs[i].surfaceText,
                        TrainingText = childTextPairs[i].trainingText,
                        ExtendedProperties = tokenDb.ExtendedProperties
                    });

                    if (i == 0)
                    {
                        tokenDbNoteAssociations.ForEach(e => e.DomainEntityIdGuid = newToken.TokenId.Id);
                    }
                    else
                    {
                        tokenDbNoteAssociations.ForEach(e =>
                        {
                            ProjectDbContext.NoteDomainEntityAssociations.Add(new Models.NoteDomainEntityAssociation
                            {
                                NoteId = e.NoteId,
                                DomainEntityIdName = e.DomainEntityIdName,
                                DomainEntityIdGuid = newToken.TokenId.Id
                            });
                        });
                    }
                }

                ProjectDbContext.TokenComponents.AddRange(newChildTokensDb);
                tokenDb.Deleted = currentDateTime;

                if (tokenDb.TokenComposites.Any())
                {
                    // For each existing Composite containing T1, replace T1 with
                    // the newly created tokens as children of the composite.

                    var tokenCompositesDb = tokenDb.TokenComposites;

                    // Remove assocations from all composites that reference this tokenDb.Id
                    ProjectDbContext.TokenCompositeTokenAssociations.RemoveRange(tokenCompositesDb
                        .SelectMany(e => e.TokenCompositeTokenAssociations
                            .Where(t => t.TokenId == tokenDb.Id)));

                    // Add new associations from same composites to newChildTokens
                    ProjectDbContext.TokenCompositeTokenAssociations.AddRange(tokenCompositesDb
                        .SelectMany(e => newChildTokens.Select(t => new Models.TokenCompositeTokenAssociation
                        {
                            Id = Guid.NewGuid(),
                            TokenCompositeId = e.Id,
                            TokenId = t.TokenId.Id
                        })));

                    newlyCreatedTokens.AddRange(newChildTokens);
                }
                else
                {
                    var newCompositeIds = new List<Guid>();

                    if (!request.CreateParallelComposite)
                    {
                        // If (createParallelComposite == false and T1 is not a member of any composite at all),
                        // create a non parallel composite C(parallel=null) with the newly created tokens and
                        // change all (i) alignments, (ii) translations, (iii) notes, and (iv)
                        // TokenVerseAssoc(source or target) that reference T1 to reference C(parallel=null) instead.

                        var composite = new Models.TokenComposite
                        {
                            Id = Guid.NewGuid(),
                            Tokens = newChildTokensDb
                        };

                        ProjectDbContext.TokenComposites.Add(composite);
                        newCompositeIds.Add(composite.Id);

                        tokenDb.SourceAlignments.ForEach(e => e.SourceTokenComponentId = composite.Id);
                        tokenDb.TargetAlignments.ForEach(e => e.TargetTokenComponentId = composite.Id);
                        tokenDb.Translations.ForEach(e => e.SourceTokenComponentId = composite.Id);
                        tokenDb.TokenVerseAssociations.ForEach(e => e.TokenComponentId = composite.Id);
                    }
                    else
                    {
                        // else, for each parallel, if T1 is not a member of either a parallel or non-parallel
                        // composite, create a parallel composite C(parallel) with the newly created tokens and
                        // change all (i) alignments, (ii) translations, (iii) notes, and (iv)
                        // TokenVerseAssoc(source or target) that reference T1 to reference C(parallel) instead.

                        bool isFirst = true;
                        foreach (var pc in tokenDb.TokenizedCorpus!.SourceParallelCorpora.Union(tokenDb.TokenizedCorpus!.TargetParallelCorpora))
                        {
                            var composite = new Models.TokenComposite
                            {
                                Id = Guid.NewGuid(),
                                Tokens = newChildTokensDb,
                                ParallelCorpusId = pc.Id
                            };

                            ProjectDbContext.TokenComposites.Add(composite);
                            newCompositeIds.Add(composite.Id);

                            if (isFirst)
                            {
                                tokenDb.SourceAlignments.ForEach(e => e.SourceTokenComponentId = composite.Id);
                                tokenDb.TargetAlignments.ForEach(e => e.TargetTokenComponentId = composite.Id);
                                tokenDb.Translations.ForEach(e => e.SourceTokenComponentId = composite.Id);
                                tokenDb.TokenVerseAssociations.ForEach(e => e.TokenComponentId = composite.Id);
                            }
                            else
                            {
                                tokenDb.SourceAlignments.ForEach(e =>
                                {
                                    ProjectDbContext.Alignments.Add(new Models.Alignment
                                    {
                                        SourceTokenComponentId = composite.Id,
                                        TargetTokenComponentId = e.TargetTokenComponentId,
                                        AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
                                        AlignmentVerification = e.AlignmentVerification
                                    });
                                });
                                tokenDb.TargetAlignments.ForEach(e =>
                                {
                                    ProjectDbContext.Alignments.Add(new Models.Alignment
                                    {
                                        SourceTokenComponentId = e.SourceTokenComponentId,
                                        TargetTokenComponentId = composite.Id,
                                        AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
                                        AlignmentVerification = e.AlignmentVerification
                                    });
                                });
                                tokenDb.Translations.ForEach(e =>
                                {
                                    ProjectDbContext.Translations.Add(new Models.Translation
                                    {
                                        SourceTokenComponentId = composite.Id,
                                        TargetText = e.TargetText,
                                        TranslationState = e.TranslationState,
                                        LexiconTranslationId = e.LexiconTranslationId
                                    });
                                });
                                tokenDb.TokenVerseAssociations.ForEach(e =>
                                {
                                    ProjectDbContext.TokenVerseAssociations.Add(new Models.TokenVerseAssociation
                                    {
                                        TokenComponentId = composite.Id,
                                        Position = e.Position,
                                        VerseId = e.VerseId
                                    });
                                });
                            }

                            isFirst = false;
                        }
                    }
                }

                // Find any tokens with a BCVW that matches the current tokenDb and
                // with a SubwordNumber that is greater than the last new split token,
                // and do a Subword renumbering
                nextSubwordNumber++;
                var tokensHavingSubwordsToRenumberDb = ProjectDbContext.Tokens
                    .Include(t => t.TokenCompositeTokenAssociations)
                        .ThenInclude(ta => ta.TokenComposite)
                    .Where(t =>
                        t.BookNumber == tokenDb.BookNumber &&
                        t.ChapterNumber == tokenDb.ChapterNumber &&
                        t.VerseNumber == tokenDb.VerseNumber &&
                        t.WordNumber == tokenDb.WordNumber &&
                        t.SubwordNumber > tokenDb.SubwordNumber)
                    .ToArray();

                for (int i = 0; i < tokensHavingSubwordsToRenumberDb.Length; i++)
                {
                    tokensHavingSubwordsToRenumberDb[i].SubwordNumber = nextSubwordNumber + i;
                }
            }

            //using (var transaction = ProjectDbContext.Database.BeginTransaction())
            //{
            //    alignmentSet.AddDomainEvent(new AlignmentSetSourceTokenIdsUpdatingEvent(request.AlignmentSetId.Id, sourceTokenIdsForDenormalization, ProjectDbContext));
            //    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            //    transaction.Commit();
            //}

            //await _mediator.Publish(new AlignmentSetSourceTokenIdsUpdatedEvent(request.AlignmentSetId.Id, sourceTokenIdsForDenormalization), cancellationToken);


            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<(IEnumerable<CompositeToken>, IEnumerable<Token>)>
            (
                (newlyCreatedComposites, newlyCreatedTokens)
            );
        }
    }
}