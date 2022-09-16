using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.Common;
using System.Linq;
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
            //DB Impl notes:
            // 1. creates a new associated TokenizedCorpus (associated with the parent CorpusId provided in the request),
            // 2. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.
            var corpus = ProjectDbContext!.Corpa.FirstOrDefault(c => c.Id == request.CorpusId.Id);
            if (corpus == null)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: $"Invalid CorpusId '{request.CorpusId.Id}' found in request"
                );
            }

            var tokenizedCorpus = new TokenizedCorpus
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

            // ITextCorpus Text ids always book ids/abbreviations:  
            var bookIds = request.TextCorpus.Texts.Select(t => t.Id).ToList();
            var tokenizationId = Guid.Empty;

            //var connectionWasOpen = ProjectDbContext.Database.GetDbConnection().State == ConnectionState.Open; 
            //if (!connectionWasOpen)
            //{
                await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            //}
           
            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var transaction = await ProjectDbContext.Database.GetDbConnection().BeginTransactionAsync(cancellationToken);

                using var tokenizedCorpusInsertCommand = CreateTokenizedCorpusInsertCommand();
                using var tokenComponentInsertCommand = CreateTokenComponentInsertCommand(); 

                await InsertTokenizedCorpusAsync(tokenizedCorpus, tokenizedCorpusInsertCommand, cancellationToken);
                tokenizationId = (Guid)tokenizedCorpusInsertCommand.Parameters["@Id"].Value!;

                foreach (var bookId in bookIds)
                {
                    var bookTokens = request.TextCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>()
                        .SelectMany(ttr => ttr.Tokens)
                        .Select(token =>
                        {
                            if (token is CompositeToken compositeToken)
                            {
                                return new TokenComposite
                                {
                                    Id = compositeToken.TokenId.Id,
                                    TokenizationId = tokenizationId,
                                    TrainingText = compositeToken.TrainingText,
                                    EngineTokenId = compositeToken.TokenId.ToString(),
                                    Tokens = compositeToken.GetPositionalSortedBaseTokens()
                                        .Select(childToken => new Models.Token
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
                                            SurfaceText = childToken.SurfaceText
                                        }).ToList()
                                };
                            }
                            else
                            {
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
                                    SurfaceText = token.SurfaceText
                                } as TokenComponent;
                            }
                        });

                        await InsertTokenComponentsAsync(
                            bookTokens, 
                            tokenComponentInsertCommand, 
                            cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

                var tokenizedCorpusDb = ProjectDbContext!.TokenizedCorpora.First(tc => tc.Id == tokenizationId);
                var tokenizedTextCorpus = new TokenizedTextCorpus(
                    ModelHelper.BuildTokenizedTextCorpusId(tokenizedCorpusDb),
                    request.CorpusId,
                    _mediator,
                    bookIds);

 //               var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedTextCorpusId(tokenizationId));

                return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
            }
            catch (Exception ex)
            {
                return new RequestResult<TokenizedTextCorpus>
                (
                    success: false,
                    message: $"Error saving tokenized corpus / tokens to database '{ex.ToString()}'"
                );
            }
            finally
            {
                //if (!connectionWasOpen)
                //{
                    await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
                //}
            }
        }

        private DbCommand CreateTokenComponentInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "EngineTokenId", "TrainingText", "TokenizationId", "Discriminator", "BookNumber", "ChapterNumber", "VerseNumber", "WordNumber", "SubwordNumber", "SurfaceText", "TokenCompositeId" };

            ApplyColumnsToCommand(command, typeof(Models.TokenComponent), columns);

            return command;
        }

        private static async Task InsertTokenComponentsAsync(IEnumerable<TokenComponent> tokenComponents, DbCommand componentCmd, CancellationToken cancellationToken)
        {
            foreach (var tokenComponent in tokenComponents)
            {
                if (tokenComponent is TokenComposite)
                {
                    var tokenComposite = (tokenComponent as TokenComposite)!;
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
            componentCmd.Parameters["@TokenCompositeId"].Value = (tokenCompositeId != null) ? tokenCompositeId : DBNull.Value;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        private static async Task InsertTokenCompositeAsync(TokenComposite tokenComposite, DbCommand componentCmd, CancellationToken cancellationToken)
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
            componentCmd.Parameters["@TokenCompositeId"].Value = DBNull.Value;
            _ = await componentCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private DbCommand CreateTokenizedCorpusInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "CorpusId", "DisplayName", "TokenizationFunction", "ScrVersType", "CustomVersData", "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.TokenizedCorpus), columns);

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