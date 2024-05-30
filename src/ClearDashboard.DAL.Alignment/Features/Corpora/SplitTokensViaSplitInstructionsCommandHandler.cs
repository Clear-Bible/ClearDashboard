using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Token = ClearBible.Engine.Corpora.Token;

namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public class SplitTokensViaSplitInstructionsCommandHandler : ProjectDbContextCommandHandler<SplitTokensViaSplitInstructionsCommand,
    RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>,
    (IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
{
    private readonly IMediator _mediator;

    public SplitTokensViaSplitInstructionsCommandHandler(
        IMediator mediator,
        ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
        ILogger<SplitTokensCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
        logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>> SaveDataAsync(
        SplitTokensViaSplitInstructionsCommand request,
        CancellationToken cancellationToken)
    {
        
        if (!request.SplitInstructions.Validate())
        {
            return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
                (
                    success: false,
                    message: request.SplitInstructions.ErrorMessage
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

        var tokensDbQueryable = ProjectDbContext.Tokens
            .Include(t => t.TokenCompositeTokenAssociations)
            .Include(t => t.TokenizedCorpus)
            .ThenInclude(tc => tc!.SourceParallelCorpora)
            .Include(t => t.TokenizedCorpus)
            .ThenInclude(tc => tc!.TargetParallelCorpora)
            .Include(t => t.Translations.Where(a => a.Deleted == null))
            .Include(t => t.SourceAlignments.Where(a => a.Deleted == null))
            .Include(t => t.TargetAlignments.Where(a => a.Deleted == null))
            .Include(t => t.TokenVerseAssociations.Where(a => a.Deleted == null))
            .Where(t => t.TokenizedCorpusId == request.TokenizedTextCorpusId.Id);

        var tokensDb = await tokensDbQueryable
            .Where(t => requestTokenIdGuids.Contains(t.Id))
            .ToListAsync(cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var missingTokenIdGuids = requestTokenIdGuids.Except(tokensDb.Select(e => e.Id));
        if (missingTokenIdGuids.Any())
        {
            return new RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
            (
                success: false,
                message: $"Request TokenId(s) '{string.Join(",", missingTokenIdGuids)}' not found in database as part of TokenizedCorpusId '{request.TokenizedTextCorpusId.Id}'"
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

        if (tokensDb.Count == 1 && request.PropagateTo != SplitTokenPropagationScope.None)
        {
            var tokenToPropagate = tokensDb.First();

            var tokensDbPropagateQueryable = tokensDbQueryable
                .Where(t => t.SurfaceText == tokenToPropagate.SurfaceText)
                .Where(t => t.Id != tokenToPropagate.Id);

            if (request.PropagateTo == SplitTokenPropagationScope.Book)
            {
                tokensDbPropagateQueryable = tokensDbPropagateQueryable
                    .Where(t => t.BookNumber == tokenToPropagate.BookNumber);
            }

            if (request.PropagateTo == SplitTokenPropagationScope.BookChapter)
            {
                tokensDbPropagateQueryable = tokensDbPropagateQueryable
                    .Where(t => t.BookNumber == tokenToPropagate.BookNumber)
                    .Where(t => t.ChapterNumber == tokenToPropagate.ChapterNumber);
            }

            if (request.PropagateTo == SplitTokenPropagationScope.BookChapterVerse)
            {
                tokensDbPropagateQueryable = tokensDbPropagateQueryable
                    .Where(t => t.BookNumber == tokenToPropagate.BookNumber)
                    .Where(t => t.ChapterNumber == tokenToPropagate.ChapterNumber)
                    .Where(t => t.VerseNumber == tokenToPropagate.VerseNumber);
            }

            tokensDb.AddRange(await tokensDbPropagateQueryable.ToListAsync(cancellationToken));
        }

		var replacementTokenInfos = new (
			string surfaceText,
			string trainingText,
			string? tokenType)[request.SplitInstructions.Count];

		for (var i = 0; i < request.SplitInstructions.Count; i++)
		{
			var splitInstruction = request.SplitInstructions[i];
			replacementTokenInfos[i] = (
				surfaceText: splitInstruction.TokenText,
				// NB:  What to do here when TrainingText is null?  Ask Dirk.  Should this be set to the Token 'surface text'
				trainingText: splitInstruction.TrainingText, // ?? string.Empty,
				// TODO:  Get TokenType from SplitInstructions
				tokenType: null
			)!;
		}

		var (
			    splitCompositeTokensByIncomingTokenId, 
				splitChildTokensByIncomingTokenId, 
				sourceTrainingTextsByAlignmentSetId
			) = SplitTokensIntoReplacements(
			tokensDb,
			replacementTokenInfos,
			request.CreateParallelComposite,
			ProjectDbContext,
			cancellationToken
		);

		// Update alignment denormalization data:
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

	public static (IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>, Dictionary<Guid, List<string>>) SplitTokensIntoReplacements(
		List<DataAccessLayer.Models.Token> tokensDb,
		(string surfaceText, string trainingText, string? tokenType)[] replacementTokenInfos, 
		bool createParallelComposite,
		ProjectDbContext projectDbContext, 
		CancellationToken cancellationToken
	)
	{
		var replacementsBySourceId =
			CreateTokenReplacements(
				tokensDb,
				replacementTokenInfos,
				projectDbContext,
				cancellationToken
			);

		var (incomingTokenIdCompositePairs, sourceTrainingTextsByAlignmentSetId) = AttachToComposites(
			replacementsBySourceId,
			createParallelComposite,
			projectDbContext,
			cancellationToken
		);

		// Generate output variables (split child tokens):
		var splitChildTokensByIncomingTokenId = replacementsBySourceId.ToDictionary(
			 e => ModelHelper.BuildTokenId(e.Value.SourceModelToken),
			 e => e.Value.ReplacementTokens.AsEnumerable());

		// Generate output variables (split composite tokens):
		var splitCompositeTokensByIncomingTokenId = new Dictionary<TokenId, IEnumerable<CompositeToken>>();

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

		return (splitCompositeTokensByIncomingTokenId, splitChildTokensByIncomingTokenId, sourceTrainingTextsByAlignmentSetId);
	}

	private static Dictionary<Guid, (DataAccessLayer.Models.Token SourceModelToken, List<Token> ReplacementTokens, List<DataAccessLayer.Models.Token> ReplacementModelTokens)> CreateTokenReplacements(List<DataAccessLayer.Models.Token> tokensDb, (string surfaceText, string trainingText, string? tokenType)[] replacementTokenInfos, ProjectDbContext projectDbContext, CancellationToken cancellationToken)
    {
		var currentDateTime = DataAccessLayer.Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

        var replacementsBySourceId = 
            new Dictionary<Guid, (
                DataAccessLayer.Models.Token SourceModelToken, 
                List<Token> ReplacementTokens, 
                List<DataAccessLayer.Models.Token> ReplacementModelTokens
            )>();

		var noteAssociationsDb = projectDbContext.NoteDomainEntityAssociations
			.Where(dea => dea.DomainEntityIdGuid != null)
			.Where(dea => tokensDb.Select(e => e.Id).Contains((Guid)dea.DomainEntityIdGuid!))
			.ToList();

		foreach (var tokenDb in tokensDb)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var newChildTokens = new List<Token>();
			var newChildTokensDb = new List<DataAccessLayer.Models.Token>();
			var nextSubwordNumber = 0;

			for (var i = 0; i < replacementTokenInfos.Length; i++)
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
					replacementTokenInfos[i].surfaceText,
					replacementTokenInfos[i].trainingText)
				{
					ExtendedProperties = tokenDb.ExtendedProperties,
				};

				var newModelToken = BuildModelToken(newToken, tokenDb.TokenizedCorpusId, tokenDb.VerseRowId);
				newModelToken.Type = replacementTokenInfos[i].tokenType;
				newModelToken.OriginTokenLocation ??= tokenDb.OriginTokenLocation ?? tokenDb.EngineTokenId;

				newChildTokens.Add(newToken);
				newChildTokensDb.Add(newModelToken);

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
						projectDbContext.NoteDomainEntityAssociations.Add(new DataAccessLayer.Models.NoteDomainEntityAssociation
						{
							NoteId = e.NoteId,
							DomainEntityIdName = e.DomainEntityIdName,
							DomainEntityIdGuid = newToken.TokenId.Id
						});
					});
				}
			}

            replacementsBySourceId.Add(tokenDb.Id, (tokenDb, newChildTokens, newChildTokensDb));

			// Put the new child tokens into the "splitChild' return value structure:
			//splitChildTokensByIncomingTokenId.Add(ModelHelper.BuildTokenId(tokenDb), newChildTokens);

			projectDbContext.TokenComponents.AddRange(newChildTokensDb);
			tokenDb.Deleted = currentDateTime;

			// Find any tokens with a BCVW that matches the current tokenDb and
			// with a SubwordNumber that is greater than the token that was split,
			// and do a Subword renumbering
			nextSubwordNumber++;
			var tokensHavingSubwordsToRenumberDb = projectDbContext.Tokens
				.Where(t =>
					t.BookNumber == tokenDb.BookNumber &&
					t.ChapterNumber == tokenDb.ChapterNumber &&
					t.VerseNumber == tokenDb.VerseNumber &&
					t.WordNumber == tokenDb.WordNumber &&
					t.SubwordNumber > tokenDb.SubwordNumber)
				.ToArray();

			for (int i = 0; i < tokensHavingSubwordsToRenumberDb.Length; i++)
			{
				tokensHavingSubwordsToRenumberDb[i].OriginTokenLocation ??= tokenDb.OriginTokenLocation ?? tokenDb.EngineTokenId;
				tokensHavingSubwordsToRenumberDb[i].SubwordNumber = nextSubwordNumber + i;
			}
		}

        return replacementsBySourceId;
	}

	private static (List<(Guid, CompositeToken)>, Dictionary<Guid, List<string>>) AttachToComposites(
		Dictionary<Guid, (DataAccessLayer.Models.Token SourceModelToken, List<Token> ReplacementTokens, List<DataAccessLayer.Models.Token> ReplacementModelTokens)> replacementsBySourceId,
		bool createParallelComposite,
		ProjectDbContext projectDbContext, 
		CancellationToken cancellationToken)
	{
		var replacementTokensById = new Dictionary<Guid, List<Token>>();
		var replacementModelTokensById = new Dictionary<Guid, List<DataAccessLayer.Models.Token>>();

		foreach (var kvp in replacementsBySourceId)
		{
			replacementTokensById.Add(kvp.Key, kvp.Value.ReplacementTokens);
			replacementModelTokensById.Add(kvp.Key, kvp.Value.ReplacementModelTokens);
		}

		var tokensDb = replacementsBySourceId.Values.Select(e => e.SourceModelToken).ToList();
		var sourceTrainingTextsByAlignmentSetId = new Dictionary<Guid, List<string>>();

		var incomingTokenIdCompositePairs = ReplaceInExistingComposites(
			tokensDb,
			replacementTokensById,
			sourceTrainingTextsByAlignmentSetId,
			projectDbContext,
			cancellationToken
		);

		// For tokens having no pre-existing composite associations, create
		// new composites:
		foreach (var tokenDb in tokensDb.Where(e => !e.TokenCompositeTokenAssociations.Any()))
		{
			cancellationToken.ThrowIfCancellationRequested();

			var tcParallelCorpora = tokenDb.TokenizedCorpus!.SourceParallelCorpora.Union(tokenDb.TokenizedCorpus!.TargetParallelCorpora);

			// If there aren't any parallel corpora associated with the token's tokenized corpus,
			// ignore the "CreateParallelComposite == true" from the request:

			if (!createParallelComposite || !tcParallelCorpora.Any())
			{
				// If (createParallelComposite == false and T1 is not a member of any composite at all),
				// create a non parallel composite C(parallel=null) with the newly created tokens and
				// change all (i) alignments, (ii) translations, (iii) notes, and (iv)
				// TokenVerseAssoc(source or target) that reference T1 to reference C(parallel=null) instead.

				var (tokenId, tokenComposite, compositeToken) = CreateComposite(
					tokenDb,
					null,
					replacementTokensById[tokenDb.Id],
					replacementModelTokensById[tokenDb.Id],
					projectDbContext);

				incomingTokenIdCompositePairs.Add((tokenDb.Id, compositeToken));
				TransferAssociations(tokenDb, tokenComposite, sourceTrainingTextsByAlignmentSetId, projectDbContext, true);
			}
			else
			{
				// else, for each parallel, if T1 is not a member of either a parallel or non-parallel
				// composite, create a parallel composite C(parallel) with the newly created tokens and
				// change all (i) alignments, (ii) translations, (iii) notes, and (iv)
				// TokenVerseAssoc(source or target) that reference T1 to reference C(parallel) instead.

				bool isFirst = true;
				foreach (var pc in tcParallelCorpora)
				{
					var (tokenId, tokenComposite, compositeToken) = CreateComposite(
						tokenDb,
						pc.Id,
						replacementTokensById[tokenDb.Id],
						replacementModelTokensById[tokenDb.Id],
						projectDbContext);

					incomingTokenIdCompositePairs.Add((tokenDb.Id, compositeToken));
					TransferAssociations(tokenDb, tokenComposite, sourceTrainingTextsByAlignmentSetId, projectDbContext, isFirst);

					isFirst = false;
				}
			}
		}

		return (incomingTokenIdCompositePairs, sourceTrainingTextsByAlignmentSetId);
	}

	private static List<(Guid, CompositeToken)> ReplaceInExistingComposites(
        List<DataAccessLayer.Models.Token> tokensDb, 
        Dictionary<Guid, List<Token>> replacementTokensById,
		Dictionary<Guid, List<string>> sourceTrainingTextsByAlignmentSetId,
		ProjectDbContext projectDbContext, 
        CancellationToken cancellationToken)
    {
		var incomingTokenIdCompositePairs = new List<(Guid, CompositeToken)>();

		var existingCompositeIds = tokensDb
	        .SelectMany(e => e.TokenCompositeTokenAssociations
		        .Select(t => t.TokenCompositeId))
	        .Distinct();

		var tokensDbTokenComposites = projectDbContext.TokenComposites
			.Include(e => e.Tokens)
			.Include(e => e.SourceAlignments)
			.Where(e => existingCompositeIds.Contains(e.Id))
			.ToList();

		// Replace tokens with new split tokens in every composite they
		// are found in:
		foreach (var tokenComposite in tokensDbTokenComposites)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var previousTrainingText = tokenComposite.TrainingText;

			var tokenIdsToReplace = tokenComposite.Tokens
				.Select(e => e.Id)
				.Intersect(tokensDb.Select(e => e.Id))
				.ToList();

			// Remove associations from this composite that reference tokenIdsToReplace
			projectDbContext.TokenCompositeTokenAssociations.RemoveRange(tokensDb
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
				projectDbContext.TokenCompositeTokenAssociations.AddRange(replacementTokensById[tokenIdToReplace]
					.Select(e => new DataAccessLayer.Models.TokenCompositeTokenAssociation
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

        return incomingTokenIdCompositePairs;
	}

	private static (Guid, TokenComposite, CompositeToken) CreateComposite(
		DataAccessLayer.Models.Token tokenDb,
		Guid? parallelCorpusId,
		List<Token> replacementTokens,
		List<DataAccessLayer.Models.Token> replacementModelTokens,
		ProjectDbContext projectDbContext)
	{
		var compositeToken = new CompositeToken(replacementTokens)
		{
			TokenId =
			{
				Id = Guid.NewGuid()
			},
		};

		if (parallelCorpusId is not null)
		{
			// TODO808:  Review with Chris
			// Tag the composite token as being a parallel composite token
			compositeToken.Metadata[DataAccessLayer.Models.MetadatumKeys.IsParallelCorpusToken] = true;
		}

		compositeToken.SetSplitSource(tokenDb.Id);
		compositeToken.SetSplitInitialChildren(replacementTokens);

		var tokenComposite = BuildModelTokenComposite(
			compositeToken,
			tokenDb.TokenizedCorpusId,
			tokenDb.VerseRowId,
			replacementModelTokens);
		tokenComposite.ParallelCorpusId = parallelCorpusId;

		projectDbContext.TokenComposites.Add(tokenComposite);

		return (tokenDb.Id, tokenComposite, compositeToken);
	}

	private static void TransferAssociations(
		DataAccessLayer.Models.Token tokenDb,
		DataAccessLayer.Models.TokenComposite tokenComposite,
		Dictionary<Guid, List<string>> sourceTrainingTextsByAlignmentSetId,
		ProjectDbContext projectDbContext,
		bool isFirstForTokenDb)
	{
		if (isFirstForTokenDb)
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
				projectDbContext.Alignments.Add(new DataAccessLayer.Models.Alignment
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
				projectDbContext.Alignments.Add(new DataAccessLayer.Models.Alignment
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
				projectDbContext.Translations.Add(new DataAccessLayer.Models.Translation
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
				projectDbContext.TokenVerseAssociations.Add(new DataAccessLayer.Models.TokenVerseAssociation
				{
					TokenComponentId = tokenComposite.Id,
					Position = e.Position,
					VerseId = e.VerseId
				});
			}
		}
	}

	private static DataAccessLayer.Models.TokenComposite BuildModelTokenComposite(CompositeToken compositeToken, Guid tokenizedCorpusId, Guid? verseRowId, IEnumerable<DataAccessLayer.Models.Token>? modelTokens = null)
    {
        modelTokens ??= compositeToken.Tokens.Union(compositeToken.OtherTokens)
            .Select(e => BuildModelToken(e, tokenizedCorpusId, verseRowId));

        var tokenComposite = new DataAccessLayer.Models.TokenComposite
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

        if (compositeToken.HasMetadatum(MetadatumKeys.ModelTokenMetadata))
        {
            tokenComposite.Metadata = compositeToken.GetMetadatum<List<Metadatum>>(MetadatumKeys.ModelTokenMetadata);
        }

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

    private static DataAccessLayer.Models.Token BuildModelToken(Token token, Guid tokenizedCorpusId, Guid? verseRowId)
    {
        var modelToken = new DataAccessLayer.Models.Token
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
            ExtendedProperties = token.ExtendedProperties,
            
        };

        if (token.HasMetadatum(MetadatumKeys.ModelTokenMetadata))
        {
            modelToken.Metadata = token.GetMetadatum<List<Metadatum>>(MetadatumKeys.ModelTokenMetadata).ToList();
        }

        return modelToken;
    }
}