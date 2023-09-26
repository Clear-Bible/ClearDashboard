using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class PutMeaningCommandHandler : ProjectDbContextCommandHandler<PutMeaningCommand,
        RequestResult<MeaningId>, MeaningId>
    {
        public PutMeaningCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<PutMeaningCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<MeaningId>> SaveDataAsync(PutMeaningCommand request,
            CancellationToken cancellationToken)
        {
            Models.Lexicon_Meaning? meaning = null;
            if (request.Meaning.MeaningId.Created != null)
            {
                meaning = ProjectDbContext!.Lexicon_Meanings.Include(s => s.User).FirstOrDefault(s => s.Id == request.Meaning.MeaningId.Id);
                if (meaning == null)
                {
                    return new RequestResult<MeaningId>
                    (
                        success: false,
                        message: $"Invalid MeaningId '{request.Meaning.MeaningId.Id}' found in request"
                    );
                }

                meaning.Text = request.Meaning.Text;
                meaning.Language = request.Meaning.Language;
            }
            else
            {
                meaning = new Models.Lexicon_Meaning
                {
                    Id = request.Meaning.MeaningId.Id,
                    Text = request.Meaning.Text,
                    Language = request.Meaning.Language,
                    LexemeId = request.LexemeId.Id
                };

                ProjectDbContext.Lexicon_Meanings.Add(meaning);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            meaning = ProjectDbContext.Lexicon_Meanings
                .Include(s => s.User)
                .First(s => s.Id == meaning.Id);

            return new RequestResult<MeaningId>(ModelHelper.BuildMeaningId(meaning));
        }
    }
}