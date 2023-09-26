using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;

//USE TO ACCESS Vocabulary
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class PutFormCommandHandler : ProjectDbContextCommandHandler<PutFormCommand,
        RequestResult<FormId>, FormId>
    {
        public PutFormCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<PutFormCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<FormId>> SaveDataAsync(PutFormCommand request,
            CancellationToken cancellationToken)
        {
            Models.Lexicon_Form? form = null;
            if (request.Form.FormId.Created != null)
            {
                form = ProjectDbContext!.Lexicon_Forms.FirstOrDefault(f => f.Id == request.Form.FormId.Id);
                if (form == null)
                {
                    return new RequestResult<FormId>
                    (
                        success: false,
                        message: $"Invalid FormId '{request.Form.FormId.Id}' found in request"
                    );
                }

                form.Text = request.Form.Text;
            }
            else
            {
                form = new Models.Lexicon_Form
                {
                    Id = request.Form.FormId.Id,
                    Text = request.Form.Text,
                    LexemeId = request.LexemeId.Id
                };

                ProjectDbContext.Lexicon_Forms.Add(form);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            form = ProjectDbContext.Lexicon_Forms
                .Include(t => t.User)
                .First(t => t.Id == form.Id);

            return new RequestResult<FormId>(ModelHelper.BuildFormId(form));
        }
    }
}