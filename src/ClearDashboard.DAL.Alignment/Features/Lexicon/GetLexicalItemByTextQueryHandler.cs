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
        private readonly IMediator _mediator;

        public GetLexicalItemByTextQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetLexicalItemByTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<LexicalItem?>> GetDataAsync(GetLexicalItemByTextQuery request, CancellationToken cancellationToken)
        {
            var lexicalItem = ProjectDbContext.LexicalItems
                .Include(li => li.LexicalItemDefinitions.Where(lid => string.IsNullOrEmpty(request.DefinitionLanguage) || lid.Language == request.DefinitionLanguage))
                    .ThenInclude(lid => lid.User)
                .Include(li => li.LexicalItemDefinitions.Where(lid => string.IsNullOrEmpty(request.DefinitionLanguage) || lid.Language == request.DefinitionLanguage))
                    .ThenInclude(lid => lid.SemanticDomainLexicalItemDefinitionAssociations)
                        .ThenInclude(sda => sda.SemanticDomain)
                            .ThenInclude(sd => sd!.User)
                .Include(li => li.LexicalItemSurfaceTexts)
                .Include(li => li.User)
                .Where(li => li.TrainingText == request.TrainingText && li.Language == request.Language)
                .Select(li => new LexicalItem(
                    ModelHelper.BuildLexicalItemId(li),
                    li.TrainingText!,
                    li.Language,
                    li.LexicalItemDefinitions.Select(lid => new LexicalItemDefinition(
                        ModelHelper.BuildLexicalItemDefinitionId(lid),
                        lid.TrainingText!,
                        lid.Language,
                        lid.SemanticDomains.Select(sd => new SemanticDomain(
                            ModelHelper.BuildSemanticDomainId(sd),
                            sd.Text!
                        )).ToList()
                    )).ToList(),
                    li.LexicalItemSurfaceTexts.Select(lis => new LexicalItemSurfaceText(
                        new LexicalItemSurfaceTextId(lis.Id),
                        lis.SurfaceText!
                    )).ToList()
                )).FirstOrDefault();

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<LexicalItem?>(lexicalItem);
        }
    }
}
