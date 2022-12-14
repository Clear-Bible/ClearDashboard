using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteLexicalItemAndDependentsCommandHandler : ProjectDbContextCommandHandler<
        DeleteLexicalItemAndDependentsCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteLexicalItemAndDependentsCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteLexicalItemAndDependentsCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteLexicalItemAndDependentsCommand request,
            CancellationToken cancellationToken)
        {
            var lexicalItem = ProjectDbContext.Lexicon_LexicalItems
                .FirstOrDefault(li => li.Id == request.LexicalItemId.Id);

            if (lexicalItem == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid LexicalItemId '{request.LexicalItemId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(lexicalItem);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}