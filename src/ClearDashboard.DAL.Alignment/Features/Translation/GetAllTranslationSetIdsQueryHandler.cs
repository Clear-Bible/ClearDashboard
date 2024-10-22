﻿using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAllTranslationSetIdsQueryHandler : ProjectDbContextQueryHandler<
        GetAllTranslationSetIdsQuery,
        RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>,
        IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>
    {

        public GetAllTranslationSetIdsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAllTranslationSetIdsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>> GetDataAsync(GetAllTranslationSetIdsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Models.TranslationSet> translationSets = ModelHelper.AddIdIncludesTranslationSetsQuery(ProjectDbContext);
            if (request.ParallelCorpusId != null)
            {
                translationSets = translationSets.Where(ts => ts.ParallelCorpusId == request.ParallelCorpusId.Id);
            }
            if (request.UserId != null)
            {
                translationSets = translationSets.Where(ts => ts.UserId == request.UserId.Id);
            }

            var translationSetIds = translationSets
                .AsEnumerable()   // To avoid error CS8143:  An expression tree may not contain a tuple literal
                .Select(ts => (
                    ModelHelper.BuildTranslationSetId(ts), 
                    ModelHelper.BuildParallelCorpusId(ts.ParallelCorpus!),
                    ModelHelper.BuildUserId(ts.User!)));

            // need an await to get the compiler to be 'quiet'
            await Task.CompletedTask;

            return new RequestResult<IEnumerable<(TranslationSetId translationSetId, ParallelCorpusId parallelCorpusId, UserId userId)>>( translationSetIds.ToList() );
        }
    }


}
