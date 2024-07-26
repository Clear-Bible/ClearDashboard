using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
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

        public static IDictionary<string, IEnumerable<Models.VerseRow>> BuildCorpusVerseRowModels(
            ITextCorpus textCorpus, 
            Guid tokenizedCorpusId, 
            Func<(string BookChapterVerse, Guid TokenizedCorpusId), (Guid verseRowId, Guid userId)> verseRowIdProvider, 
            string displayName,
            IEnumerable<string>? bookIds = null)
        {
            var verseRowsByBook = new Dictionary<string, IEnumerable<Models.VerseRow>>();

            bookIds ??= textCorpus.Texts.Select(t => t.Id).ToList();
            foreach (var bookId in bookIds)
            {
                var tokensTextRows = ExtractValidateBook(
                    textCorpus,
                    bookId,
                    displayName);

                // This method currently doesn't have any way to use the real
                // VerseRowIds when building VerseRows.  So we correct each
                // token's VerseRowId before doing its insert
                var (verseRows, btTokenCount) = BuildVerseRowModel(tokensTextRows, tokenizedCorpusId, verseRowIdProvider);
                verseRowsByBook.Add(bookId, verseRows);
            }

            return verseRowsByBook;
        }

        public static (IEnumerable<Models.VerseRow>, int) BuildVerseRowModel(
            IEnumerable<TokensTextRow> tokensTextRows, 
            Guid tokenizedCorpusId,
            Func<(string BookChapterVerse, Guid TokenizedCorpusId), (Guid verseRowId, Guid userId)> verseRowIdProvider)
        {
            var tokenCount = 0;
            var verseRows = tokensTextRows
                .Where(ttr => ttr.IsEmpty == false)
                .Select(ttr =>
                {
                    var (b, c, v) = (
                        ((VerseRef)ttr.Ref).BookNum,
                        ((VerseRef)ttr.Ref).ChapterNum,
                        ((VerseRef)ttr.Ref).VerseNum);
                    var bookChapterVerse = $"{b:000}{c:000}{v:000}";
                    var (verseRowId, userId) = verseRowIdProvider((bookChapterVerse, tokenizedCorpusId));

                    return new Models.VerseRow
                    {
                        Id = verseRowId,
                        UserId = userId,
                        TokenizedCorpusId = tokenizedCorpusId,
                        BookChapterVerse = bookChapterVerse,
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
												var modelToken = new Models.Token
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
									var modelToken = new Models.Token
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
                })
                .ToList();

            return (verseRows, tokenCount);
        }
        public static DbCommand CreateVerseRowUpdateCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();

            var entityType = metadataModel.ToEntityType(typeof(Models.VerseRow));

            var columns = entityType.ToProperties(new List<string>
            {
                nameof(Models.VerseRow.OriginalText), 
                nameof(Models.VerseRow.IsSentenceStart), 
                nameof(Models.VerseRow.IsInRange), 
                nameof(Models.VerseRow.IsRangeStart), 
                nameof(Models.VerseRow.IsEmpty), 
                nameof(Models.VerseRow.Modified)
            }).ToArray();

            var whereColumns = new (IProperty, DataUtil.WhereEquality)[] { 
                (entityType.ToProperty(nameof(Models.IdentifiableEntity.Id)), DataUtil.WhereEquality.Equals) 
            };

            DataUtil.ApplyColumnsToUpdateCommand(
                command, 
                entityType, 
                columns, 
                whereColumns, 
                Array.Empty<(IProperty, int)>()
            );

            command.Prepare();

            return command;
        }

        public static async Task UpdateVerseRowAsync(Models.VerseRow verseRow, DbCommand command, CancellationToken cancellationToken)
        {
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            setCommandProperty(nameof(Models.VerseRow.Id), verseRow.Id);
            setCommandProperty(nameof(Models.VerseRow.OriginalText), verseRow.OriginalText != null ? verseRow.OriginalText : DBNull.Value);
            setCommandProperty(nameof(Models.VerseRow.IsSentenceStart), verseRow.IsSentenceStart);
            setCommandProperty(nameof(Models.VerseRow.IsInRange), verseRow.IsInRange);
            setCommandProperty(nameof(Models.VerseRow.IsRangeStart), verseRow.IsRangeStart);
            setCommandProperty(nameof(Models.VerseRow.IsEmpty), verseRow.IsEmpty);
            setCommandProperty(nameof(Models.VerseRow.Modified), verseRow.Modified);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateVerseRowInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var entityType = metadataModel.ToEntityType(typeof(Models.VerseRow));
            var insertProperties = entityType.ToProperties(new List<string>
            {
                nameof(Models.VerseRow.Id), 
                nameof(Models.VerseRow.BookChapterVerse), 
                nameof(Models.VerseRow.OriginalText), 
                nameof(Models.VerseRow.TokenizedCorpusId), 
                nameof(Models.VerseRow.IsSentenceStart), 
                nameof(Models.VerseRow.IsInRange), 
                nameof(Models.VerseRow.IsRangeStart), 
                nameof(Models.VerseRow.IsEmpty), 
                nameof(Models.VerseRow.UserId), 
                nameof(Models.VerseRow.Created)
            }).ToArray();

            DataUtil.ApplyColumnsToInsertCommand(command, entityType, insertProperties);

            command.Prepare();

            return command;
        }

        public static async Task InsertVerseRowAsync(Models.VerseRow verseRow, DbCommand command, IUserProvider userProvider, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            object? dt(DateTimeOffset d) => converter.ConvertToProvider(d);
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            setCommandProperty(nameof(Models.VerseRow.Id), verseRow.Id);
            setCommandProperty(nameof(Models.VerseRow.BookChapterVerse), verseRow.BookChapterVerse);
            setCommandProperty(nameof(Models.VerseRow.OriginalText), verseRow.OriginalText != null ? verseRow.OriginalText : DBNull.Value);
            setCommandProperty(nameof(Models.VerseRow.IsSentenceStart), verseRow.IsSentenceStart);
            setCommandProperty(nameof(Models.VerseRow.IsInRange), verseRow.IsInRange);
            setCommandProperty(nameof(Models.VerseRow.IsRangeStart), verseRow.IsRangeStart);
            setCommandProperty(nameof(Models.VerseRow.IsEmpty), verseRow.IsEmpty);
            setCommandProperty(nameof(Models.VerseRow.TokenizedCorpusId), verseRow.TokenizedCorpusId);
            setCommandProperty(nameof(Models.VerseRow.UserId), Guid.Empty != verseRow.UserId ? verseRow.UserId : userProvider!.CurrentUser!.Id);
            setCommandProperty(nameof(Models.VerseRow.Created), dt(verseRow.Created));

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateTokenComponentInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var entityType = metadataModel.ToEntityType(typeof(Models.Token));
            var insertProperties = entityType.ToProperties(new List<string>
            {
                nameof(Models.Token.Id), 
                nameof(Models.Token.EngineTokenId), 
                nameof(Models.Token.TrainingText), 
                nameof(Models.Token.VerseRowId), 
                nameof(Models.Token.TokenizedCorpusId),
				nameof(Models.TokenComposite.ParallelCorpusId),
				nameof(Models.Token.BookNumber), 
                nameof(Models.Token.ChapterNumber),  
                nameof(Models.Token.VerseNumber), 
                nameof(Models.Token.WordNumber), 
                nameof(Models.Token.SubwordNumber), 
                nameof(Models.Token.SurfaceText), 
                nameof(Models.Token.ExtendedProperties),
				nameof(Models.Token.Type),
				nameof(Models.Token.Metadata),
				nameof(Models.Token.GrammarId),
				nameof(Models.Token.CircumfixGroup),
				DbCommandExtensions.DISCRIMINATOR_COLUMN_NAME
            }).ToArray();

            DataUtil.ApplyColumnsToInsertCommand(command, entityType, insertProperties);

            command.Prepare();

            return command;
        }

        public static DbCommand CreateTokenCompositeTokenAssociationInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var entityType = metadataModel.ToEntityType(typeof(Models.TokenCompositeTokenAssociation));
            var insertProperties = entityType.ToProperties(new List<string>
            {
                nameof(Models.TokenCompositeTokenAssociation.Id),
                nameof(Models.TokenCompositeTokenAssociation.TokenId), 
                nameof(Models.TokenCompositeTokenAssociation.TokenCompositeId)
            }).ToArray();

            DataUtil.ApplyColumnsToInsertCommand(command, entityType, insertProperties);

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
        public static async Task InsertTokenAsync(Models.Token token, Guid? tokenCompositeId, DbCommand command, CancellationToken cancellationToken)
        {
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            setCommandProperty(nameof(Models.Token.Id), token.Id);
            setCommandProperty(nameof(Models.Token.EngineTokenId), token.EngineTokenId);
            setCommandProperty(nameof(Models.Token.TrainingText), token.TrainingText);
            setCommandProperty(nameof(Models.Token.VerseRowId), token.VerseRowId);
            setCommandProperty(nameof(Models.Token.TokenizedCorpusId), token.TokenizedCorpusId);
			setCommandProperty(nameof(Models.TokenComposite.ParallelCorpusId), DBNull.Value);
			setCommandProperty(DbCommandExtensions.DISCRIMINATOR_COLUMN_NAME, token.GetType().Name);
            setCommandProperty(nameof(Models.Token.BookNumber), token.BookNumber);
            setCommandProperty(nameof(Models.Token.ChapterNumber), token.ChapterNumber);
            setCommandProperty(nameof(Models.Token.VerseNumber), token.VerseNumber);
            setCommandProperty(nameof(Models.Token.WordNumber), token.WordNumber);
            setCommandProperty(nameof(Models.Token.SubwordNumber), token.SubwordNumber);
            setCommandProperty(nameof(Models.Token.SurfaceText), token.SurfaceText);
			setCommandProperty(nameof(Models.Token.ExtendedProperties), token.ExtendedProperties != null ? token.ExtendedProperties : DBNull.Value);
			setCommandProperty(nameof(Models.Token.Type), token.Type != null ? token.Type : DBNull.Value);
			setCommandProperty(nameof(Models.Token.Metadata), JsonSerializer.Serialize(token.Metadata));
			setCommandProperty(nameof(Models.Token.GrammarId), token.GrammarId != null ? token.GrammarId : DBNull.Value);
			setCommandProperty(nameof(Models.Token.CircumfixGroup), token.CircumfixGroup != null ? token.CircumfixGroup : DBNull.Value);

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        public static async Task InsertTokenCompositeAsync(Models.TokenComposite tokenComposite, DbCommand command, CancellationToken cancellationToken)
        {
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            setCommandProperty(nameof(Models.TokenComposite.Id), tokenComposite.Id);
            setCommandProperty(nameof(Models.TokenComposite.EngineTokenId), tokenComposite.EngineTokenId);
            setCommandProperty(nameof(Models.TokenComposite.TrainingText), tokenComposite.TrainingText);
            setCommandProperty(nameof(Models.TokenComposite.VerseRowId), tokenComposite.VerseRowId);
            setCommandProperty(nameof(Models.TokenComposite.ExtendedProperties), tokenComposite.ExtendedProperties != null ? tokenComposite.ExtendedProperties : DBNull.Value);
            setCommandProperty(nameof(Models.TokenComposite.TokenizedCorpusId), tokenComposite.TokenizedCorpusId);
			setCommandProperty(nameof(Models.TokenComposite.ParallelCorpusId), tokenComposite.ParallelCorpusId != null ? tokenComposite.ParallelCorpusId : DBNull.Value);
			setCommandProperty(DbCommandExtensions.DISCRIMINATOR_COLUMN_NAME, tokenComposite.GetType().Name);
            setCommandProperty(nameof(Models.Token.BookNumber), DBNull.Value);
            setCommandProperty(nameof(Models.Token.ChapterNumber), DBNull.Value);
            setCommandProperty(nameof(Models.Token.VerseNumber), DBNull.Value);
            setCommandProperty(nameof(Models.Token.WordNumber), DBNull.Value);
            setCommandProperty(nameof(Models.Token.SubwordNumber), DBNull.Value);
			setCommandProperty(nameof(Models.TokenComposite.SurfaceText), tokenComposite.SurfaceText);
			setCommandProperty(nameof(Models.TokenComposite.Type), tokenComposite.Type != null ? tokenComposite.Type : DBNull.Value);
			setCommandProperty(nameof(Models.TokenComposite.Metadata), JsonSerializer.Serialize(tokenComposite.Metadata));
			setCommandProperty(nameof(Models.TokenComposite.GrammarId), tokenComposite.GrammarId != null ? tokenComposite.GrammarId : DBNull.Value);
			setCommandProperty(nameof(Models.TokenComposite.CircumfixGroup), tokenComposite.CircumfixGroup != null ? tokenComposite.CircumfixGroup : DBNull.Value);

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        public static async Task<Guid> InsertTokenCompositeTokenAssociationAsync(Guid tokenId, Guid tokenCompositeId, DbCommand command, CancellationToken cancellationToken)
        {
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            var id = Guid.NewGuid();

            setCommandProperty(nameof(Models.TokenCompositeTokenAssociation.Id), id);
            setCommandProperty(nameof(Models.TokenCompositeTokenAssociation.TokenId), tokenId);
            setCommandProperty(nameof(Models.TokenCompositeTokenAssociation.TokenCompositeId), tokenCompositeId);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return id;
        }

        public static DbCommand CreateTokenizedCorpusInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var entityType = metadataModel.ToEntityType(typeof(Models.TokenizedCorpus));
            var insertProperties = entityType.ToProperties(new List<string>
            {
                nameof(Models.TokenizedCorpus.Id), 
                nameof(Models.TokenizedCorpus.CorpusId), 
                nameof(Models.TokenizedCorpus.DisplayName), 
                nameof(Models.TokenizedCorpus.TokenizationFunction), 
                nameof(Models.TokenizedCorpus.ScrVersType), 
                nameof(Models.TokenizedCorpus.CustomVersData), 
                nameof(Models.TokenizedCorpus.Metadata), 
                nameof(Models.TokenizedCorpus.UserId), 
                nameof(Models.TokenizedCorpus.Created), 
                nameof(Models.TokenizedCorpus.LastTokenized), 
            }).ToArray();

            DataUtil.ApplyColumnsToInsertCommand(command, entityType, insertProperties);

            command.Prepare();

            return command;
        }

        public static async Task InsertTokenizedCorpusAsync(Models.TokenizedCorpus tokenizedCorpus, DbCommand command, IUserProvider userProvider, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            object? dt(DateTimeOffset? d) => converter.ConvertToProvider(d);
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            setCommandProperty(nameof(Models.TokenizedCorpus.Id), Guid.Empty != tokenizedCorpus.Id ? tokenizedCorpus.Id : Guid.NewGuid());
            setCommandProperty(nameof(Models.TokenizedCorpus.CorpusId), tokenizedCorpus.Corpus?.Id ?? tokenizedCorpus.CorpusId);
            setCommandProperty(nameof(Models.TokenizedCorpus.DisplayName), tokenizedCorpus.DisplayName);
            setCommandProperty(nameof(Models.TokenizedCorpus.TokenizationFunction), tokenizedCorpus.TokenizationFunction);
            setCommandProperty(nameof(Models.TokenizedCorpus.ScrVersType), tokenizedCorpus.ScrVersType);
            setCommandProperty(nameof(Models.TokenizedCorpus.CustomVersData), tokenizedCorpus.CustomVersData != null ? tokenizedCorpus.CustomVersData : DBNull.Value);
            setCommandProperty(nameof(Models.TokenizedCorpus.Metadata), JsonSerializer.Serialize(tokenizedCorpus.Metadata));
            setCommandProperty(nameof(Models.TokenizedCorpus.UserId), Guid.Empty != tokenizedCorpus.UserId ? tokenizedCorpus.UserId : userProvider!.CurrentUser!.Id);
            setCommandProperty(nameof(Models.TokenizedCorpus.Created), converter.ConvertToProvider(tokenizedCorpus.Created));
            setCommandProperty(nameof(Models.TokenizedCorpus.LastTokenized), dt(tokenizedCorpus.LastTokenized));

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

		public static DbCommand CreateTokenComponentUpdateSurfaceTrainingEngineTokenIdCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenComponent));

			var columns = entityType.ToProperties(new List<string>
			{
				nameof(Models.TokenComponent.SurfaceText),
				nameof(Models.TokenComponent.TrainingText),
				nameof(Models.TokenComponent.EngineTokenId)
			}).ToArray();

			var whereColumns = new (IProperty, DataUtil.WhereEquality)[] {
				(entityType.ToProperty(nameof(Models.IdentifiableEntity.Id)), DataUtil.WhereEquality.Equals)
			};

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task UpdateTokenComponentSurfaceTrainingTextAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(Models.TokenComponent.SurfaceText)}"].Value = tokenComponent.SurfaceText;
			command.Parameters[$"@{nameof(Models.TokenComponent.TrainingText)}"].Value = tokenComponent.TrainingText;
			command.Parameters[$"@{nameof(Models.TokenComponent.EngineTokenId)}"].Value = tokenComponent.EngineTokenId;
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = tokenComponent.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static async Task SoftDeleteMetadataUpdateTokenComponentAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			// Keep DbContext model in sync:
			tokenComponent.Deleted ??= DateTimeOffset.UtcNow;

			await SoftDeleteMetadataUpdateByIdAsync((DateTimeOffset)tokenComponent.Deleted, tokenComponent.Metadata, tokenComponent.Id, command, cancellationToken);
		}

		public static DbCommand CreateSoftDeleteMetadataUpdateByIdCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenComponent));

			var columns = entityType.ToProperties(new List<string>
			{
				nameof(Models.TokenComponent.Deleted),
				nameof(Models.TokenComponent.Metadata)
			}).ToArray();

			var whereColumns = new (IProperty, DataUtil.WhereEquality)[] {
				(entityType.ToProperty(nameof(Models.IdentifiableEntity.Id)), DataUtil.WhereEquality.Equals)
			};

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SoftDeleteMetadataUpdateByIdAsync(DateTimeOffset deleted, List<Models.Metadatum> metadata, Guid id, DbCommand command, CancellationToken cancellationToken)
		{
			var converter = new DateTimeOffsetToBinaryConverter();

			command.Parameters[$"@{nameof(Models.TokenComponent.Deleted)}"].Value = converter.ConvertToProvider(deleted);
			command.Parameters[$"@{nameof(Models.TokenComponent.Metadata)}"].Value = JsonSerializer.Serialize(metadata);
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTokenComponentTypeUpdateCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenComponent));

			var columns = entityType.ToProperties(new List<string>
			{
				nameof(Models.TokenComponent.Type)
			}).ToArray();

			var whereColumns = new (IProperty, DataUtil.WhereEquality)[] {
				(entityType.ToProperty(nameof(Models.IdentifiableEntity.Id)), DataUtil.WhereEquality.Equals)
			};

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task UpdateTypeTokenComponentAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(Models.TokenComponent.Type)}"].Value = tokenComponent.Type;
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = tokenComponent.Id;
			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTokenSubwordRenumberCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenComponent));

			var columns = entityType.ToProperties(new List<string>
			{
				nameof(Models.Token.OriginTokenLocation),
				nameof(Models.Token.SubwordNumber), 
                nameof(Models.Token.EngineTokenId)
			}).ToArray();

			var whereColumns = new (IProperty, DataUtil.WhereEquality)[] {
				(entityType.ToProperty(nameof(Models.IdentifiableEntity.Id)), DataUtil.WhereEquality.Equals)
			};

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SubwordRenumberTokenAsync(DataAccessLayer.Models.Token token, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(Models.Token.OriginTokenLocation)}"].Value = token.OriginTokenLocation;
			command.Parameters[$"@{nameof(Models.Token.SubwordNumber)}"].Value = token.SubwordNumber;
			command.Parameters[$"@{nameof(Models.Token.EngineTokenId)}"].Value = token.EngineTokenId;
			command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = token.Id;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTokenComponentDeleteCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenComponent));

			var whereColumns = new (IProperty, DataUtil.WhereEquality)[] {
				(entityType.ToProperty(nameof(Models.IdentifiableEntity.Id)), DataUtil.WhereEquality.Equals)
			};

			DataUtil.ApplyColumnsToDeleteCommand(
				command,
				entityType,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task DeleteTokenComponentAsync(Models.TokenComponent tokenComponent, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters["@Id"].Value = tokenComponent.Id;
			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}
	}
}