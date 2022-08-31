using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;

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
            var translationSetModel = new Models.TranslationSet();
            translationSetModel.ParallelCorpusId = request.ParallelCorpusId.Id;
            //translationSetModel.DerivedFrom =
            //translationSetModel.EngineWordAlignmentId =
            //translationSetModel.TranslationModel =

            try
            {
                  
            }
            catch (NullReferenceException e)
            {
                return new RequestResult<Alignment.Translation.TranslationSet>
                (
                    success: false,
                    message: e.Message
                );
            }

            //ProjectDbContext.TranslationSets.Add(translationSetModel);
            //await ProjectDbContext.SaveChangesAsync();

            return new RequestResult<TranslationSet>(new TranslationSet(
                new TranslationSetId(new Guid()), 
                request.ParallelCorpusId, 
                request.TranslationModel, 
                _mediator));
        }
    }
}