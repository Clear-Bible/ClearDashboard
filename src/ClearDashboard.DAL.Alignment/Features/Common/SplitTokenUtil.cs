using ClearBible.Engine.Corpora;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Features.Common;

public static class SplitTokenUtil
{
	public static (Dictionary<TokenId, IEnumerable<CompositeToken>>, Dictionary<TokenId, IEnumerable<Token>>) SplitTokensIntoReplacements(
		List<Models.Token> tokensDb,
		(string surfaceText, string trainingText, string? tokenType)[] replacementTokenInfos,
		bool createParallelComposite,
		SplitTokenDbCommands splitTokenDbCommands,
		CancellationToken cancellationToken
	)
	{
		if (tokensDb.Count == 0)
			return ([], []);

		var replacementsBySourceId =
			CreateTokenReplacements(
				tokensDb,
				replacementTokenInfos,
				splitTokenDbCommands,
				cancellationToken
			);

		var incomingTokenIdCompositePairs = AttachToComposites(
			replacementsBySourceId,
			createParallelComposite,
			splitTokenDbCommands,
			cancellationToken
		);

		TransferNoteAssociationsToReplacementComposites(incomingTokenIdCompositePairs, splitTokenDbCommands, cancellationToken);

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

		return (splitCompositeTokensByIncomingTokenId, splitChildTokensByIncomingTokenId);
	}

	public static Dictionary<Guid, (Models.Token SourceModelToken, List<Token> ReplacementTokens, List<Models.Token> ReplacementModelTokens)> CreateTokenReplacements(List<Models.Token> tokensDb, (string surfaceText, string trainingText, string? tokenType)[] replacementTokenInfos, SplitTokenDbCommands splitTokenDbCommands, CancellationToken cancellationToken)
	{
		if (tokensDb.Count == 0)
			return [];

		var currentDateTime = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

		var replacementsBySourceId =
			new Dictionary<Guid, (
				Models.Token SourceModelToken,
				List<Token> ReplacementTokens,
				List<Models.Token> ReplacementModelTokens
			)>();

		foreach (var tokenDb in tokensDb)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var newChildTokens = new List<Token>();
			var newChildTokensDb = new List<Models.Token>();
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
			}

			replacementsBySourceId.Add(tokenDb.Id, (tokenDb, newChildTokens, newChildTokensDb));

			splitTokenDbCommands.AddTokenComponentsToInsert(newChildTokensDb);

			if (tokenDb.Deleted is null && !tokenDb.GetWasSplit())
			{
				tokenDb.Deleted = currentDateTime;
				tokenDb.SetWasSplit();
				splitTokenDbCommands.AddTokenComponentToSplitSoftDelete(tokenDb);
			}

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

	public static void TransferNoteAssociationsToReplacementChildren(
		Dictionary<Guid, (Models.Token SourceModelToken, List<Token> ReplacementTokens, List<Models.Token> ReplacementModelTokens)> replacementsBySourceId,
		SplitTokenDbCommands splitTokenDbCommands,
		CancellationToken cancellationToken)
	{
		var noteAssociationsDb = splitTokenDbCommands.ProjectDbContext.NoteDomainEntityAssociations
			.Where(dea => dea.DomainEntityIdGuid != null)
			.Where(dea => replacementsBySourceId.Keys.Contains((Guid)dea.DomainEntityIdGuid!))
			.ToList();

		foreach (var kvp in replacementsBySourceId)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var tokenDbNoteAssociations = noteAssociationsDb
				.Where(e => e.DomainEntityIdGuid == kvp.Key)
				.ToList();

			for (int i = 0; i < kvp.Value.ReplacementModelTokens.Count; i++)
			{
				if (i == 0)
				{
					splitTokenDbCommands.AddTokenComponentNoteAssociationsToSet(kvp.Value.ReplacementModelTokens[i], tokenDbNoteAssociations);
				}
				else
				{
					tokenDbNoteAssociations.ForEach(e =>
					{
						splitTokenDbCommands.AddNoteAssociationToInsert(new Models.NoteDomainEntityAssociation
						{
							NoteId = e.NoteId,
							DomainEntityIdName = e.DomainEntityIdName,
							DomainEntityIdGuid = kvp.Value.ReplacementModelTokens[i].Id
						});
					});
				}
			}
		}
	}

	public static void TransferNoteAssociationsToReplacementComposites(
		List<(Guid, CompositeToken)> incomingTokenIdCompositePairs,
		SplitTokenDbCommands splitTokenDbCommands,
		CancellationToken cancellationToken)
	{
		var compositesByIncomingTokenId = incomingTokenIdCompositePairs
			.GroupBy(e => e.Item1)
			.ToDictionary(g => g.Key, g => g.Select(e => e.Item2).ToList());

		var noteAssociationsDb = splitTokenDbCommands.ProjectDbContext.NoteDomainEntityAssociations
			.Where(dea => dea.DomainEntityIdGuid != null)
			.Where(dea => compositesByIncomingTokenId.Keys.Contains((Guid)dea.DomainEntityIdGuid!))
			.ToList();

		foreach (var kvp in compositesByIncomingTokenId)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var tokenDbNoteAssociations = noteAssociationsDb
				.Where(e => e.DomainEntityIdGuid == kvp.Key)
				.ToList();

			for (int i = 0; i < kvp.Value.Count; i++)
			{
				if (i == 0)
				{
					splitTokenDbCommands.AddTokenNoteAssociationsToSet(kvp.Value[i], tokenDbNoteAssociations);
				}
				else
				{
					tokenDbNoteAssociations.ForEach(e =>
					{
						splitTokenDbCommands.AddNoteAssociationToInsert(new Models.NoteDomainEntityAssociation
						{
							NoteId = e.NoteId,
							DomainEntityIdName = e.DomainEntityIdName,
							DomainEntityIdGuid = kvp.Value[i].TokenId.Id
						});
					});
				}
			}
		}
	}

	private static List<(Guid, CompositeToken)> AttachToComposites(
		Dictionary<Guid, (Models.Token SourceModelToken, List<Token> ReplacementTokens, List<Models.Token> ReplacementModelTokens)> replacementsBySourceId,
		bool createParallelComposite,
		SplitTokenDbCommands splitTokenDbCommands,
		CancellationToken cancellationToken)
	{
		var replacementTokensById = new Dictionary<Guid, List<Token>>();
		var replacementModelTokensById = new Dictionary<Guid, List<Models.Token>>();

		foreach (var kvp in replacementsBySourceId)
		{
			replacementTokensById.Add(kvp.Key, kvp.Value.ReplacementTokens);
			replacementModelTokensById.Add(kvp.Key, kvp.Value.ReplacementModelTokens);
		}

		var tokensDb = replacementsBySourceId.Values.Select(e => e.SourceModelToken).ToList();

		var incomingTokenIdCompositePairs = ReplaceInExistingComposites(
			tokensDb,
			replacementTokensById,
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
				TransferAssociations(tokenDb, tokenComposite, splitTokenDbCommands, true);
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
					TransferAssociations(tokenDb, tokenComposite, splitTokenDbCommands, isFirst);

					isFirst = false;
				}
			}
		}

		return incomingTokenIdCompositePairs;
	}

	private static List<(Guid, CompositeToken)> ReplaceInExistingComposites(
		List<Models.Token> tokensDb,
		Dictionary<Guid, List<Token>> replacementTokensById,
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
			.Include(e => e.SourceAlignments.Where(a => a.Deleted == null))
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
			tokenComposite.EngineTokenId = compositeToken.TokenId.ToString();
			splitTokenDbCommands.AddTokenComponentToUpdateSurfaceTrainingEngineTokenId(tokenComposite);
			splitTokenDbCommands.AddAlignmentTrainingTextChange(tokenComposite.SourceAlignments, previousTrainingText, tokenComposite.TrainingText);
		}

		return incomingTokenIdCompositePairs;
	}

	public static CompositeToken AssignChildTokensToTokenComposite(
		Models.TokenComposite tokenComposite,
		List<Token> newChildTokens,
		SplitTokenDbCommands splitTokenDbCommands)
	{
		// Add new associations from this composite to new child token ids:
		splitTokenDbCommands.AddTokenCompositeTokenAssociationsToInsert(newChildTokens
			.Select(e => new Models.TokenCompositeTokenAssociation
			{
				Id = Guid.NewGuid(),
				TokenCompositeId = tokenComposite.Id,
				TokenId = e.TokenId.Id
			}));

		// Using the higher level CompositeToken structure here (instead 
		// of Models.TokenComposite) should reset the Surface and Training
		// text using the new split child tokens:
		var compositeToken = ModelHelper.BuildCompositeToken(tokenComposite);
		compositeToken.Tokens = newChildTokens;

		// Update the token composite's surface and training text
		tokenComposite.SurfaceText = compositeToken.SurfaceText;
		tokenComposite.TrainingText = compositeToken.TrainingText;
		tokenComposite.EngineTokenId = compositeToken.TokenId.ToString();
		splitTokenDbCommands.AddTokenComponentToUpdateSurfaceTrainingEngineTokenId(tokenComposite);

		return compositeToken;
	}

	private static (Guid, Models.TokenComposite, CompositeToken) CreateComposite(
		Models.Token tokenDb,
		Guid? parallelCorpusId,
		List<Token> replacementTokens,
		List<Models.Token> replacementModelTokens,
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
			compositeToken.Metadata[Models.MetadatumKeys.IsParallelCorpusToken] = true;
		}

		compositeToken.SetSplitSourceId(tokenDb.Id);
		compositeToken.SetSplitSourceSurfaceText(tokenDb.SurfaceText!);
		compositeToken.SetSplitInitialChildren(replacementModelTokens);

		var tokenComposite = BuildModelTokenComposite(
			compositeToken,
			tokenDb.TokenizedCorpusId,
			tokenDb.VerseRowId,
			replacementModelTokens);
		tokenComposite.ParallelCorpusId = parallelCorpusId;

		splitTokenDbCommands.AddTokenComponentToInsert(tokenComposite);
		splitTokenDbCommands.AddTokenCompositeTokenAssociationsToInsert(replacementTokens
			.Select(e => new Models.TokenCompositeTokenAssociation
			{
				Id = Guid.NewGuid(),
				TokenCompositeId = tokenComposite.Id,
				TokenId = e.TokenId.Id
			}));

		return (tokenDb.Id, tokenComposite, compositeToken);
	}

	private static void TransferAssociations(
		Models.Token tokenDb,
		Models.TokenComposite tokenComposite,
		SplitTokenDbCommands splitTokenDbCommands,
		bool isFirstForTokenDb)
	{
		if (isFirstForTokenDb)
		{
			splitTokenDbCommands.AddAlignmentSourceIdsToSet(tokenDb.SourceAlignments, tokenComposite);
			splitTokenDbCommands.AddAlignmentTargetIdsToSet(tokenDb.TargetAlignments, tokenComposite);
			splitTokenDbCommands.AddTranslationSourceIdsToSet(tokenDb.Translations, tokenComposite);
			splitTokenDbCommands.AddTVATokenComponentIdsToSet(tokenDb.TokenVerseAssociations, tokenComposite);

			splitTokenDbCommands.AddAlignmentTrainingTextChange(tokenDb.SourceAlignments, tokenDb.TrainingText, tokenComposite.TrainingText);
		}
		else
		{
			foreach (var e in tokenDb.SourceAlignments)
			{
				splitTokenDbCommands.AddAlignmentToInsert(new Models.Alignment
				{
					AlignmentSetId = e.AlignmentSetId,
					SourceTokenComponentId = tokenComposite.Id,
					TargetTokenComponentId = e.TargetTokenComponentId,
					AlignmentOriginatedFrom = e.AlignmentOriginatedFrom,
					AlignmentVerification = e.AlignmentVerification,
					Score = e.Score
				});
			}
			foreach (var e in tokenDb.TargetAlignments)
			{
				splitTokenDbCommands.AddAlignmentToInsert(new Models.Alignment
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
				splitTokenDbCommands.AddTranslationToInsert(new Models.Translation
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
				splitTokenDbCommands.AddTokenVerseAssociationToInsert(new Models.TokenVerseAssociation
				{
					TokenComponentId = tokenComposite.Id,
					Position = e.Position,
					VerseId = e.VerseId
				});
			}

			splitTokenDbCommands.AddAlignmentTrainingTextChange(tokenDb.SourceAlignments, null, tokenComposite.TrainingText);
		}
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

		if (compositeToken.HasMetadatum(Models.MetadatumKeys.ModelTokenMetadata))
		{
			tokenComposite.Metadata = compositeToken.GetMetadatum<List<Models.Metadatum>>(Models.MetadatumKeys.ModelTokenMetadata).ToList();
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

	private static Models.Token BuildModelToken(Token token, Guid tokenizedCorpusId, Guid? verseRowId)
	{
		var modelToken = new Models.Token
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

		if (token.HasMetadatum(Models.MetadatumKeys.ModelTokenMetadata))
		{
			modelToken.Metadata = token.GetMetadatum<List<Models.Metadatum>>(Models.MetadatumKeys.ModelTokenMetadata).ToList();
		}

		return modelToken;
	}
}
