using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Extensions;

using ModelCorpusType = ClearDashboard.DataAccessLayer.Models.CorpusType;
using ModelCorpus = ClearDashboard.DataAccessLayer.Models.Corpus;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateCorpusCommandHandler : ProjectDbContextCommandHandler<CreateCorpusCommand,
        RequestResult<Corpus>, Corpus>
    {
        private readonly IMediator _mediator;

        public CreateCorpusCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateCorpusCommandHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Corpus>> SaveDataAsync(
            CreateCorpusCommand request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif
            //DB Impl notes:
            // 1. creates a new Corpus

            var modelCorpus = new ModelCorpus
            {
                IsRtl = request.IsRtl,
                Name = request.Name,
                Language = request.Language,
                ParatextGuid = request.ParatextId,
            };

            if (Enum.TryParse<ModelCorpusType>(request.CorpusType, out ModelCorpusType corpusType))
            {
                modelCorpus.CorpusType = corpusType;
            } 
            else
            {
                modelCorpus.CorpusType = ModelCorpusType.Unknown;
            }

            ProjectDbContext.Corpa.Add(modelCorpus);

            try
            {
                // NB:  passing in the cancellation token to SaveChangesAsync.
                await ProjectDbContext.SaveChangesAsync(cancellationToken);
            } 
            catch (OperationCanceledException)
            {
#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (canceled)");
#endif
                return new RequestResult<Corpus>
                (
                    success: false,
                    message: "Operation canceled",
                    canceled: true
                );
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Save corpus (end)");
            sw.Restart();
#endif

            var corpus = await Corpus.Get(_mediator, ModelHelper.BuildCorpusId(modelCorpus));

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

            return new RequestResult<Corpus>(corpus);
        }
    }
}