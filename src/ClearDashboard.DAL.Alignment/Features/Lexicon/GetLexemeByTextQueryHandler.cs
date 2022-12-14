﻿using ClearDashboard.DAL.Alignment.Lexicon;
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
                .Include(l => l.Definitions.Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage))
                    .ThenInclude(d => d.User)
                .Include(l => l.Definitions.Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage))
                    .ThenInclude(d => d.Translations)
                .Include(l => l.Definitions.Where(d => string.IsNullOrEmpty(request.DefinitionLanguage) || d.Language == request.DefinitionLanguage))
                    .ThenInclude(d => d.SemanticDomainDefinitionAssociations)
                        .ThenInclude(sda => sda.SemanticDomain)
                            .ThenInclude(sd => sd!.User)
                .Include(l => l.Forms)
                .Include(l => l.User)
                .Where(l => l.Lemma == request.Lemma && (string.IsNullOrEmpty(request.Language) || l.Language == request.Language))
                .Select(l => new Lexeme(
                    ModelHelper.BuildLexemeId(l),
                    l.Lemma!,
                    l.Language,
                    l.Definitions
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
