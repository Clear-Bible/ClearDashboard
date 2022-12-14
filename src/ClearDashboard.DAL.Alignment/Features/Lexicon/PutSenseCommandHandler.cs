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
    public class PutSenseCommandHandler : ProjectDbContextCommandHandler<PutSenseCommand,
        RequestResult<SenseId>, SenseId>
    {
        public PutSenseCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<PutSenseCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<SenseId>> SaveDataAsync(PutSenseCommand request,
            CancellationToken cancellationToken)
        {
            Models.Lexicon_Sense? sense = null;
            if (request.Sense.SenseId != null)
            {
                sense = ProjectDbContext!.Lexicon_Senses.Include(s => s.User).FirstOrDefault(s => s.Id == request.Sense.SenseId.Id);
                if (sense == null)
                {
                    return new RequestResult<SenseId>
                    (
                        success: false,
                        message: $"Invalid SenseId '{request.Sense.SenseId.Id}' found in request"
                    );
                }

                sense.Text = request.Sense.Text;
                sense.Language = request.Sense.Language;
            }
            else
            {
                sense = new Models.Lexicon_Sense
                {
                    Id = request.Sense.SenseId?.Id ?? Guid.NewGuid(),
                    Text = request.Sense.Text,
                    Language = request.Sense.Language,
                    LexemeId = request.LexemeId.Id
                };

                ProjectDbContext.Lexicon_Senses.Add(sense);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            sense = ProjectDbContext.Lexicon_Senses
                .Include(s => s.User)
                .First(s => s.Id == sense.Id);

            return new RequestResult<SenseId>(ModelHelper.BuildSenseId(sense));
        }
    }
}