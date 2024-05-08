using Autofac;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Data;
using System.Diagnostics;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateTokenizedCorpusFromTextCorpusCommandHandler : ProjectDbContextCommandHandler<
        CreateTokenizedCorpusFromTextCorpusCommand,
        RequestResult<TokenizedTextCorpus>,
        TokenizedTextCorpus>
    {
        private readonly IComponentContext _context;

        public CreateTokenizedCorpusFromTextCorpusCommandHandler(IComponentContext context,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateTokenizedCorpusFromTextCorpusCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
			_context = context;
        }

        protected override async Task<RequestResult<TokenizedTextCorpus>> SaveDataAsync(
            CreateTokenizedCorpusFromTextCorpusCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
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
                TokenizationFunction = request.TokenizationFunction
            };

            if (TokenizedTextCorpus.FixedTokenizedCorpusIdsByCorpusType.TryGetValue(corpus.CorpusType, out Guid tokenizedCorpusId))
            {
                tokenizedCorpus.Id = tokenizedCorpusId;
            }

            tokenizedCorpus.LastTokenized = tokenizedCorpus.Created;

            if (request.Versification.IsCustomized)
            {
                tokenizedCorpus.ScrVersType = (int)request.Versification.BaseVersification.Type;
                using (var writer = new StringWriter())
                {
                    request.Versification.Save(writer);
                    tokenizedCorpus.CustomVersData = writer.ToString();
                }
            }
            else
            {
                tokenizedCorpus.ScrVersType = (int)request.Versification.Type;
            }

            var (tokenizedTextCorpus, tokenCount) = await CreateTokenizedTextCorpus(
                tokenizedCorpus,
                request.CorpusId,
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

        private async Task<(TokenizedTextCorpus, int)> CreateTokenizedTextCorpus(Models.TokenizedCorpus tokenizedCorpus, CorpusId corpusId, ITextCorpus textCorpus, ScrVers versification, CancellationToken cancellationToken)
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

                using var tokenizedCorpusInsertCommand = TokenizedCorpusDataBuilder.CreateTokenizedCorpusInsertCommand(connection);
                using var verseRowInsertCommand = TokenizedCorpusDataBuilder.CreateVerseRowInsertCommand(connection);
                using var tokenComponentInsertCommand = TokenizedCorpusDataBuilder.CreateTokenComponentInsertCommand(connection);
                using var tokenCompositeTokenAssociationInsertCommand = TokenizedCorpusDataBuilder.CreateTokenCompositeTokenAssociationInsertCommand(connection);

                await TokenizedCorpusDataBuilder.InsertTokenizedCorpusAsync(tokenizedCorpus, tokenizedCorpusInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                var tokenizedCorpusId = (Guid)tokenizedCorpusInsertCommand.Parameters["@Id"].Value!;

                foreach (var bookId in bookIds)
                {
                    var tokensTextRows = TokenizedCorpusDataBuilder.ExtractValidateBook(textCorpus, bookId, corpusId.Name);
                    var (verseRows, btTokenCount) = TokenizedCorpusDataBuilder.BuildVerseRowModel(tokensTextRows, tokenizedCorpusId);

                    foreach (var verseRow in verseRows)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await TokenizedCorpusDataBuilder.InsertVerseRowAsync(verseRow, verseRowInsertCommand, ProjectDbContext.UserProvider!, cancellationToken);
                        await TokenizedCorpusDataBuilder.InsertTokenComponentsAsync(verseRow.TokenComponents, tokenComponentInsertCommand, tokenCompositeTokenAssociationInsertCommand, cancellationToken);
                    }
                }

                await transaction.CommitAsync(cancellationToken);

                var tokenizedCorpusDb = ModelHelper.AddIdIncludesTokenizedCorpaQuery(ProjectDbContext!)
                    .First(tc => tc.Id == tokenizedCorpusId);
                var tokenizedTextCorpus = new TokenizedTextCorpus(
                    ModelHelper.BuildTokenizedTextCorpusId(tokenizedCorpusDb),
                    _context,
                    bookIds,
                    versification,
                    false,
                    false);

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
    }
}