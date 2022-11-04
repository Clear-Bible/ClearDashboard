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
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using SIL.Machine.Corpora;

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

            try
            {
                var (tokenizedTextCorpus, tokenCount) = await CreateTokenizedTextCorpus(
                    tokenizedCorpus,
                    corpus,
                    request.TextCorpus,
                    cancellationToken);

#if DEBUG
                proc.Refresh();
                Logger.LogInformation($"Private memory usage (AFTER BULK INSERT): {proc.PrivateMemorySize64}");

                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end) [token count: {tokenCount}]");
#endif
                return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
            }
            catch (OperationCanceledException)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: "Operation canceled",
                    canceled: true
                );
            }
            catch (Exception ex)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: $"Error saving tokenized corpus / tokens to database '{ex}'"
                );
            }
        }

        private async Task<(TokenizedTextCorpus, int)> CreateTokenizedTextCorpus(Models.TokenizedCorpus tokenizedCorpus, Models.Corpus corpus, ITextCorpus textCorpus, CancellationToken cancellationToken)
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
                using var tokenComponentInsertCommand = CreateTokenComponentInsertCommand(connection);

                await InsertTokenizedCorpusAsync(tokenizedCorpus, tokenizedCorpusInsertCommand, cancellationToken);
                var tokenizationId = (Guid)tokenizedCorpusInsertCommand.Parameters["@Id"].Value!;

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

                    var (bookTokens, btTokenCount) = GetBookTokens(ttrs, tokenizationId);

                    await InsertTokenComponentsAsync(
                            bookTokens,
                            tokenComponentInsertCommand,
                            cancellationToken);

                    tokenCount += btTokenCount;
                }

                await transaction.CommitAsync(cancellationToken);

                var tokenizedCorpusDb = ModelHelper.AddIdIncludesTokenizedCorpaQuery(ProjectDbContext!)
                    .First(tc => tc.Id == tokenizationId);
                var tokenizedTextCorpus = new TokenizedTextCorpus(
                    ModelHelper.BuildTokenizedTextCorpusId(tokenizedCorpusDb),
                    new CorpusId(corpus.Id),
                    _mediator,
                    bookIds);

//               var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedTextCorpusId(tokenizationId));

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

        private static (IEnumerable<Models.TokenComponent>, int) GetBookTokens(IEnumerable<TokensTextRow> ttrs, Guid tokenizationId)
        {
            var tokenCount = 0;
            var bookTokens = ttrs
                .SelectMany(ttr => ttr.Tokens)
                .Select(token =>
                {
                    if (token is CompositeToken compositeToken)
                    {
                        tokenCount++;
                        return new Models.TokenComposite
                        {
                            Id = compositeToken.TokenId.Id,
                            TokenizationId = tokenizationId,
                            TrainingText = compositeToken.TrainingText,
                            EngineTokenId = compositeToken.TokenId.ToString(),
                            Tokens = compositeToken.GetPositionalSortedBaseTokens()
                                .Select(childToken =>
                                {
                                    tokenCount++;
                                    return new Models.Token
                                    {
                                        Id = childToken.TokenId.Id,
                                        TokenizationId = tokenizationId,
                                        TrainingText = childToken.TrainingText,
                                        EngineTokenId = childToken.TokenId.ToString(),
                                        BookNumber = childToken.TokenId.BookNumber,
                                        ChapterNumber = childToken.TokenId.ChapterNumber,
                                        VerseNumber = childToken.TokenId.VerseNumber,
                                        WordNumber = childToken.TokenId.WordNumber,
                                        SubwordNumber = childToken.TokenId.SubWordNumber,
                                        SurfaceText = childToken.SurfaceText,
                                        PropertiesJson = childToken.ExtendedProperties
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
                            TokenizationId = tokenizationId,
                            TrainingText = token.TrainingText,
                            EngineTokenId = token.TokenId.ToString(),
                            BookNumber = token.TokenId.BookNumber,
                            ChapterNumber = token.TokenId.ChapterNumber,
                            VerseNumber = token.TokenId.VerseNumber,
                            WordNumber = token.TokenId.WordNumber,
                            SubwordNumber = token.TokenId.SubWordNumber,
                            SurfaceText = token.SurfaceText,
                            PropertiesJson = token.ExtendedProperties
                        } as Models.TokenComponent;
                    }
                });

            return (bookTokens, tokenCount);
        }

        private static DbCommand CreateTokenComponentInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "EngineTokenId", "TrainingText", "TokenizationId", "Discriminator", "BookNumber", "ChapterNumber", "VerseNumber", "WordNumber", "SubwordNumber", "SurfaceText", "PropertiesJson", "TokenCompositeId" };

            ApplyColumnsToCommand(command, typeof(Models.TokenComponent), columns);

            command.Prepare();

            return command;
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
            componentCmd.Parameters["@TokenizationId"].Value = token.TokenizationId;
            componentCmd.Parameters["@Discriminator"].Value = token.GetType().Name;
            componentCmd.Parameters["@BookNumber"].Value = token.BookNumber;
            componentCmd.Parameters["@ChapterNumber"].Value = token.ChapterNumber;
            componentCmd.Parameters["@VerseNumber"].Value = token.VerseNumber;
            componentCmd.Parameters["@WordNumber"].Value = token.WordNumber;
            componentCmd.Parameters["@SubwordNumber"].Value = token.SubwordNumber;
            componentCmd.Parameters["@SurfaceText"].Value = token.SurfaceText;
            componentCmd.Parameters["@PropertiesJson"].Value = (token.PropertiesJson != null) ? token.PropertiesJson : DBNull.Value;
            componentCmd.Parameters["@TokenCompositeId"].Value = (tokenCompositeId != null) ? tokenCompositeId : DBNull.Value;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        private static async Task InsertTokenCompositeAsync(Models.TokenComposite tokenComposite, DbCommand componentCmd, CancellationToken cancellationToken)
        {
            componentCmd.Parameters["@Id"].Value = tokenComposite.Id;
            componentCmd.Parameters["@EngineTokenId"].Value = tokenComposite.EngineTokenId;
            componentCmd.Parameters["@TrainingText"].Value = tokenComposite.TrainingText;
            componentCmd.Parameters["@TokenizationId"].Value = tokenComposite.TokenizationId;
            componentCmd.Parameters["@Discriminator"].Value = tokenComposite.GetType().Name;
            componentCmd.Parameters["@BookNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@ChapterNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@VerseNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@WordNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@SubwordNumber"].Value = DBNull.Value;
            componentCmd.Parameters["@SurfaceText"].Value = DBNull.Value;
            componentCmd.Parameters["@PropertiesJson"].Value = DBNull.Value;
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