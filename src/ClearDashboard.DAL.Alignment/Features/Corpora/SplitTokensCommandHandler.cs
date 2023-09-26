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

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class SplitTokensCommandHandler : ProjectDbContextCommandHandler<SplitTokensCommand,
        RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>, 
        (IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
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

        protected override async Task<RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>> SaveDataAsync(
            SplitTokensCommand request,
            CancellationToken cancellationToken)
        {
            if (request.TrainingText3 is null && request.SurfaceTextIndex > 0)
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request '{nameof(request.TrainingText3)}' can only be null if '{nameof(request.SurfaceTextIndex)}' == 0 (incoming value: [{request.SurfaceTextIndex}])"
                );
            }

            if (!request.TokenIdsWithSameSurfaceText.Any())
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request '{nameof(request.TokenIdsWithSameSurfaceText)}' was empty"
                );
            }

            var requestTokenIdGuids = request.TokenIdsWithSameSurfaceText.Select(e => e.Id);

            var tokensDb = await ProjectDbContext.Tokens
                .Include(t => t.TokenCompositeTokenAssociations)
                .Include(t => t.TokenizedCorpus)
                    .ThenInclude(tc => tc!.SourceParallelCorpora)
                .Include(t => t.TokenizedCorpus)
                    .ThenInclude(tc => tc!.TargetParallelCorpora)
                .Include(t => t.Translations.Where(a => a.Deleted == null))
                .Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
                .Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
                .Where(t => t.TokenizedCorpusId == request.TokenizedTextCorpusId.Id)
                .Where(t => requestTokenIdGuids.Contains(t.Id))
                .ToListAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            var noteAssociationsDb = ProjectDbContext.NoteDomainEntityAssociations
                .Where(dea => dea.DomainEntityIdGuid != null)
                .Where(dea => requestTokenIdGuids.Contains((Guid)dea.DomainEntityIdGuid!))
                .ToList();

            var missingTokenIdGuids = requestTokenIdGuids.Except(tokensDb.Select(e => e.Id));
            if (missingTokenIdGuids.Any())
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request TokenId(s) '{string.Join(",", missingTokenIdGuids)}' not found in database"
                );
            }

            var surfaceText = tokensDb.First().SurfaceText;
            if (string.IsNullOrEmpty(surfaceText))
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"First matching Token in database has SurfaceText that is null or empty (TokenId: '{tokensDb.First().Id}')"
                );
            }

            if (!tokensDb.Select(e => e.SurfaceText).All(e => e == surfaceText))
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Tokens in database matching request TokenIds do not all having SurfaceText matching: '{surfaceText}'"
                );
            }

            if (request.SurfaceTextIndex < 0 ||
                request.SurfaceTextLength <= 0 ||
                (request.SurfaceTextIndex + request.SurfaceTextLength) >= surfaceText.Length)
            {
                return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: $"Request SurfaceTextIndex [{request.SurfaceTextIndex}] and/or SurfaceTextLength [{request.SurfaceTextLength}] is out of range for SurfaceText '{surfaceText}'"
                );
            }

            var splitCompositeTokensByIncomingTokenId = new Dictionary<TokenId, IEnumerable<CompositeToken>>();
            var splitChildTokensByIncomingTokenId = new Dictionary<TokenId, IEnumerable<Token>>();
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

            var replacementTokensById = new Dictionary<Guid, List<Token>>();
            var replacementModelTokensById = new Dictionary<Guid, List<Models.Token>>();
            var sourceTrainingTextsByAlignmentSetId = new Dictionary<Guid, List<string>>();

            foreach (var tokenDb in tokensDb)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var newChildTokens = new List<Token>();
                var newChildTokensDb = new List<Models.Token>();
                var nextSubwordNumber = 0;

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
                    newChildTokensDb.Add(BuildModelToken(newToken, tokenDb.TokenizedCorpusId, tokenDb.VerseRowId));

                    var tokenDbNoteAssociations = noteAssociationsDb
                        .Where(e => e.DomainEntityIdGuid == tokenDb.Id)
                        .ToList();

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

                replacementTokensById.Add(tokenDb.Id, newChildTokens);
                replacementModelTokensById.Add(tokenDb.Id, newChildTokensDb);

                // Put the new child tokens into the "splitChild' return value structure:
                splitChildTokensByIncomingTokenId.Add(ModelHelper.BuildTokenId(tokenDb), newChildTokens);

                ProjectDbContext.TokenComponents.AddRange(newChildTokensDb);
                tokenDb.Deleted = currentDateTime;

                // Find any tokens with a BCVW that matches the current tokenDb and
                // with a SubwordNumber that is greater than the last new split token,
                // and do a Subword renumbering
                nextSubwordNumber++;
                var tokensHavingSubwordsToRenumberDb = ProjectDbContext.Tokens
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

            var existingCompositeIds = tokensDb
                .SelectMany(e => e.TokenCompositeTokenAssociations
                .Select(t => t.TokenCompositeId))
                .Distinct();

            var tokensDbTokenComposites = ProjectDbContext.TokenComposites
                .Include(e => e.Tokens)
                .Include(e => e.SourceAlignments)
                .Where(e => existingCompositeIds.Contains(e.Id))
                .ToList();

            // Replace tokens with new split tokens in every composite they
            // are found in:
            var incomingTokenIdCompositePairs = new List<(Guid, CompositeToken)>();
            foreach (var tokenComposite in tokensDbTokenComposites)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var previousTrainingText = tokenComposite.TrainingText;

                var tokenIdsToReplace = tokenComposite.Tokens
                    .Select(e => e.Id)
                    .Intersect(tokensDb.Select(e => e.Id))
                    .ToList();

                // Remove assocations from this composite that reference tokenIdsToReplace
                ProjectDbContext.TokenCompositeTokenAssociations.RemoveRange(tokensDb
                    .SelectMany(e => e.TokenCompositeTokenAssociations
                        .Where(t =>
                            t.TokenCompositeId == tokenComposite.Id &&
                            tokenIdsToReplace.Contains(t.TokenId)
                        )));

                // Get existing child tokens for the composites, leaving out the ones
                // we are going to replace with split token children:
                var compositeToken = ModelHelper.BuildCompositeToken(tokenComposite);
                var compositeChildTokens = compositeToken.Tokens
                    .ExceptBy(tokenIdsToReplace, e => e.TokenId.Id)
                    .ToList();

                foreach (var tokenIdToReplace in tokenIdsToReplace)
                {
                    compositeChildTokens.AddRange(replacementTokensById[tokenIdToReplace]);

                    // Add new associations from this composites to replacement child token ids:
                    ProjectDbContext.TokenCompositeTokenAssociations.AddRange(replacementTokensById[tokenIdToReplace]
                        .Select(e => new Models.TokenCompositeTokenAssociation
                        {
                            Id = Guid.NewGuid(),
                            TokenCompositeId = tokenComposite.Id,
                            TokenId = e.TokenId.Id
                        }));

                    incomingTokenIdCompositePairs.Add((tokenIdToReplace, compositeToken));
                }

                // Using the higher level CompositeToken structure here (instead 
                // of Models.TokenComposite) should reset the Surface and Training
                // text using the new split child tokens:
                compositeToken.Tokens = compositeChildTokens;

                tokenComposite.SurfaceText = compositeToken.SurfaceText;
                tokenComposite.TrainingText = compositeToken.TrainingText;

                foreach (var e in tokenComposite.SourceAlignments)
                {
                    if (sourceTrainingTextsByAlignmentSetId.TryGetValue(e.AlignmentSetId, out var sourceTrainingTexts))
                    {
                        sourceTrainingTexts.Add(previousTrainingText!);
                        sourceTrainingTexts.Add(tokenComposite.TrainingText!);
                    }
                    else
                    {
                        sourceTrainingTextsByAlignmentSetId.Add(e.AlignmentSetId, new List<string>
                        {
                            previousTrainingText!,
                            tokenComposite.TrainingText!
                        });
                    }
                }
            }

            foreach (var tokenDb in tokensDb.Where(e => !e.TokenCompositeTokenAssociations.Any()))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!request.CreateParallelComposite)
                {
                    // If (createParallelComposite == false and T1 is not a member of any composite at all),
                    // create a non parallel composite C(parallel=null) with the newly created tokens and
                    // change all (i) alignments, (ii) translations, (iii) notes, and (iv)
                    // TokenVerseAssoc(source or target) that reference T1 to reference C(parallel=null) instead.

                    var compositeToken = new CompositeToken(replacementTokensById[tokenDb.Id]);
                    compositeToken.TokenId.Id = Guid.NewGuid();

                    incomingTokenIdCompositePairs.Add((tokenDb.Id, compositeToken));

                    var tokenComposite = BuildModelTokenComposite(
                        compositeToken,
                        tokenDb.TokenizedCorpusId,
                        tokenDb.VerseRowId, 
                        replacementModelTokensById[tokenDb.Id]);

                    ProjectDbContext.TokenComposites.Add(tokenComposite);

                    foreach (var e in tokenDb.SourceAlignments) 
                    { 
                        e.SourceTokenComponentId = tokenComposite.Id;
                        if (sourceTrainingTextsByAlignmentSetId.TryGetValue(e.AlignmentSetId, out var sourceTrainingTexts))
                        {
                            sourceTrainingTexts.Add(tokenDb.TrainingText!);
                            sourceTrainingTexts.Add(tokenComposite.TrainingText!);
                        }
                        else
                        {
                            sourceTrainingTextsByAlignmentSetId.Add(e.AlignmentSetId, new List<string> 
                            { 
                                tokenDb.TrainingText!,
                                tokenComposite.TrainingText! 
                            });
                        }
                    }
                    foreach (var e in tokenDb.TargetAlignments) { e.TargetTokenComponentId = tokenComposite.Id; }
                    foreach (var e in tokenDb.Translations) { e.SourceTokenComponentId = tokenComposite.Id; }
                    foreach (var e in tokenDb.TokenVerseAssociations) { e.TokenComponentId = tokenComposite.Id; }
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
                        var compositeToken = new CompositeToken(replacementTokensById[tokenDb.Id]);
                        compositeToken.TokenId.Id = Guid.NewGuid();

                        var tokenComposite = BuildModelTokenComposite(
                            compositeToken,
                            tokenDb.TokenizedCorpusId,
                            tokenDb.VerseRowId,
                            replacementModelTokensById[tokenDb.Id]);
                        tokenComposite.ParallelCorpusId = pc.Id;

                        ProjectDbContext.TokenComposites.Add(tokenComposite);

                        incomingTokenIdCompositePairs.Add((tokenDb.Id, compositeToken));

                        if (isFirst)
                        {
                            foreach (var e in tokenDb.SourceAlignments)
                            {
                                e.SourceTokenComponentId = tokenComposite.Id;
                                if (sourceTrainingTextsByAlignmentSetId.TryGetValue(e.AlignmentSetId, out var sourceTrainingTexts))
                                {
                                    sourceTrainingTexts.Add(tokenDb.TrainingText!);
                                    sourceTrainingTexts.Add(tokenComposite.TrainingText!);
                                }
                                else
                                {
                                    sourceTrainingTextsByAlignmentSetId.Add(e.AlignmentSetId, new List<string> 
                                    {
                                        tokenDb.TrainingText!,
                                        tokenComposite.TrainingText! 
                                    });
                                }
                            }
                            foreach (var e in tokenDb.TargetAlignments) { e.TargetTokenComponentId = tokenComposite.Id; }
                            foreach (var e in tokenDb.Translations) { e.SourceTokenComponentId = tokenComposite.Id; }
                            foreach (var e in tokenDb.TokenVerseAssociations) { e.TokenComponentId = tokenComposite.Id; }
                        }
                        else
                        {
                            foreach (var e in tokenDb.SourceAlignments)
                            {
                                ProjectDbContext.Alignments.Add(new Models.Alignment
                                {
                                    AlignmentSetId = e.AlignmentSetId,
                                    SourceTokenComponentId = tokenComposite.Id,
                                    TargetTokenComponentId = e.TargetTokenComponentId,
                                    AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
                                    AlignmentVerification = e.AlignmentVerification,
                                    Score = e.Score
                                });
                                if (sourceTrainingTextsByAlignmentSetId.TryGetValue(e.AlignmentSetId, out var sourceTrainingTexts))
                                {
                                    sourceTrainingTexts.Add(tokenComposite.TrainingText!);
                                }
                                else
                                {
                                    sourceTrainingTextsByAlignmentSetId.Add(e.AlignmentSetId, new List<string> { tokenComposite.TrainingText! });
                                }
                            }
                            foreach (var e in tokenDb.TargetAlignments)
                            {
                                ProjectDbContext.Alignments.Add(new Models.Alignment
                                {
                                    AlignmentSetId = e.AlignmentSetId,
                                    SourceTokenComponentId = e.SourceTokenComponentId,
                                    TargetTokenComponentId = tokenComposite.Id,
                                    AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
                                    AlignmentVerification = e.AlignmentVerification,
                                    Score = e.Score
                                });
                            }
                            foreach (var e in tokenDb.Translations)
                            {
                                ProjectDbContext.Translations.Add(new Models.Translation
                                {
                                    TranslationSetId = e.TranslationSetId,
                                    SourceTokenComponentId = tokenComposite.Id,
                                    TargetText = e.TargetText,
                                    TranslationState = e.TranslationState,
                                    LexiconTranslationId = e.LexiconTranslationId,
                                    Modified = e.Modified
                                });
                            }
                            foreach (var e in tokenDb.TokenVerseAssociations)
                            {
                                ProjectDbContext.TokenVerseAssociations.Add(new Models.TokenVerseAssociation
                                {
                                    TokenComponentId = tokenComposite.Id,
                                    Position = e.Position,
                                    VerseId = e.VerseId
                                });
                            }
                        }

                        isFirst = false;
                    }
                }
            }

            var compositesByIncomingTokenId = incomingTokenIdCompositePairs
                .GroupBy(e => e.Item1)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Item2));

            tokensDb.ForEach(e =>
            {
                if (compositesByIncomingTokenId.TryGetValue(e.Id, out var composites))
                {
                    splitCompositeTokensByIncomingTokenId.Add(ModelHelper.BuildTokenId(e), composites);
                }
            });

            if (sourceTrainingTextsByAlignmentSetId.Any())
            {
                using (var transaction = ProjectDbContext.Database.BeginTransaction())
                {
                    await _mediator.Publish(new AlignmentSetSourceTrainingTextsUpdatingEvent(sourceTrainingTextsByAlignmentSetId, ProjectDbContext));
                    _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);
                }

                await _mediator.Publish(new AlignmentSetSourceTrainingTextsUpdatedEvent(sourceTrainingTextsByAlignmentSetId), cancellationToken);
            }
            else
            {
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            }

            return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
            (
                (splitCompositeTokensByIncomingTokenId, splitChildTokensByIncomingTokenId)
            );
        }

        private static Models.TokenComposite BuildModelTokenComposite(CompositeToken compositeToken, Guid tokenizedCorpusId, Guid? verseRowId, IEnumerable<Models.Token>? modelTokens = null)
        {
            modelTokens ??= compositeToken.Tokens.Union(compositeToken.OtherTokens)
                .Select(e => BuildModelToken(e, tokenizedCorpusId, verseRowId));

            var tokenComposite = new Models.TokenComposite
            {
                Id = compositeToken.TokenId.Id,
                Tokens = modelTokens.ToList()
            };

            tokenComposite.SurfaceText = compositeToken.SurfaceText;
            tokenComposite.TrainingText = compositeToken.TrainingText;
        
            tokenComposite.EngineTokenId = compositeToken.TokenId.ToString();
            tokenComposite.TokenizedCorpusId = tokenizedCorpusId;
            tokenComposite.ParallelCorpusId = null;
            tokenComposite.ExtendedProperties = null;
            tokenComposite.Deleted = null;

            if (modelTokens.GroupBy(e => e.VerseRowId).Count() == 1)
            {
                tokenComposite.VerseRowId = verseRowId;
            }
            else
            {
                tokenComposite.VerseRowId = null;
            }

            return tokenComposite;
        }

        private static Models.Token BuildModelToken(Token token, Guid tokenizedCorpusId, Guid? verseRowId)
        {
            return new Models.Token
            {
                Id = token.TokenId.Id,
                TokenizedCorpusId = tokenizedCorpusId,
                VerseRowId = verseRowId,
                EngineTokenId = token.TokenId.ToString(),
                BookNumber = token.TokenId.BookNumber,
                ChapterNumber = token.TokenId.ChapterNumber,
                VerseNumber = token.TokenId.VerseNumber,
                WordNumber = token.TokenId.WordNumber,
                SubwordNumber = token.TokenId.SubWordNumber,
                SurfaceText = token.SurfaceText,
                TrainingText = token.TrainingText,
                ExtendedProperties = token.ExtendedProperties
            };
        }
    }
}