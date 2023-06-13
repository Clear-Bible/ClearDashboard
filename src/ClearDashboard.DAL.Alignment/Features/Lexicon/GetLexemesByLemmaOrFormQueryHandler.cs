using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.EventsAndDelegates;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Features.Lexicon
{
    public class GetLexemesByLemmaOrFormQueryHandler : ProjectDbContextQueryHandler<GetLexemesByLemmaOrFormQuery,
        RequestResult<IEnumerable<Lexeme>>, IEnumerable<Lexeme>>
    {
        public GetLexemesByLemmaOrFormQueryHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<GetLexemesByLemmaOrFormQueryHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Lexeme>>> GetDataAsync(GetLexemesByLemmaOrFormQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var lexemesByLemmaOrForms = await ProjectDbContext.Lexicon_Lexemes
                .Include(e => e.Meanings.Where(m => string.IsNullOrEmpty(request.MeaningLanguage) || m.Language == request.MeaningLanguage))
                    .ThenInclude(m => m.User)
                .Include(e => e.Meanings.Where(m => string.IsNullOrEmpty(request.MeaningLanguage) || m.Language == request.MeaningLanguage))
                    .ThenInclude(m => m.Translations)
                .Include(e => e.Meanings.Where(m => string.IsNullOrEmpty(request.MeaningLanguage) || m.Language == request.MeaningLanguage))
                    .ThenInclude(m => m.SemanticDomainMeaningAssociations)
                        .ThenInclude(sda => sda.SemanticDomain)
                            .ThenInclude(sd => sd!.User)
                .Include(e => e.Forms)
                .Include(e => e.User)
                .Where(e => 
                    (e.Lemma == request.LemmaOrForm || e.Forms.Any(f => f.Text == request.LemmaOrForm)) && 
                    (string.IsNullOrEmpty(request.Language) || e.Language == request.Language))
                .ToListAsync();

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Queried lexemesByLemmaOrForms [result count: {lexemesByLemmaOrForms.Count}]");
            sw.Restart();
#endif

            var lexemes = lexemesByLemmaOrForms
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
                ))
                .ToList();

            return new RequestResult<IEnumerable<Lexeme>>(lexemes);
        }
    }
}
