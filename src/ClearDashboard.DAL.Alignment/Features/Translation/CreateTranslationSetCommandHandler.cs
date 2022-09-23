using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class CreateTranslationSetCommandHandler : ProjectDbContextCommandHandler<CreateTranslationSetCommand,
        RequestResult<TranslationSet>, TranslationSet>
    {
        private readonly IMediator _mediator;

        public CreateTranslationSetCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateTranslationSetCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<TranslationSet>> SaveDataAsync(CreateTranslationSetCommand request,
            CancellationToken cancellationToken)
        {
            var parallelCorpus = ProjectDbContext!.ParallelCorpa.FirstOrDefault(c => c.Id == request.ParallelCorpusId.Id);
            if (parallelCorpus == null)
            {
                return new RequestResult<TranslationSet>
                (
                    success: false,
                    message: $"Invalid ParallelCorpusId '{request.ParallelCorpusId.Id}' found in request"
                );
            }

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            try
            {
                var translationSetModel = new Models.TranslationSet
                {
                    ParallelCorpusId = request.ParallelCorpusId.Id,
                    DisplayName = request.DisplayName,
                    SmtModel = request.SmtModel,
                    Metadata = request.Metadata,
                    //DerivedFrom = ,
                    //EngineWordAlignment = ,
                    TranslationModel = request.TranslationModel
                        .Select(tm => new Models.TranslationModelEntry
                        {
                            SourceText = tm.Key,
                            TargetTextScores = tm.Value
                                .Select(tts => new Models.TranslationModelTargetTextScore
                                {
                                    Text = tts.Key,
                                    Score = tts.Value
                                }).ToList()
                        }).ToList()
                };

                ProjectDbContext.TranslationSets.Add(translationSetModel);
                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

                return new RequestResult<TranslationSet>(new TranslationSet(
                    ModelHelper.BuildTranslationSetId(translationSetModel),
                    ModelHelper.BuildParallelCorpusId(translationSetModel.ParallelCorpus!),
                    _mediator));

            }
            catch (Exception e)
            {
                return new RequestResult<Alignment.Translation.TranslationSet>
                (
                    success: false,
                    message: e.Message
                );
            }
        }
    }
}