using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Interfaces;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using static ClearDashboard.DAL.Alignment.Features.Common.DataUtil;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class TokenizedCorpusDataBuilder
    {
        public static IEnumerable<TokensTextRow> ExtractValidateBook(ITextCorpus textCorpus, string bookId, string? corpusName)
        {
            List<TokensTextRow> tokensTextRows = new();
            try
            {
                tokensTextRows = textCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>().ToList();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            var dups = tokensTextRows
                .SelectMany(ttr => ttr.Tokens)
                .SelectMany(t => (t is CompositeToken token) ? token.Tokens : new List<Token>() { t })
                .GroupBy(t => t.TokenId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (dups.Any())
            {
                throw new InvalidDataEngineException(name: "Token.Ids", value: $"{string.Join(",", dups)}", message: $"Engine token Id duplicates found in corpus '{corpusName}' book '{bookId}' Tokens Ids: {string.Join(", ", dups)}");
            }

            var multiVerseSpanningComposites = tokensTextRows
                .SelectMany(ttr => ttr.Tokens)
                .Where(ct => ct is CompositeToken)
                .Select(ct => (ct as CompositeToken)!.Tokens
                    .GroupBy(token => new { token.TokenId.BookNumber, token.TokenId.ChapterNumber, token.TokenId.VerseNumber }))
                    .Where(g2 => g2.Count() > 1)
                    .Select(g2 => g2
                        .Select(g => new { bcv = g.Key, Count = g.Count() }));

            if (multiVerseSpanningComposites.Any())
            {
                throw new Exception($"TokensTextRow for book '{bookId}' contains CompositeToken(s) having child tokens from more than one book-chapter-verse");
            }

            return tokensTextRows;
        }
        public static (IEnumerable<Models.VerseRow>, int) BuildVerseRowModel(IEnumerable<TokensTextRow> tokensTextRows, Guid tokenizedCorpusId)
        {
            var tokenCount = 0;
            var verseRows = tokensTextRows
                .Where(ttr => ttr.IsEmpty == false)
                .Select(ttr =>
                {
                    var verseRowId = Guid.NewGuid(); //TEMP
                    var (b, c, v) = (
                        ((VerseRef)ttr.Ref).BookNum,
                        ((VerseRef)ttr.Ref).ChapterNum,
                        ((VerseRef)ttr.Ref).VerseNum);

                    return new Models.VerseRow
                    {
                        Id = verseRowId,
                        TokenizedCorpusId = tokenizedCorpusId,
                        BookChapterVerse = $"{b:000}{c:000}{v:000}",
                        OriginalText = ttr.OriginalText,
                        IsSentenceStart = ttr.IsSentenceStart,
                        IsInRange = ttr.IsInRange,
                        IsRangeStart = ttr.IsRangeStart,
                        IsEmpty = ttr.IsEmpty,
                        TokenComponents = ttr.Tokens
                            .Select(token =>
                            {
                                if (token is CompositeToken compositeToken)
                                {
                                    tokenCount++;
                                    return new Models.TokenComposite
                                    {
                                        Id = compositeToken.TokenId.Id,
                                        VerseRowId = verseRowId,
                                        TokenizedCorpusId = tokenizedCorpusId,
                                        TrainingText = compositeToken.TrainingText,
                                        SurfaceText = compositeToken.SurfaceText,
                                        ExtendedProperties = compositeToken.ExtendedProperties,
                                        EngineTokenId = compositeToken.TokenId.ToString(),
                                        Tokens = compositeToken.GetPositionalSortedBaseTokens()
                                            .Select(childToken =>
                                            {
                                                tokenCount++;
                                                var modelToken =  new Models.Token
                                                {
                                                    Id = childToken.TokenId.Id,
                                                    VerseRowId = verseRowId,
                                                    TokenizedCorpusId = tokenizedCorpusId,
                                                    TrainingText = childToken.TrainingText,
                                                    EngineTokenId = childToken.TokenId.ToString(),
                                                    BookNumber = childToken.TokenId.BookNumber,
                                                    ChapterNumber = childToken.TokenId.ChapterNumber,
                                                    VerseNumber = childToken.TokenId.VerseNumber,
                                                    WordNumber = childToken.TokenId.WordNumber,
                                                    SubwordNumber = childToken.TokenId.SubWordNumber,
                                                    SurfaceText = childToken.SurfaceText,
                                                    ExtendedProperties = childToken.ExtendedProperties
                                                };

                                                if (childToken.HasMetadatum(Models.MetadatumKeys.ModelTokenMetadata))
                                                {
                                                    modelToken.Metadata = childToken.GetMetadatum<List<Models.Metadatum>>(Models.MetadatumKeys.ModelTokenMetadata).ToList();
                                                }

                                                return modelToken;
                                            }).ToList()
                                    };
                                }
                                else
                                {
                                    tokenCount++;
                                    var modelToken  = new Models.Token
                                    {
                                        Id = token.TokenId.Id,
                                        VerseRowId = verseRowId,
                                        TokenizedCorpusId = tokenizedCorpusId,
                                        TrainingText = token.TrainingText,
                                        EngineTokenId = token.TokenId.ToString(),
                                        BookNumber = token.TokenId.BookNumber,
                                        ChapterNumber = token.TokenId.ChapterNumber,
                                        VerseNumber = token.TokenId.VerseNumber,
                                        WordNumber = token.TokenId.WordNumber,
                                        SubwordNumber = token.TokenId.SubWordNumber,
                                        SurfaceText = token.SurfaceText,
                                        ExtendedProperties = token.ExtendedProperties
                                    } as Models.TokenComponent;

                                    if (token.HasMetadatum(Models.MetadatumKeys.ModelTokenMetadata))
                                    {
                                        modelToken.Metadata = token.GetMetadatum<List<Models.Metadatum>>(Models.MetadatumKeys.ModelTokenMetadata).ToList();
                                    }

                                    return modelToken;
                                }
                            })
                        .ToList()
                    };
                });

            return (verseRows, tokenCount);
        }
        public static DbCommand CreateVerseRowUpdateCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "OriginalText", "IsSentenceStart", "IsInRange", "IsRangeStart", "IsEmpty", "Modified" };
            var whereColumns = new (string, DataUtil.WhereEquality)[] { ("Id", DataUtil.WhereEquality.Equals) };

            DataUtil.ApplyColumnsToUpdateCommand(command, typeof(Models.VerseRow), columns, whereColumns, Array.Empty<(string, int)>());

            command.Prepare();

            return command;
        }

        public static async Task UpdateVerseRowAsync(Models.VerseRow verseRow, DbCommand verseRowCmd, CancellationToken cancellationToken)
        {
            verseRowCmd.Parameters["@Id"].Value = verseRow.Id;
            verseRowCmd.Parameters["@OriginalText"].Value = verseRow.OriginalText != null ? verseRow.OriginalText : DBNull.Value;
            verseRowCmd.Parameters["@IsSentenceStart"].Value = verseRow.IsSentenceStart;
            verseRowCmd.Parameters["@IsInRange"].Value = verseRow.IsInRange;
            verseRowCmd.Parameters["@IsRangeStart"].Value = verseRow.IsRangeStart;
            verseRowCmd.Parameters["@IsEmpty"].Value = verseRow.IsEmpty;
            verseRowCmd.Parameters["@Modified"].Value = verseRow.Modified;

            _ = await verseRowCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateVerseRowInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "BookChapterVerse", "OriginalText", "TokenizedCorpusId", "IsSentenceStart", "IsInRange", "IsRangeStart", "IsEmpty", "UserId", "Created" };

            DataUtil.ApplyColumnsToInsertCommand(command, typeof(Models.VerseRow), columns);

            command.Prepare();

            return command;
        }

        public static async Task InsertVerseRowAsync(Models.VerseRow verseRow, DbCommand verseRowCmd, IUserProvider userProvider, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            verseRowCmd.Parameters["@Id"].Value = verseRow.Id;
            verseRowCmd.Parameters["@BookChapterVerse"].Value = verseRow.BookChapterVerse;
            verseRowCmd.Parameters["@OriginalText"].Value = verseRow.OriginalText != null ? verseRow.OriginalText : DBNull.Value;
            verseRowCmd.Parameters["@IsSentenceStart"].Value = verseRow.IsSentenceStart;
            verseRowCmd.Parameters["@IsInRange"].Value = verseRow.IsInRange;
            verseRowCmd.Parameters["@IsRangeStart"].Value = verseRow.IsRangeStart;
            verseRowCmd.Parameters["@IsEmpty"].Value = verseRow.IsEmpty;
            verseRowCmd.Parameters["@TokenizedCorpusId"].Value = verseRow.TokenizedCorpusId;
            verseRowCmd.Parameters["@UserId"].Value = Guid.Empty != verseRow.UserId ? verseRow.UserId : userProvider!.CurrentUser!.Id;
            verseRowCmd.Parameters["@Created"].Value = converter.ConvertToProvider(verseRow.Created);

            _ = await verseRowCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateTokenComponentInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "EngineTokenId", "TrainingText", "VerseRowId", "TokenizedCorpusId", "Discriminator", "BookNumber", "ChapterNumber", "VerseNumber", "WordNumber", "SubwordNumber", "SurfaceText", "ExtendedProperties" };

            DataUtil.ApplyColumnsToInsertCommand(command, typeof(Models.TokenComponent), columns);

            command.Prepare();

            return command;
        }

        public static DbCommand CreateTokenCompositeTokenAssociationInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "TokenId", "TokenCompositeId" };

            DataUtil.ApplyColumnsToInsertCommand(command, typeof(Models.TokenCompositeTokenAssociation), columns);

            command.Prepare();

            return command;
        }

        public static async Task InsertTokenComponentsAsync(IEnumerable<Models.TokenComponent> tokenComponents, DbCommand componentCmd, DbCommand assocCmd, CancellationToken cancellationToken)
        {
            foreach (var tokenComponent in tokenComponents)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (tokenComponent is Models.TokenComposite)
                {
                    var tokenComposite = (tokenComponent as Models.TokenComposite)!;
                    await InsertTokenCompositeAsync(tokenComposite, componentCmd, cancellationToken);

                    foreach (var token in tokenComposite.Tokens)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await InsertTokenAsync(token, tokenComposite.Id, componentCmd, cancellationToken);
                        await InsertTokenCompositeTokenAssociationAsync(token.Id, tokenComposite.Id, assocCmd, cancellationToken);
                    }
                }
                else
                {
                    await InsertTokenAsync((tokenComponent as Models.Token)!, null, componentCmd, cancellationToken);
                }

            }
        }
        public static async Task InsertTokenAsync(Models.Token token, Guid? tokenCompositeId, DbCommand componentCmd, CancellationToken cancellationToken)
        {
            componentCmd.Parameters["@Id"].Value = token.Id;
            componentCmd.Parameters["@EngineTokenId"].Value = token.EngineTokenId;
            componentCmd.Parameters["@TrainingText"].Value = token.TrainingText;
            componentCmd.Parameters["@VerseRowId"].Value = token.VerseRowId;
            componentCmd.Parameters["@TokenizedCorpusId"].Value = token.TokenizedCorpusId;
            componentCmd.Parameters["@Discriminator"].Value = token.GetType().Name;
            componentCmd.Parameters["@BookNumber"].Value = token.BookNumber;
            componentCmd.Parameters["@ChapterNumber"].Value = token.ChapterNumber;
            componentCmd.Parameters["@VerseNumber"].Value = token.VerseNumber;
            componentCmd.Parameters["@WordNumber"].Value = token.WordNumber;
            componentCmd.Parameters["@SubwordNumber"].Value = token.SubwordNumber;
            componentCmd.Parameters["@SurfaceText"].Value = token.SurfaceText;
            componentCmd.Parameters["@ExtendedProperties"].Value = token.ExtendedProperties != null ? token.ExtendedProperties : DBNull.Value;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        public static async Task InsertTokenCompositeAsync(Models.TokenComposite tokenComposite, DbCommand componentCmd, CancellationToken cancellationToken)
        {
            componentCmd.Parameters["@Id"].Value = tokenComposite.Id;
            componentCmd.Parameters["@EngineTokenId"].Value = tokenComposite.EngineTokenId;
            componentCmd.Parameters["@TrainingText"].Value = tokenComposite.TrainingText;
            componentCmd.Parameters["@VerseRowId"].Value = tokenComposite.VerseRowId;
            componentCmd.Parameters["@ExtendedProperties"].Value = tokenComposite.ExtendedProperties != null ? tokenComposite.ExtendedProperties : DBNull.Value;
            componentCmd.Parameters["@TokenizedCorpusId"].Value = tokenComposite.TokenizedCorpusId;
            componentCmd.Parameters["@Discriminator"].Value = tokenComposite.GetType().Name;
            componentCmd.Parameters["@BookNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@ChapterNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@VerseNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@WordNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@SubwordNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@SurfaceText"].Value = tokenComposite.SurfaceText;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        public static async Task<Guid> InsertTokenCompositeTokenAssociationAsync(Guid tokenId, Guid tokenCompositeId, DbCommand assocCmd, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();

            assocCmd.Parameters["@Id"].Value = id;
            assocCmd.Parameters["@TokenId"].Value = tokenId;
            assocCmd.Parameters["@TokenCompositeId"].Value = tokenCompositeId;
            _ = await assocCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return id;
        }

        public static DbCommand CreateTokenizedCorpusInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "CorpusId", "DisplayName", "TokenizationFunction", "ScrVersType", "CustomVersData", "Metadata", "UserId", "Created", "LastTokenized" };

            DataUtil.ApplyColumnsToInsertCommand(command, typeof(Models.TokenizedCorpus), columns);

            command.Prepare();

            return command;
        }

        public static async Task InsertTokenizedCorpusAsync(Models.TokenizedCorpus tokenizedCorpus, DbCommand command, IUserProvider userProvider, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            command.Parameters["@Id"].Value = Guid.Empty != tokenizedCorpus.Id ? tokenizedCorpus.Id : Guid.NewGuid();
            command.Parameters["@CorpusId"].Value = tokenizedCorpus.Corpus?.Id ?? tokenizedCorpus.CorpusId;
            command.Parameters["@DisplayName"].Value = tokenizedCorpus.DisplayName;
            command.Parameters["@TokenizationFunction"].Value = tokenizedCorpus.TokenizationFunction;
            command.Parameters["@ScrVersType"].Value = tokenizedCorpus.ScrVersType;
            command.Parameters["@CustomVersData"].Value = tokenizedCorpus.CustomVersData != null ? tokenizedCorpus.CustomVersData : DBNull.Value;
            command.Parameters["@Metadata"].Value = JsonSerializer.Serialize(tokenizedCorpus.Metadata);
            command.Parameters["@UserId"].Value = Guid.Empty != tokenizedCorpus.UserId ? tokenizedCorpus.UserId : userProvider!.CurrentUser!.Id;
            command.Parameters["@Created"].Value = converter.ConvertToProvider(tokenizedCorpus.Created);
            command.Parameters["@LastTokenized"].Value = converter.ConvertToProvider(tokenizedCorpus.LastTokenized);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateTokenCompositeIdUpdateCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "TokenCompositeId" };
            var whereColumns = new (string, DataUtil.WhereEquality)[] { ("Id", DataUtil.WhereEquality.Equals) };

            DataUtil.ApplyColumnsToUpdateCommand(command, typeof(Models.TokenComponent), columns, whereColumns, Array.Empty<(string, int)>());

            command.Prepare();

            return command;
        }

        public static async Task SetTokenCompositeIdAsync(Guid? tokenCompositeId, Guid tokenComponentId, DbCommand command, CancellationToken cancellationToken)
        {
            command.Parameters["@TokenCompositeId"].Value = (tokenCompositeId is null) ? DBNull.Value : tokenCompositeId;
            command.Parameters["@Id"].Value = tokenComponentId;

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

		public static DbCommand CreateTokenComponentUpdateSurfaceTrainingTextCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { nameof(Models.TokenComponent.SurfaceText), nameof(Models.TokenComponent.TrainingText) };
			var whereColumns = new (string, WhereEquality)[] { (nameof(Models.IdentifiableEntity.Id), WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				typeof(Models.TokenComponent),
				columns,
				whereColumns,
				Array.Empty<(string, int)>());

			command.Prepare();

			return command;
		}

		public static async Task UpdateTokenComponentSurfaceTrainingTextAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(Models.TokenComponent.SurfaceText)}"].Value = tokenComponent.SurfaceText;
			command.Parameters[$"@{nameof(Models.TokenComponent.TrainingText)}"].Value = tokenComponent.TrainingText;
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = tokenComponent.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static async Task SoftDeleteTokenComponentAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			// Keep DbContext model in sync:
			tokenComponent.Deleted ??= DateTimeOffset.UtcNow;

			await DataUtil.SoftDeleteByIdAsync((DateTimeOffset)tokenComponent.Deleted, tokenComponent.Id, command, cancellationToken);
		}

		public static DbCommand CreateTokenComponentTypeUpdateCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { nameof(Models.TokenComponent.Type) };
			var whereColumns = new (string, WhereEquality)[] { (nameof(Models.IdentifiableEntity.Id), WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				typeof(Models.TokenComponent),
				columns,
				whereColumns,
				Array.Empty<(string, int)>());

			command.Prepare();

			return command;
		}

		public static async Task UpdateTypeTokenComponentAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(Models.TokenComponent.Type)}"].Value = tokenComponent.Type;
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = tokenComponent.Id;
			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTokenSubwordRenumberCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { nameof(Models.Token.OriginTokenLocation), nameof(Models.Token.SubwordNumber) };
			var whereColumns = new (string, WhereEquality)[] { (nameof(Models.IdentifiableEntity.Id), WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				typeof(Models.TokenComponent),
				columns,
				whereColumns,
				Array.Empty<(string, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SubwordRenumberTokenAsync(DataAccessLayer.Models.Token token, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(Models.Token.OriginTokenLocation)}"].Value = token.OriginTokenLocation;
			command.Parameters[$"@{nameof(Models.Token.SubwordNumber)}"].Value = token.SubwordNumber;
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = token.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}