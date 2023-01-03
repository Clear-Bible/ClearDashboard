using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class DeleteLexemeAndDependentsCommandHandler : ProjectDbContextCommandHandler<
        DeleteLexemeAndDependentsCommand,
        RequestResult<Unit>, Unit>
    {
        public DeleteLexemeAndDependentsCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DeleteLexemeAndDependentsCommandHandler> logger) : base(
                projectDbContextFactory,
                projectProvider,
                logger)
        {
        }

        protected override async Task<RequestResult<Unit>> SaveDataAsync(DeleteLexemeAndDependentsCommand request,
            CancellationToken cancellationToken)
        {
            var lexeme = ProjectDbContext.Lexicon_Lexemes
                .FirstOrDefault(l => l.Id == request.LexemeId.Id);

            if (lexeme == null)
            {
                return new RequestResult<Unit>
                (
                    success: false,
                    message: $"Invalid LexemeId '{request.LexemeId.Id}' found in request"
                );
            }

            ProjectDbContext.Remove(lexeme);
            _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);

            return new RequestResult<Unit>(Unit.Value);
        }
    }
}