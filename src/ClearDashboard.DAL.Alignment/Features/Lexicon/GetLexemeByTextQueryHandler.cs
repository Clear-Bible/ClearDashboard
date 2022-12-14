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
                .Include(l => l.Senses.Where(s => string.IsNullOrEmpty(request.SenseLanguage) || s.Language == request.SenseLanguage))
                    .ThenInclude(d => d.User)
                .Include(l => l.Senses.Where(s => string.IsNullOrEmpty(request.SenseLanguage) || s.Language == request.SenseLanguage))
                    .ThenInclude(d => d.Translations)
                .Include(l => l.Senses.Where(s => string.IsNullOrEmpty(request.SenseLanguage) || s.Language == request.SenseLanguage))
                    .ThenInclude(d => d.SemanticDomainSenseAssociations)
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
                    l.Senses
                        .Where(s => string.IsNullOrEmpty(request.SenseLanguage) || s.Language == request.SenseLanguage)
                        .Select(s => new Sense(
                            ModelHelper.BuildSenseId(s),
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
