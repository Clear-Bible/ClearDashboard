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
    public class PutLexicalItemSurfaceTextCommandHandler : LexiconDbContextCommandHandler<PutLexicalItemSurfaceTextCommand,
        RequestResult<LexicalItemSurfaceTextId>, LexicalItemSurfaceTextId>
    {
        public PutLexicalItemSurfaceTextCommandHandler(
            LexiconDbContextFactory? lexiconDbContextFactory, 
            ILogger<PutLexicalItemSurfaceTextCommandHandler> logger) : base(lexiconDbContextFactory,
            logger)
        {
        }

        protected override async Task<RequestResult<LexicalItemSurfaceTextId>> SaveDataAsync(PutLexicalItemSurfaceTextCommand request,
            CancellationToken cancellationToken)
        {
            Models.LexicalItemSurfaceText? lexicalItemSurfaceText = null;
            if (request.LexicalItemSurfaceText.LexicalItemSurfaceTextId != null)
            {
                lexicalItemSurfaceText = LexiconDbContext!.LexicalItemSurfaceTexts.FirstOrDefault(lid => lid.Id == request.LexicalItemSurfaceText.LexicalItemSurfaceTextId.Id);
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

                LexiconDbContext.LexicalItemSurfaceTexts.Add(lexicalItemSurfaceText);
            }

            _ = await LexiconDbContext!.SaveChangesAsync(cancellationToken);
            return new RequestResult<LexicalItemSurfaceTextId>(new LexicalItemSurfaceTextId(lexicalItemSurfaceText.Id));
        }
    }
}