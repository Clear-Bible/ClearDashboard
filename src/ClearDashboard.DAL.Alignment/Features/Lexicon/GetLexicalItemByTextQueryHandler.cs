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
    public class GetLexicalItemByTextQueryHandler : ProjectDbContextQueryHandler<GetLexicalItemByTextQuery,
        RequestResult<LexicalItem?>, LexicalItem?>
    {
        public GetLexicalItemByTextQueryHandler(
            ProjectDbContextFactory? projectDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<GetLexicalItemByTextQueryHandler> logger) : base(projectDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<LexicalItem?>> GetDataAsync(GetLexicalItemByTextQuery request, CancellationToken cancellationToken)
        {
            var lexicalItem = ProjectDbContext.Lexicon_LexicalItems
                .Include(li => li.Definitions.Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage))
                    .ThenInclude(d => d.User)
                .Include(li => li.Definitions.Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage))
                    .ThenInclude(d => d.Translations)
                .Include(li => li.Definitions.Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage))
                    .ThenInclude(d => d.SemanticDomainDefinitionAssociations)
                        .ThenInclude(sda => sda.SemanticDomain)
                            .ThenInclude(sd => sd!.User)
                .Include(li => li.Forms)
                .Include(li => li.User)
                .Where(li => li.Lemma == request.Lemma && (string.IsNullOrEmpty(request.Language) || li.Language == request.Language))
                .Select(li => new LexicalItem(
                    ModelHelper.BuildLexicalItemId(li),
                    li.Lemma!,
                    li.Language,
                    li.Definitions
                        .Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage)
                        .Select(d => new Definition(
                            ModelHelper.BuildDefinitionId(d),
                            d.Text!,
                            d.Language,
                            d.Translations.Select(t => new Alignment.Lexicon.Translation(
                                ModelHelper.BuildTranslationId(t),
                                t.Text ?? string.Empty
                            )).ToList(),
                            d.SemanticDomains.Select(sd => new SemanticDomain(
                                ModelHelper.BuildSemanticDomainId(sd),
                                sd.Text!
                            )).ToList()
                    )).ToList(),
                    li.Forms.Select(lis => new Form(
                        new FormId(lis.Id),
                        lis.Text!
                    )).ToList()
                )).FirstOrDefault();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<LexicalItem?>(lexicalItem);
        }
    }
}
