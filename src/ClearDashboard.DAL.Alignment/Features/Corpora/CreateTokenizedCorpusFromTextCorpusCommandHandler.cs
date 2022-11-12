using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using SIL.EventsAndDelegates;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateTokenizedCorpusFromTextCorpusCommandHandler : ProjectDbContextCommandHandler<
        CreateTokenizedCorpusFromTextCorpusCommand,
        RequestResult<TokenizedTextCorpus>,
        TokenizedTextCorpus>
    {
        private readonly IMediator _mediator;

        public CreateTokenizedCorpusFromTextCorpusCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateTokenizedCorpusFromTextCorpusCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<TokenizedTextCorpus>> SaveDataAsync(
            CreateTokenizedCorpusFromTextCorpusCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            //DB Impl notes:
            // 1. creates a new associated TokenizedCorpus (associated with the parent CorpusId provided in the request),
            // 2. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.
            var corpus = ProjectDbContext!.Corpa.FirstOrDefault(c => c.Id == request.CorpusId.Id);

#if DEBUG
            sw.Stop();
#endif

            if (corpus == null)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: $"Invalid CorpusId '{request.CorpusId.Id}' found in request"
                );
            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Insert Tokenized corpus '{request.DisplayName}'");
            sw.Restart();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Logger.LogInformation($"Private memory usage (BEFORE BULK INSERT): {proc.PrivateMemorySize64}");
#endif

            var tokenizedCorpus = new Models.TokenizedCorpus
            {
                Corpus = corpus,
                DisplayName = request.DisplayName,
                TokenizationFunction = request.TokenizationFunction,
                ScrVersType = (int)request.Versification.Type
            };

            if (request.Versification.IsCustomized)
            {
                var writer = new StringWriter();
                request.Versification.Save(writer);

                tokenizedCorpus.CustomVersData = writer.ToString();
            }

            var (tokenizedTextCorpus, tokenCount) = await CreateTokenizedTextCorpus(
                tokenizedCorpus,
                corpus,
                request.TextCorpus,
                request.Versification,
                cancellationToken);

#if DEBUG
            proc.Refresh();
            Logger.LogInformation($"Private memory usage (AFTER BULK INSERT): {proc.PrivateMemorySize64}");

            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end) [token count: {tokenCount}]");
#endif
            return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
        }

        private async Task<(TokenizedTextCorpus, int)> CreateTokenizedTextCorpus(Models.TokenizedCorpus tokenizedCorpus, Models.Corpus corpus, ITextCorpus textCorpus, ScrVers versification, CancellationToken cancellationToken)
        {
            var bookIds = textCorpus.Texts.Select(t => t.Id).ToList();
            var tokenCount = 0;

            //var connectionWasOpen = ProjectDbContext.Database.GetDbConnection().State == ConnectionState.Open; 
            //if (!connectionWasOpen)
            //{
            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            //}

            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                using var tokenizedCorpusInsertCommand = CreateTokenizedCorpusInsertCommand(connection);
                using var verseRowInsertCommand = CreateVerseRowInsertCommand(connection);
                using var tokenComponentInsertCommand = CreateTokenComponentInsertCommand(connection);

                await InsertTokenizedCorpusAsync(tokenizedCorpus, tokenizedCorpusInsertCommand, cancellationToken);
                var tokenizedCorpusId = (Guid)tokenizedCorpusInsertCommand.Parameters["@Id"].Value!;

                foreach (var bookId in bookIds)
                {
                    var ttrs = textCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>().ToList();

                    var dups = ttrs
                        .SelectMany(ttr => ttr.Tokens)
                        .SelectMany(t => (t is CompositeToken token) ? token.Tokens : new List<Token>() { t })
                        .GroupBy(t => t.TokenId)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key);

                    if (dups.Any())
                    {
                        throw new InvalidDataEngineException(name: "Token.Ids", value: $"{string.Join(",", dups)}", message: $"Engine token Id duplicates found in corpus '{corpus.Name}' book '{bookId}'");
                    }

                    var multiVerseSpanningComposites = ttrs
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

                    var (verseRows, btTokenCount) = GetVerseRows(ttrs, tokenizedCorpusId);

                    await InsertVerseRowsAsync(
                            verseRows,
                            verseRowInsertCommand,
                            tokenComponentInsertCommand,
                            cancellationToken);

                    tokenCount += btTokenCount;
                }


                await transaction.CommitAsync(cancellationToken);

                var tokenizedCorpusDb = ModelHelper.AddIdIncludesTokenizedCorpaQuery(ProjectDbContext!)
                    .First(tc => tc.Id == tokenizedCorpusId);
                var tokenizedTextCorpus = new TokenizedTextCorpus(
                    ModelHelper.BuildTokenizedTextCorpusId(tokenizedCorpusDb),
                    new CorpusId(corpus.Id),
                    _mediator,
                    bookIds,
                    versification);

//               var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedTextCorpusId(tokenizedCorpusId));
                return (tokenizedTextCorpus, tokenCount);
            }
            finally
            {
                //if (!connectionWasOpen)
                //{
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
                //}
            }
        }

        private static (IEnumerable<Models.VerseRow>, int) GetVerseRows(IEnumerable<TokensTextRow> ttrs, Guid tokenizedCorpusId)
        {
            var tokenCount = 0;
            var verseRows = ttrs
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
                                        ExtendedProperties = compositeToken.ExtendedProperties,
                                        EngineTokenId = compositeToken.TokenId.ToString(),
                                        Tokens = compositeToken.GetPositionalSortedBaseTokens()
                                            .Select(childToken =>
                                            {
                                                tokenCount++;
                                                return new Models.Token
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
                                            }).ToList()
                                    };
                                }
                                else
                                {
                                    tokenCount++;
                                    return new Models.Token
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
                                }
                            })
                        .ToList()
                    };
                });

            return (verseRows, tokenCount);
        }

        private static DbCommand CreateVerseRowInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "BookChapterVerse", "OriginalText", "TokenizedCorpusId", "IsSentenceStart", "IsInRange", "IsRangeStart", "IsEmpty", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.VerseRow), columns);

            command.Prepare();

            return command;
        }

        private async Task InsertVerseRowAsync(Models.VerseRow verseRow, DbCommand verseRowCmd, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            verseRowCmd.Parameters["@Id"].Value = verseRow.Id;
            verseRowCmd.Parameters["@BookChapterVerse"].Value = verseRow.BookChapterVerse;
            verseRowCmd.Parameters["@OriginalText"].Value = (verseRow.OriginalText != null) ? verseRow.OriginalText : DBNull.Value;
            verseRowCmd.Parameters["@IsSentenceStart"].Value = verseRow.IsSentenceStart;
            verseRowCmd.Parameters["@IsInRange"].Value = verseRow.IsInRange;
            verseRowCmd.Parameters["@IsRangeStart"].Value = verseRow.IsRangeStart;
            verseRowCmd.Parameters["@IsEmpty"].Value = verseRow.IsEmpty;
            verseRowCmd.Parameters["@TokenizedCorpusId"].Value = verseRow.TokenizedCorpusId;
            verseRowCmd.Parameters["@UserId"].Value = Guid.Empty != verseRow.UserId ? verseRow.UserId : ProjectDbContext.UserProvider!.CurrentUser!.Id;
            verseRowCmd.Parameters["@Created"].Value = converter.ConvertToProvider(verseRow.Created);

            _ = await verseRowCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static DbCommand CreateTokenComponentInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "EngineTokenId", "TrainingText", "VerseRowId", "TokenizedCorpusId", "Discriminator", "BookNumber", "ChapterNumber", "VerseNumber", "WordNumber", "SubwordNumber", "SurfaceText", "ExtendedProperties", "TokenCompositeId" };

            ApplyColumnsToCommand(command, typeof(Models.TokenComponent), columns);

            command.Prepare();

            return command;
        }
        private async Task InsertVerseRowsAsync(IEnumerable<Models.VerseRow> verseRows, DbCommand verseRowCmd, DbCommand componentCmd, CancellationToken cancellationToken)
        {
            foreach (var verseRow in verseRows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await InsertVerseRowAsync(verseRow, verseRowCmd, cancellationToken);
                await InsertTokenComponentsAsync(verseRow.TokenComponents, componentCmd, cancellationToken);
            }
        }

        private static async Task InsertTokenComponentsAsync(IEnumerable<Models.TokenComponent> tokenComponents, DbCommand componentCmd, CancellationToken cancellationToken)
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
                    }
                }
                else
                {
                    await InsertTokenAsync((tokenComponent as Models.Token)!, null, componentCmd, cancellationToken);
                }

            }
        }
        private static async Task InsertTokenAsync(Models.Token token, Guid? tokenCompositeId, DbCommand componentCmd, CancellationToken cancellationToken)
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
            componentCmd.Parameters["@ExtendedProperties"].Value = (token.ExtendedProperties != null) ? token.ExtendedProperties : DBNull.Value;
            componentCmd.Parameters["@TokenCompositeId"].Value = (tokenCompositeId != null) ? tokenCompositeId : DBNull.Value;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        private static async Task InsertTokenCompositeAsync(Models.TokenComposite tokenComposite, DbCommand componentCmd, CancellationToken cancellationToken)
        {
            componentCmd.Parameters["@Id"].Value = tokenComposite.Id;
            componentCmd.Parameters["@EngineTokenId"].Value = tokenComposite.EngineTokenId;
            componentCmd.Parameters["@TrainingText"].Value = tokenComposite.TrainingText;
            componentCmd.Parameters["@VerseRowId"].Value = tokenComposite.VerseRowId;
            componentCmd.Parameters["@ExtendedProperties"].Value = (tokenComposite.ExtendedProperties != null) ? tokenComposite.ExtendedProperties : DBNull.Value;
            componentCmd.Parameters["@TokenizedCorpusId"].Value = tokenComposite.TokenizedCorpusId;
            componentCmd.Parameters["@Discriminator"].Value = tokenComposite.GetType().Name;
            componentCmd.Parameters["@BookNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@ChapterNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@VerseNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@WordNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@SubwordNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@SurfaceText"].Value = DBNull.Value;
            componentCmd.Parameters["@TokenCompositeId"].Value = DBNull.Value;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static DbCommand CreateTokenizedCorpusInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "CorpusId", "DisplayName", "TokenizationFunction", "ScrVersType", "CustomVersData", "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.TokenizedCorpus), columns);

            command.Prepare();

            return command;
        }

        private async Task InsertTokenizedCorpusAsync(Models.TokenizedCorpus tokenizedCorpus, DbCommand command, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            command.Parameters["@Id"].Value = (Guid.Empty != tokenizedCorpus.Id) ? tokenizedCorpus.Id : Guid.NewGuid();
            command.Parameters["@CorpusId"].Value = tokenizedCorpus.Corpus?.Id ?? tokenizedCorpus.CorpusId;
            command.Parameters["@DisplayName"].Value = tokenizedCorpus.DisplayName;
            command.Parameters["@TokenizationFunction"].Value = tokenizedCorpus.TokenizationFunction;
            command.Parameters["@ScrVersType"].Value = tokenizedCorpus.ScrVersType;
            command.Parameters["@CustomVersData"].Value = tokenizedCorpus.CustomVersData != null ? tokenizedCorpus.CustomVersData : DBNull.Value;
            command.Parameters["@Metadata"].Value = JsonSerializer.Serialize(tokenizedCorpus.Metadata);
            command.Parameters["@UserId"].Value = Guid.Empty != tokenizedCorpus.UserId ? tokenizedCorpus.UserId : ProjectDbContext.UserProvider!.CurrentUser!.Id;
            command.Parameters["@Created"].Value = converter.ConvertToProvider(tokenizedCorpus.Created);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static void ApplyColumnsToCommand(DbCommand command, Type type, string[] columns)
        {
            command.CommandText =
            $@"
                INSERT INTO {type.Name} ({string.Join(", ", columns)})
                VALUES ({string.Join(", ", columns.Select(c => "@" + c))})
            ";

            foreach (var column in columns)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{column}";
                command.Parameters.Add(parameter);
            }
        }
    }
}