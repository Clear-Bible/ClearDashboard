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
    public class GetInternalLexiconQueryHandler : ProjectDbContextQueryHandler<GetInternalLexiconQuery,
        RequestResult<Alignment.Lexicon.Lexicon>, Alignment.Lexicon.Lexicon>
    {
        public GetInternalLexiconQueryHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<GetInternalLexiconQueryHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<Alignment.Lexicon.Lexicon>> GetDataAsync(GetInternalLexiconQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var lexemeModels = await ProjectDbContext.Lexicon_Lexemes
                .Include(e => e.Meanings)
                    .ThenInclude(m => m.User)
                .Include(e => e.Meanings)
                    .ThenInclude(m => m.Translations)
                        .ThenInclude(t => t.User)
                .Include(e => e.Meanings)
                    .ThenInclude(m => m.SemanticDomainMeaningAssociations)
                        .ThenInclude(sda => sda.SemanticDomain)
                            .ThenInclude(sd => sd!.User)
                .Include(e => e.Forms)
                .Include(e => e.User)
                .ToListAsync();

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Queried all database lexemes [result count: {lexemeModels.Count}]");
            sw.Restart();
#endif

            var lexicon = new Alignment.Lexicon.Lexicon(
                lexemeModels
                    .Select(e => ModelHelper.BuildLexeme(e, null, false))
                    .ToList()
            );

            return new RequestResult<Alignment.Lexicon.Lexicon>(lexicon);
        }
    }
}
