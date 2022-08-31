using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
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
    public class PutTranslationSetModelEntryCommandHandler : ProjectDbContextCommandHandler<PutTranslationSetModelEntryCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public PutTranslationSetModelEntryCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutTranslationSetTranslationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(PutTranslationSetModelEntryCommand request,
            CancellationToken cancellationToken)
        {
            // FIXME:  IMPLEMENT!

            try
            {
                // need an await to get the compiler to be 'quiet'
                await Task.CompletedTask;

                return new RequestResult<object>(null);
            }
            catch (NullReferenceException e)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: e.Message
                );
            }

        }
    }
}