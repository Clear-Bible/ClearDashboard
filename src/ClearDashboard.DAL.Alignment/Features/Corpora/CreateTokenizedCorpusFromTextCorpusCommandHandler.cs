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
        private readonly ITokenizedCorpusDataCreator _tokenizedCorpusDataCreator;

        public CreateTokenizedCorpusFromTextCorpusCommandHandler(IComponentContext context,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateTokenizedCorpusFromTextCorpusCommandHandler> logger,
            ITokenizedCorpusDataCreator tokenizedCorpusDataCreator)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
			_context = context;
            _tokenizedCorpusDataCreator = tokenizedCorpusDataCreator;
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
                CorpusId = corpus.Id,
                DisplayName = request.DisplayName,
                TokenizationFunction = request.TokenizationFunction
            };

            if (TokenizedTextCorpus.FixedTokenizedCorpusIdsByCorpusType.TryGetValue(corpus.CorpusType, out Guid tokenizedCorpusId))
            {
                tokenizedCorpus.Id = tokenizedCorpusId;
            }
            else
            {
                tokenizedCorpus.Id = Guid.NewGuid();
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

            var tokenizedTextCorpus = await DataUtil.BulkWriteToDatabaseTransactional<(int, int), TokenizedTextCorpus>(
                ProjectDbContext,
                async (connection, metadataModel, cancellationToken) => await _tokenizedCorpusDataCreator.BulkWriteTokenizedCorpusToDatabaseAsync(
                    connection, 
                    metadataModel, 
                    tokenizedCorpus, 
                    request.TextCorpus, 
                    cancellationToken),
                async (projectDbContext, bulkWriteCounts) => {
#if DEBUG
                    proc.Refresh();
                    Logger.LogInformation($"Private memory usage (AFTER BULK INSERT): {proc.PrivateMemorySize64}");

                    Logger.LogInformation($"Elapsed={sw.Elapsed} - tokenized corpus verse rows {bulkWriteCounts.Item1} and tokens inserted: {bulkWriteCounts.Item2}]");
#endif
                    var tokenizedCorpusDb = ModelHelper.AddIdIncludesTokenizedCorpaQuery(ProjectDbContext!)
                        .First(tc => tc.Id == tokenizedCorpus.Id);

                    var tokenizedTextCorpus = new TokenizedTextCorpus(
                        ModelHelper.BuildTokenizedTextCorpusId(tokenizedCorpusDb),
                        _context,
                        request.TextCorpus.Texts.Select(t => t.Id).ToList(),
                        request.Versification,
                        false,
                        false);

                    return await Task.FromResult(tokenizedTextCorpus);
                },
                cancellationToken
            );

            return new RequestResult<TokenizedTextCorpus>(tokenizedTextCorpus);
        }
    }
}