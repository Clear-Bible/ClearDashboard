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
                TokenizationFunction = request.TokenizationFunction
            };

            // ITextCorpus Text ids always book ids/abbreviations:  
            var bookIds = request.TextCorpus.Texts.Select(t => t.Id).ToList();
            var tokenizationId = Guid.Empty;

            var connectionWasOpen = ProjectDbContext.Database.GetDbConnection().State == ConnectionState.Open; 
            if (!connectionWasOpen)
            {
                await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            }
           
            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var transaction = await ProjectDbContext.Database.GetDbConnection().BeginTransactionAsync(cancellationToken);

                using var tokenizedCorpusInsertCommand = CreateTokenizedCorpusInsertCommand();
                using var tokenInsertCommand = CreateTokenInsertCommand();

                await InsertTokenizedCorpusAsync(tokenizedCorpus, tokenizedCorpusInsertCommand, cancellationToken);
                tokenizationId = (Guid)tokenizedCorpusInsertCommand.Parameters["@Id"].Value!;

                foreach (var bookId in bookIds)
                {
                    var bookTokens = request.TextCorpus.GetRows(new List<string>() { bookId }).Cast<TokensTextRow>()
                        .SelectMany(ttr => ttr.Tokens)
                        .SelectMany(token =>
                        {
                            if (token is CompositeToken compositeToken)
                            {
                                var tokenCompositeId = Guid.NewGuid();
                                return compositeToken.GetPositionalSortedBaseTokens()
                                    .Select(childToken => new Models.Token
                                    {
                                        TokenizationId = tokenizationId,
                                        BookNumber = childToken.TokenId.BookNumber,
                                        ChapterNumber = childToken.TokenId.ChapterNumber,
                                        VerseNumber = childToken.TokenId.VerseNumber,
                                        WordNumber = childToken.TokenId.WordNumber,
                                        SubwordNumber = childToken.TokenId.SubWordNumber,
                                        SurfaceText = childToken.SurfaceText,
                                        TrainingText = childToken.TrainingText,
                                        TokenCompositeId = tokenCompositeId
                                    });
                            }
                            else
                            {
                                return new List<Models.Token>() {
                                new Models.Token
                                {
                                    TokenizationId = tokenizationId,
                                    BookNumber = token.TokenId.BookNumber,
                                    ChapterNumber = token.TokenId.ChapterNumber,
                                    VerseNumber = token.TokenId.VerseNumber,
                                    WordNumber = token.TokenId.WordNumber,
                                    SubwordNumber = token.TokenId.SubWordNumber,
                                    SurfaceText = token.SurfaceText,
                                    TrainingText = token.TrainingText,
                                    TokenCompositeId = null
                                }
                                };
                            }
                        });

                    var invalidComposites = bookTokens
                        .Where(bt => bt.TokenCompositeId != null)
                        .GroupBy(bt => bt.TokenCompositeId)
                        .Select(gc => gc
                            .GroupBy(ct => new { ct.ChapterNumber, ct.VerseNumber }))
                            .Where(gct => gct.Count() > 1)
                            .Select(gct => string.Join("-", gct
                                .SelectMany(gc => gc
                                    .Select(t => new TokenId(t.BookNumber, t.ChapterNumber, t.VerseNumber, t.WordNumber, t.SubwordNumber).ToString()))));

                    if (invalidComposites.Any())
                    {
                        return new RequestResult<TokenizedTextCorpus>
                        (
                            success: false,
                            message: $"Invalid CompositeToken(s) found in request.  GetTokensByTokenizedCorpusIdAndBookIdHandler requires all Tokens of a Composite to have the same chapter and verse numbers.  {string.Join(", ", invalidComposites)}"
                        );
                    }

                    await InsertTokensAsync(bookTokens, tokenInsertCommand, cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

//                var tokenizedTextCorpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedCorpusId(tokenizedCorpus.Id));
                var tokenizedTextCorpus = new TokenizedTextCorpus(
                    new TokenizedTextCorpusId(tokenizationId),
                    request.CorpusId,
                    _mediator,
                    bookIds);

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
                if (!connectionWasOpen)
                {
                    await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
                }
            }
        }

        private DbCommand CreateTokenInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "TokenizationId", "BookNumber", "ChapterNumber", "VerseNumber", "WordNumber", "SubwordNumber", "SurfaceText", "TrainingText", "TokenCompositeId" };

            ApplyColumnsToCommand(command, typeof(Models.Token), columns);

            return command;
        }

        private static async Task InsertTokensAsync(IEnumerable<Models.Token> tokens, DbCommand command, CancellationToken cancellationToken)
        {
            foreach (var token in tokens)
            {
                command.Parameters["@Id"].Value = (Guid.Empty != token.Id) ? token.Id : Guid.NewGuid();
                command.Parameters["@TokenizationId"].Value = token.Tokenization?.Id ?? token.TokenizationId;
                command.Parameters["@BookNumber"].Value = token.BookNumber;
                command.Parameters["@ChapterNumber"].Value = token.ChapterNumber;
                command.Parameters["@VerseNumber"].Value = token.VerseNumber;
                command.Parameters["@WordNumber"].Value = token.WordNumber;
                command.Parameters["@SubwordNumber"].Value = token.SubwordNumber;
                command.Parameters["@SurfaceText"].Value = token.SurfaceText;
                command.Parameters["@TrainingText"].Value = token.TrainingText;
                command.Parameters["@TokenCompositeId"].Value = (token.TokenCompositeId != null) ? token.TokenCompositeId : DBNull.Value;

                _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private DbCommand CreateTokenizedCorpusInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "CorpusId", "TokenizationFunction", "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.TokenizedCorpus), columns);

            return command;
        }

        private async Task InsertTokenizedCorpusAsync(Models.TokenizedCorpus tokenizedCorpus, DbCommand command, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            command.Parameters["@Id"].Value = (Guid.Empty != tokenizedCorpus.Id) ? tokenizedCorpus.Id : Guid.NewGuid();
            command.Parameters["@CorpusId"].Value = tokenizedCorpus.Corpus?.Id ?? tokenizedCorpus.CorpusId;
            command.Parameters["@TokenizationFunction"].Value = tokenizedCorpus.TokenizationFunction;
            command.Parameters["@Metadata"].Value = JsonSerializer.Serialize(tokenizedCorpus.Metadata);
            command.Parameters["@UserId"].Value = Guid.Empty != tokenizedCorpus.UserId ? tokenizedCorpus.UserId : ProjectDbContext.UserProvider!.CurrentUser!.Id;
            command.Parameters["@Created"].Value = converter.ConvertToProvider(tokenizedCorpus.Created);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private void ApplyColumnsToCommand(DbCommand command, Type type, string[] columns)
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