using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Notes;
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
    public class PutLexicalItemSurfaceTextCommandHandler : ProjectDbContextCommandHandler<PutLexicalItemSurfaceTextCommand,
        RequestResult<LexicalItemSurfaceTextId>, LexicalItemSurfaceTextId>
    {
        public PutLexicalItemSurfaceTextCommandHandler(
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<PutLexicalItemSurfaceTextCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
        }

        protected override async Task<RequestResult<LexicalItemSurfaceTextId>> SaveDataAsync(PutLexicalItemSurfaceTextCommand request,
            CancellationToken cancellationToken)
        {
            Models.LexicalItemSurfaceText? lexicalItemSurfaceText = null;
            if (request.LexicalItemSurfaceText.LexicalItemSurfaceTextId != null)
            {
                lexicalItemSurfaceText = ProjectDbContext!.LexicalItemSurfaceTexts.FirstOrDefault(lid => lid.Id == request.LexicalItemSurfaceText.LexicalItemSurfaceTextId.Id);
                if (lexicalItemSurfaceText == null)
                {
                    return new RequestResult<LexicalItemSurfaceTextId>
                    (
                        success: false,
                        message: $"Invalid LexicalItemSurfaceTextId '{request.LexicalItemSurfaceText.LexicalItemSurfaceTextId.Id}' found in request"
                    );
                }

                lexicalItemSurfaceText.SurfaceText = request.LexicalItemSurfaceText.SurfaceText;
            }
            else
            {
                lexicalItemSurfaceText = new Models.LexicalItemSurfaceText
                {
                    Id = request.LexicalItemSurfaceText.LexicalItemSurfaceTextId?.Id ?? Guid.NewGuid(),
                    SurfaceText = request.LexicalItemSurfaceText.SurfaceText,
                    LexicalItemId = request.LexicalItemId.Id
                };

                ProjectDbContext.LexicalItemSurfaceTexts.Add(lexicalItemSurfaceText);
            }

            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
            return new RequestResult<LexicalItemSurfaceTextId>(new LexicalItemSurfaceTextId(lexicalItemSurfaceText.Id));
        }
    }
}