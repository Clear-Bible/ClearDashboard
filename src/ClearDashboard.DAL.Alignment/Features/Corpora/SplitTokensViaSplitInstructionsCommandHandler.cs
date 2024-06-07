using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Migrations;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SIL.Scripture;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Reflection;
using System.Threading;
using static ClearDashboard.DAL.Alignment.Features.Common.DataUtil;
using Token = ClearBible.Engine.Corpora.Token;

namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public class SplitTokensViaSplitInstructionsCommandHandler : ProjectDbContextCommandHandler<SplitTokensViaSplitInstructionsCommand,
    RequestResult<(IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>,
    (IDictionary<TokenId, IEnumerable<CompositeToken>>, IDictionary<TokenId, IEnumerable<Token>>)>
{
	private readonly IUserProvider _userProvider;
	private readonly IMediator _mediator;

    public SplitTokensViaSplitInstructionsCommandHandler(
        IMediator mediator,
        ProjectDbContextFactory? projectNameDbContextFactory, 
		IProjectProvider projectProvider,
		IUserProvider userProvider,
		ILogger<SplitTokensCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
        logger)
    {
		_userProvider = userProvider;
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

		using var splitTokenDbCommands = await SplitTokenDbCommands.CreateAsync(ProjectDbContext, _userProvider, cancellationToken);

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
			splitTokenDbCommands,
			cancellationToken
		);

		await splitTokenDbCommands.CommitTransactionAsync(cancellationToken);

		// TODO:  convert to bulk inserts?
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
		SplitTokenDbCommands splitTokenDbCommands, 
		CancellationToken cancellationToken
	)
	{
		var replacementsBySourceId =
			CreateTokenReplacements(
				tokensDb,
				replacementTokenInfos,
				splitTokenDbCommands,
				cancellationToken
			);

		var (incomingTokenIdCompositePairs, sourceTrainingTextsByAlignmentSetId) = AttachToComposites(
			replacementsBySourceId,
			createParallelComposite,
			splitTokenDbCommands,
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

	private static Dictionary<Guid, (DataAccessLayer.Models.Token SourceModelToken, List<Token> ReplacementTokens, List<DataAccessLayer.Models.Token> ReplacementModelTokens)> CreateTokenReplacements(List<DataAccessLayer.Models.Token> tokensDb, (string surfaceText, string trainingText, string? tokenType)[] replacementTokenInfos, SplitTokenDbCommands splitTokenDbCommands, CancellationToken cancellationToken)
    {
		var currentDateTime = DataAccessLayer.Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

        var replacementsBySourceId = 
            new Dictionary<Guid, (
                DataAccessLayer.Models.Token SourceModelToken, 
                List<Token> ReplacementTokens, 
                List<DataAccessLayer.Models.Token> ReplacementModelTokens
            )>();

		var noteAssociationsDb = splitTokenDbCommands.ProjectDbContext.NoteDomainEntityAssociations
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
					splitTokenDbCommands.AddTokenComponentNoteAssociationsToSet(newModelToken, tokenDbNoteAssociations);
				}
				else
				{
					tokenDbNoteAssociations.ForEach(e =>
					{
						splitTokenDbCommands.AddNoteAssociationToInsert(new DataAccessLayer.Models.NoteDomainEntityAssociation
						{
							NoteId = e.NoteId,
							DomainEntityIdName = e.DomainEntityIdName,
							DomainEntityIdGuid = newToken.TokenId.Id
						});
					});
				}
			}

            replacementsBySourceId.Add(tokenDb.Id, (tokenDb, newChildTokens, newChildTokensDb));

			splitTokenDbCommands.AddTokenComponentsToInsert(newChildTokensDb);
			tokenDb.Deleted = currentDateTime;

			// Find any tokens with a BCVW that matches the current tokenDb and
			// with a SubwordNumber that is greater than the token that was split,
			// and do a Subword renumbering
			nextSubwordNumber++;
			var tokensHavingSubwordsToRenumberDb = splitTokenDbCommands.ProjectDbContext.Tokens
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

				splitTokenDbCommands.AddTokenToSubwordRenumber(tokensHavingSubwordsToRenumberDb[i]);
			}
		}

        return replacementsBySourceId;
	}

	private static (List<(Guid, CompositeToken)>, Dictionary<Guid, List<string>>) AttachToComposites(
		Dictionary<Guid, (DataAccessLayer.Models.Token SourceModelToken, List<Token> ReplacementTokens, List<DataAccessLayer.Models.Token> ReplacementModelTokens)> replacementsBySourceId,
		bool createParallelComposite,
		SplitTokenDbCommands splitTokenDbCommands, 
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
			splitTokenDbCommands,
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
					splitTokenDbCommands);

				incomingTokenIdCompositePairs.Add((tokenDb.Id, compositeToken));
				TransferAssociations(tokenDb, tokenComposite, sourceTrainingTextsByAlignmentSetId, splitTokenDbCommands, true);
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
						splitTokenDbCommands);

					incomingTokenIdCompositePairs.Add((tokenDb.Id, compositeToken));
					TransferAssociations(tokenDb, tokenComposite, sourceTrainingTextsByAlignmentSetId, splitTokenDbCommands, isFirst);

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
		SplitTokenDbCommands splitTokenDbCommands, 
        CancellationToken cancellationToken)
    {
		var incomingTokenIdCompositePairs = new List<(Guid, CompositeToken)>();

		var existingCompositeIds = tokensDb
	        .SelectMany(e => e.TokenCompositeTokenAssociations
		        .Select(t => t.TokenCompositeId))
	        .Distinct();

		var tokensDbTokenComposites = splitTokenDbCommands.ProjectDbContext.TokenComposites
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
			splitTokenDbCommands.AddTokenCompositeTokenAssociationsToRemove(tokensDb
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
				splitTokenDbCommands.AddTokenCompositeTokenAssociationsToInsert(replacementTokensById[tokenIdToReplace]
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
			splitTokenDbCommands.AddTokenComponentToUpdateSurfaceTrainingText(tokenComposite);

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
		SplitTokenDbCommands splitTokenDbCommands)
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

		splitTokenDbCommands.AddTokenComponentToInsert(tokenComposite);

		return (tokenDb.Id, tokenComposite, compositeToken);
	}

	private static void TransferAssociations(
		DataAccessLayer.Models.Token tokenDb,
		DataAccessLayer.Models.TokenComposite tokenComposite,
		Dictionary<Guid, List<string>> sourceTrainingTextsByAlignmentSetId,
		SplitTokenDbCommands splitTokenDbCommands,
		bool isFirstForTokenDb)
	{
		if (isFirstForTokenDb)
		{
			splitTokenDbCommands.AddAlignmentSourceIdsToSet(tokenDb.SourceAlignments, tokenComposite);
			splitTokenDbCommands.AddAlignmentTargetIdsToSet(tokenDb.TargetAlignments, tokenComposite);
			splitTokenDbCommands.AddTranslationSourceIdsToSet(tokenDb.Translations, tokenComposite);
			splitTokenDbCommands.AddTVATokenComponentIdsToSet(tokenDb.TokenVerseAssociations, tokenComposite);

			foreach (var e in tokenDb.SourceAlignments)
			{
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
		}
		else
		{
			foreach (var e in tokenDb.SourceAlignments)
			{
				splitTokenDbCommands.AddAlignmentToInsert(new DataAccessLayer.Models.Alignment
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
				splitTokenDbCommands.AddAlignmentToInsert(new DataAccessLayer.Models.Alignment
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
				splitTokenDbCommands.AddTranslationToInsert(new DataAccessLayer.Models.Translation
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
				splitTokenDbCommands.AddTokenVerseAssociationToInsert(new DataAccessLayer.Models.TokenVerseAssociation
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
            tokenComposite.Metadata = compositeToken.GetMetadatum<List<Metadatum>>(MetadatumKeys.ModelTokenMetadata).ToList();
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


public class SplitTokenDbCommands : IDisposable
{
	private bool disposedValue;

	public ProjectDbContext ProjectDbContext { get; init; }
	public IUserProvider UserProvider { get; init; }
	public DbConnection Connection { get; init; }
	public DbTransaction Transaction { get; init; }

	private readonly DbCommand _tokenComponentInsertCommand;
	private readonly DbCommand _tokenComponentUpdateSurfaceTrainingTextCommand;
	private readonly DbCommand _tokenCompositeTokenAssociationInsertCommand;
	private readonly DbCommand _tokenComponentSoftDeleteCommand;
	private readonly DbCommand _tokenComponentUpdateTypeCommand;
	private readonly DbCommand _tokenSubwordRenumberCommand;
	private readonly DbCommand _noteAssociationInsertCommand;
	private readonly DbCommand _noteAssociationDomainEntityIdSetCommand;
	private readonly DbCommand _alignmentSourceIdSetCommand;
	private readonly DbCommand _alignmentTargetIdSetCommand;
	private readonly DbCommand _translationSourceIdSetCommand;
	private readonly DbCommand _tvaTokenComponentIdSetCommand;
	private readonly DbCommand _alignmentInsertCommand;
	private readonly DbCommand _translationInsertCommand;
	private readonly DbCommand _tvaInsertCommand;

	private readonly List<TokenComponent> _tokenComponentsToInsert = new();
	private readonly List<TokenComponent> _tokenComponentsToUpdateSurfaceTrainingText = new();
	private readonly List<TokenCompositeTokenAssociation> _tokenCompositeTokenAssociationsToInsert = new();
	private readonly List<TokenCompositeTokenAssociation> _tokenCompositeTokenAssociationsToRemove = new();
	private readonly List<TokenComponent> _tokenComponentsToSoftDelete = new();
	private readonly List<TokenComponent> _tokenComponentsToUpdateType = new();
	private readonly List<DataAccessLayer.Models.Token> _tokensToSubwordRenumber = new();
	private readonly List<NoteDomainEntityAssociation> _noteAssociationsToInsert = new();
	private readonly List<(TokenComponent TokenComponent, IEnumerable<NoteDomainEntityAssociation> NoteAssociations)> _tokenComponentNoteAssociationsToSet = new();
	private readonly List<(IEnumerable<Guid> AlignmentIds, Guid TokenComponentId)> _alignmentSourceIdsToSet = new();
	private readonly List<(IEnumerable<Guid> AlignmentIds, Guid TokenComponentId)> _alignmentTargetIdsToSet = new();
	private readonly List<(IEnumerable<Guid> TranslationIds, Guid TokenComponentId)> _translationSourceIdsToSet = new();
	private readonly List<(IEnumerable<Guid> TVAIds, Guid TokenComponentId)> _tvaTokenComponentIdsToSet = new();
	private readonly List<DataAccessLayer.Models.Alignment> _alignmentsToInsert = new();
	private readonly List<DataAccessLayer.Models.Translation> _translationsToInsert = new();
	private readonly List<TokenVerseAssociation> _tvasToInsert = new();

	public static async Task<SplitTokenDbCommands> CreateAsync(ProjectDbContext projectDbContext, IUserProvider userProvider, CancellationToken cancellationToken)
	{
		projectDbContext.Database.OpenConnection();

		var connection = projectDbContext.Database.GetDbConnection();
		var transaction = await connection.BeginTransactionAsync(cancellationToken);

		return new SplitTokenDbCommands(projectDbContext, userProvider, connection, transaction);
	}

	private SplitTokenDbCommands(ProjectDbContext projectDbContext, IUserProvider userProvider, DbConnection connection, DbTransaction transaction)
	{
		ProjectDbContext = projectDbContext;
		UserProvider = userProvider;
		Connection = connection;
		Transaction = transaction;

		_tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection);
		_tokenComponentUpdateSurfaceTrainingTextCommand = TokenizedCorpusDataBuilder.CreateTokenComponentUpdateSurfaceTrainingTextCommand(connection);
		_tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection);
		_tokenComponentSoftDeleteCommand = DataUtil.CreateSoftDeleteByIdUpdateCommand(connection, typeof(TokenComponent));
		_tokenComponentUpdateTypeCommand = TokenizedCorpusDataBuilder.CreateTokenComponentTypeUpdateCommand(connection);
		_tokenSubwordRenumberCommand = TokenizedCorpusDataBuilder.CreateTokenSubwordRenumberCommand(connection);
		_noteAssociationInsertCommand = CreateNoteAssociationInsertCommand(connection);
		_noteAssociationDomainEntityIdSetCommand = CreateNoteAssociationDomainEntityIdSetCommand(connection);
		_alignmentSourceIdSetCommand = AlignmentUtil.CreateAlignmentSourceOrTargetIdSetCommand(connection, true);
		_alignmentTargetIdSetCommand = AlignmentUtil.CreateAlignmentSourceOrTargetIdSetCommand(connection, false);
		_translationSourceIdSetCommand = AlignmentUtil.CreateTranslationSourceIdSetCommand(connection);
		_tvaTokenComponentIdSetCommand = AlignmentUtil.CreateTVATokenComponentIdSetCommand(connection);
		_alignmentInsertCommand = AlignmentUtil.CreateAlignmentInsertCommand(connection);
		_translationInsertCommand = AlignmentUtil.CreateTranslationInsertCommand(connection);
		_tvaInsertCommand = AlignmentUtil.CreateTokenVerseAssociationInsertCommand(connection);
	}

	public async Task CommitTransactionAsync(CancellationToken cancellationToken)
	{
		await Transaction.CommitAsync(cancellationToken);
	}

	public async Task ExecuteBulkOperationsAsync(CancellationToken cancellationToken)
	{
		foreach (var tokenComponent in _tokenComponentsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (tokenComponent is TokenComposite)
			{
				await TokenizedCorpusDataBuilder.InsertTokenCompositeAsync((tokenComponent as TokenComposite)!, _tokenComponentInsertCommand, cancellationToken);
			}
			else
			{
				await TokenizedCorpusDataBuilder.InsertTokenAsync((tokenComponent as DataAccessLayer.Models.Token)!, null, _tokenComponentInsertCommand, cancellationToken);
			}

		}

		foreach (var tc in _tokenComponentsToUpdateSurfaceTrainingText)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.UpdateTokenComponentSurfaceTrainingTextAsync(tc, _tokenComponentUpdateSurfaceTrainingTextCommand, cancellationToken);
		}

		foreach (var tc in _tokenComponentsToSoftDelete)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.SoftDeleteTokenComponentAsync(tc, _tokenComponentSoftDeleteCommand, cancellationToken);
		}

		foreach (var tc in _tokenComponentsToUpdateType)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.UpdateTypeTokenComponentAsync(tc, _tokenComponentUpdateTypeCommand, cancellationToken);
		}

		foreach (var t in _tokensToSubwordRenumber)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await TokenizedCorpusDataBuilder.SubwordRenumberTokenAsync(t, _tokenSubwordRenumberCommand, cancellationToken);
		}

		foreach (var na in _noteAssociationsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await InsertNoteDomainEntityAssociationAsync(na, _noteAssociationInsertCommand, cancellationToken);
		}

		foreach (var tcn in _tokenComponentNoteAssociationsToSet)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await SetNoteAssociationsDomainEntityIdAsync(tcn.TokenComponent, tcn.NoteAssociations, _noteAssociationDomainEntityIdSetCommand, cancellationToken);
		}

		if (_tokenCompositeTokenAssociationsToRemove.Count != 0)
		{
			await DataUtil.DeleteIdentifiableEntityAsync(
				Connection,
				typeof(TokenCompositeTokenAssociation),
				_tokenCompositeTokenAssociationsToRemove.Select(e => e.Id).ToArray(),
				cancellationToken);
		}

		foreach (var t in _tokenCompositeTokenAssociationsToInsert)
		{
			t.Id = await TokenizedCorpusDataBuilder.InsertTokenCompositeTokenAssociationAsync(t.TokenId, t.TokenCompositeId, _tokenCompositeTokenAssociationInsertCommand, cancellationToken);
		}

		foreach (var (AlignmentIds, TokenComponentId) in _alignmentSourceIdsToSet) 
		{
			foreach (var alignmentId in AlignmentIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetAlignmentSourceIdAsync(alignmentId, TokenComponentId, _alignmentSourceIdSetCommand, cancellationToken);
			}
		}

		foreach (var (AlignmentIds, TokenComponentId) in _alignmentTargetIdsToSet)
		{
			foreach (var alignmentId in AlignmentIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetAlignmentTargetIdAsync(alignmentId, TokenComponentId, _alignmentTargetIdSetCommand, cancellationToken);
			}
		}

		foreach (var (TranslationIds, TokenComponentId) in _translationSourceIdsToSet)
		{
			foreach (var translationId in TranslationIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetTranslationSourceIdAsync(translationId, TokenComponentId, _translationSourceIdSetCommand, cancellationToken);
			}
		}

		foreach (var (TVAIds, TokenComponentId) in _tvaTokenComponentIdsToSet)
		{
			foreach (var tvaId in TVAIds)
			{
				cancellationToken.ThrowIfCancellationRequested();
				await AlignmentUtil.SetTVATokenComponentIdAsync(tvaId, TokenComponentId, _tvaTokenComponentIdSetCommand, cancellationToken);
			}
		}

		foreach (var alignment in _alignmentsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AlignmentUtil.InsertAlignmentAsync(
				alignment, 
				alignment.AlignmentSetId, 
				_alignmentInsertCommand, 
				Guid.Empty != alignment.UserId ? alignment.UserId : UserProvider.CurrentUser!.Id, 
				cancellationToken);
		}
		foreach (var translation in _translationsToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AlignmentUtil.InsertTranslationAsync(
				translation, 
				translation.TranslationSetId, 
				_translationInsertCommand, 
				Guid.Empty != translation.UserId ? translation.UserId : UserProvider.CurrentUser!.Id, 
				cancellationToken);
		}

		foreach (var tva in _tvasToInsert)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await AlignmentUtil.InsertTokenVerseAssociationAsync(
				tva, 
				_tvaInsertCommand, 
				Guid.Empty != tva.UserId ? tva.UserId : UserProvider.CurrentUser!.Id, 
				cancellationToken);
		}

		_tokenComponentsToInsert.Clear();
		_tokenComponentsToUpdateSurfaceTrainingText.Clear();
		_tokenComponentsToSoftDelete.Clear();
		_tokenComponentsToUpdateType.Clear();
		_tokensToSubwordRenumber.Clear();
		_noteAssociationsToInsert.Clear();
		_tokenComponentNoteAssociationsToSet.Clear();
		_tokenCompositeTokenAssociationsToRemove.Clear();
		_tokenCompositeTokenAssociationsToInsert.Clear();
		_alignmentSourceIdsToSet.Clear();
		_alignmentTargetIdsToSet.Clear();
		_translationSourceIdsToSet.Clear();
		_tvaTokenComponentIdsToSet.Clear();
		_alignmentsToInsert.Clear();
		_translationsToInsert.Clear();
		_tvasToInsert.Clear();
}

	public void AddTokenComponentToInsert(TokenComponent tokenComponent)
	{
		_tokenComponentsToInsert.Add(tokenComponent);
	}

	public void AddTokenComponentsToInsert(IEnumerable<TokenComponent> tokenComponents)
	{
		_tokenComponentsToInsert.AddRange(tokenComponents);
	}

	public void AddTokenComponentToUpdateSurfaceTrainingText(TokenComponent tokenComponent)
	{
		_tokenComponentsToUpdateSurfaceTrainingText.Add(tokenComponent);
	}

	public void AddTokenComponentToSoftDelete(IEnumerable<TokenComponent> tokenComponents)
	{
		_tokenComponentsToSoftDelete.AddRange(tokenComponents);
	}

	public void AddTokenComponentToUpdateType(TokenComponent tokenComponent)
	{
		_tokenComponentsToUpdateType.Add(tokenComponent);
	}

	public void AddTokenToSubwordRenumber(DataAccessLayer.Models.Token token)
	{
		_tokensToSubwordRenumber.Add(token);
	}

	public void AddNoteAssociationToInsert(NoteDomainEntityAssociation noteAssociation)
	{
		_noteAssociationsToInsert.Add(noteAssociation);
	}

	public void AddTokenComponentNoteAssociationsToSet(TokenComponent tokenComponent, IEnumerable<NoteDomainEntityAssociation> noteAssociations)
	{
		_tokenComponentNoteAssociationsToSet.Add((tokenComponent, noteAssociations));
	}

	public void AddTokenCompositeTokenAssociationsToInsert(IEnumerable<TokenCompositeTokenAssociation> tokenAssociations)
	{
		_tokenCompositeTokenAssociationsToInsert.AddRange(tokenAssociations);
	}

	public void AddTokenCompositeTokenAssociationsToRemove(IEnumerable<TokenCompositeTokenAssociation> tokenAssociations)
	{
		_tokenCompositeTokenAssociationsToRemove.AddRange(tokenAssociations);
	}

	public void AddAlignmentSourceIdsToSet(IEnumerable<DataAccessLayer.Models.Alignment> alignments, TokenComponent sourceTokenComponent)
	{
		_alignmentSourceIdsToSet.Add((alignments.Select(e => e.Id), sourceTokenComponent.Id));
	}

	public void AddAlignmentTargetIdsToSet(IEnumerable<DataAccessLayer.Models.Alignment> alignments, TokenComponent targetTokenComponent)
	{
		_alignmentTargetIdsToSet.Add((alignments.Select(e => e.Id), targetTokenComponent.Id));
	}

	public void AddTranslationSourceIdsToSet(IEnumerable<DataAccessLayer.Models.Translation> translations, TokenComponent sourceTokenComponent)
	{
		_translationSourceIdsToSet.Add((translations.Select(e => e.Id), sourceTokenComponent.Id));
	}

	public void AddTVATokenComponentIdsToSet(IEnumerable<DataAccessLayer.Models.TokenVerseAssociation> tvas, TokenComponent tokenComponent)
	{
		_tvaTokenComponentIdsToSet.Add((tvas.Select(e => e.Id), tokenComponent.Id));
	}

	public void AddAlignmentToInsert(DataAccessLayer.Models.Alignment alignment)
	{
		_alignmentsToInsert.Add(alignment);
	}

	public void AddTranslationToInsert(DataAccessLayer.Models.Translation translation)
	{
		_translationsToInsert.Add(translation);
	}

	public void AddTokenVerseAssociationToInsert(TokenVerseAssociation tokenVerseAssociation)
	{
		_tvasToInsert.Add(tokenVerseAssociation);
	}

	private static DbCommand CreateNoteAssociationInsertCommand(DbConnection connection)
	{
		var command = connection.CreateCommand();
		var columns = new string[] { "Id", "NoteId", "DomainEntityIdGuid", "DomainEntityIdName" };

		DataUtil.ApplyColumnsToInsertCommand(command, typeof(NoteDomainEntityAssociation), columns);

		command.Prepare();

		return command;
	}

	private static async Task InsertNoteDomainEntityAssociationAsync(NoteDomainEntityAssociation noteAssociation, DbCommand command, CancellationToken cancellationToken)
	{
		command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = noteAssociation.Id;
		command.Parameters[$"@{nameof(NoteDomainEntityAssociation.NoteId)}"].Value = noteAssociation.NoteId;
		command.Parameters[$"@{nameof(NoteDomainEntityAssociation.DomainEntityIdGuid)}"].Value = noteAssociation.DomainEntityIdGuid;
		command.Parameters[$"@{nameof(NoteDomainEntityAssociation.DomainEntityIdName)}"].Value = noteAssociation.DomainEntityIdName;
		_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
	}

	private static DbCommand CreateNoteAssociationDomainEntityIdSetCommand(DbConnection connection)
	{
		var command = connection.CreateCommand();
		var columns = new string[] { nameof(NoteDomainEntityAssociation.DomainEntityIdGuid) };
		var whereColumns = new (string, WhereEquality)[] { (nameof(IdentifiableEntity.Id), WhereEquality.Equals) };

		DataUtil.ApplyColumnsToUpdateCommand(
			command,
			typeof(NoteDomainEntityAssociation),
			columns,
			whereColumns,
			Array.Empty<(string, int)>());

		command.Prepare();

		return command;
	}

	private async Task SetNoteAssociationsDomainEntityIdAsync(TokenComponent tokenComponent, IEnumerable<NoteDomainEntityAssociation> noteAssociations, DbCommand command, CancellationToken cancellationToken)
	{
		foreach (var noteAssociation in noteAssociations)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Keep DbContext model in sync:
			noteAssociation.DomainEntityIdGuid = tokenComponent.Id;

			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = noteAssociation.Id;
			command.Parameters[$"@{nameof(NoteDomainEntityAssociation.DomainEntityIdGuid)}"].Value = tokenComponent.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_tokenComponentInsertCommand.Dispose();
				_tokenComponentUpdateSurfaceTrainingTextCommand.Dispose();
				_tokenCompositeTokenAssociationInsertCommand.Dispose();
				_tokenComponentSoftDeleteCommand.Dispose();
				_tokenComponentUpdateTypeCommand.Dispose();
				_tokenSubwordRenumberCommand.Dispose();
				_noteAssociationInsertCommand.Dispose();
				_noteAssociationDomainEntityIdSetCommand.Dispose();
				_alignmentSourceIdSetCommand.Dispose();
				_alignmentTargetIdSetCommand.Dispose();
				_translationSourceIdSetCommand.Dispose();
				_tvaTokenComponentIdSetCommand.Dispose();
				_alignmentInsertCommand.Dispose();
				_translationInsertCommand.Dispose();
				_tvaInsertCommand.Dispose();

				Transaction.Dispose();

				ProjectDbContext.Database.CloseConnection();
				Connection.Dispose();
			}

			// TODO: free unmanaged resources (unmanaged objects) and override finalizer
			// TODO: set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
