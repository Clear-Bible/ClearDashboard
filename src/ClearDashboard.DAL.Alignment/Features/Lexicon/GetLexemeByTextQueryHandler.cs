using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class GetLexemeByTextQueryHandler : ProjectDbContextQueryHandler<GetLexemeByTextQuery,
        RequestResult<Lexeme?>, Lexeme?>
    {
        public GetLexemeByTextQueryHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<GetLexemeByTextQueryHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<Lexeme?>> GetDataAsync(GetLexemeByTextQuery request, CancellationToken cancellationToken)
        {
            var lexeme = ProjectDbContext.Lexicon_Lexemes
                .Include(l => l.Meanings.Where(s => string.IsNullOrEmpty(request.MeaningLanguage) || s.Language == request.MeaningLanguage))
                    .ThenInclude(d => d.User)
                .Include(l => l.Meanings.Where(s => string.IsNullOrEmpty(request.MeaningLanguage) || s.Language == request.MeaningLanguage))
                    .ThenInclude(d => d.Translations)
                        .ThenInclude(t => t.User)
                .Include(l => l.Meanings.Where(s => string.IsNullOrEmpty(request.MeaningLanguage) || s.Language == request.MeaningLanguage))
                    .ThenInclude(d => d.SemanticDomainMeaningAssociations)
                        .ThenInclude(sda => sda.SemanticDomain)
                            .ThenInclude(sd => sd!.User)
                .Include(l => l.Forms)
                .Include(l => l.User)
                .Where(l => l.Lemma == request.Lemma && (string.IsNullOrEmpty(request.Language) || l.Language == request.Language))
                .Select(l => new Lexeme(
                    ModelHelper.BuildLexemeId(l),
                    l.Lemma!,
                    l.Language,
                    l.Type,
                    l.Meanings
                        .Where(s => string.IsNullOrEmpty(request.MeaningLanguage) || s.Language == request.MeaningLanguage)
                        .Select(s => new Meaning(
                            ModelHelper.BuildMeaningId(s),
                            s.Text!,
                            s.Language,
                            s.Translations.Select(t => new Alignment.Lexicon.Translation(
                                ModelHelper.BuildTranslationId(t),
                                t.Text ?? string.Empty
                            )).ToList(),
                            s.SemanticDomains.Select(sd => new SemanticDomain(
                                ModelHelper.BuildSemanticDomainId(sd),
                                sd.Text!
                            )).ToList()
                    )).ToList(),
                    l.Forms.Select(f => new Form(
                        new FormId(f.Id),
                        f.Text!
                    )).ToList()
                )).FirstOrDefault();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<Lexeme?>(lexeme);
        }
    }
}
