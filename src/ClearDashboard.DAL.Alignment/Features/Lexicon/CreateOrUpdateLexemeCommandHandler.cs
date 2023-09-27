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
    public class CreateOrUpdateLexemeCommandHandler : ProjectDbContextCommandHandler<CreateOrUpdateLexemeCommand,
        RequestResult<LexemeId>, LexemeId>
    {
        public CreateOrUpdateLexemeCommandHandler(
            ProjectDbContextFactory? projectDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<CreateOrUpdateLexemeCommandHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<LexemeId>> SaveDataAsync(CreateOrUpdateLexemeCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                Models.Lexicon_Lexeme? lexeme = null;
                if (request.LexemeId.Created is not null)
                {
                    lexeme = ProjectDbContext!.Lexicon_Lexemes.FirstOrDefault(li => li.Id == request.LexemeId.Id);
                    if (lexeme == null)
                    {
                        return new RequestResult<LexemeId>
                        (
                            success: false,
                            message: $"Invalid LexemeId '{request.LexemeId.Id}' found in request"
                        );
                    }

                    lexeme.Lemma = request.Lemma;
                    lexeme.Language = request.Language;
                    lexeme.Type = request.Type;
                }
                else
                {
                    lexeme = new Models.Lexicon_Lexeme
                    {
                        Id = request.LexemeId.Id,
                        Lemma = request.Lemma,
                        Language = request.Language,
                        Type = request.Type
                    };
                }

                ProjectDbContext.Lexicon_Lexemes.Add(lexeme);

                _ = await ProjectDbContext!.SaveChangesAsync(cancellationToken);
                lexeme = ProjectDbContext.Lexicon_Lexemes.Include(n => n.User).First(li => li.Id == lexeme.Id);

                return new RequestResult<LexemeId>(ModelHelper.BuildLexemeId(lexeme));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is not null)
                {
                    return new RequestResult<LexemeId>
                    (
                        success: false,
                        message: $"{ex.InnerException.Message}"
                    );
                }
                else
                {
                    return new RequestResult<LexemeId>
                    (
                        success: false,
                        message: $"{ex.Message}"
                    );
                }
            }
        }
    }
}