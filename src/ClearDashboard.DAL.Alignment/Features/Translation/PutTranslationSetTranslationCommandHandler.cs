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
    public class PutTranslationSetTranslationCommandHandler : ProjectDbContextCommandHandler<PutTranslationSetTranslationCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public PutTranslationSetTranslationCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutTranslationSetTranslationCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(PutTranslationSetTranslationCommand request,
            CancellationToken cancellationToken)
        {
            var translationSet = ProjectDbContext!.TranslationSets
                .Include(ts => ts.Translations
                    .OrderBy(t => t.Token!.BookNumber)
                    .OrderBy(t => t.Token!.ChapterNumber)
                    .OrderBy(t => t.Token!.VerseNumber)
                    .OrderBy(t => t.Token!.WordNumber)
                    .OrderBy(t => t.Token!.SubwordNumber))
                .FirstOrDefault(c => c.Id == request.TranslationSetId.Id);
            if (translationSet == null)
            {
                return new RequestResult<object>
                (
                    success: false,
                    message: $"Invalid TranslationSetId '{request.TranslationSetId.Id}' found in request"
                );
            }

            try
            {
 //               translationSet.Translations.

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