﻿using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
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
    public class GetAlignmentSetTargetTokensBySourceTrainingTextQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentSetTargetTokensBySourceTrainingTextQuery,
        RequestResult<IEnumerable<Token>>,
        IEnumerable<Token>>
    {

        public GetAlignmentSetTargetTokensBySourceTrainingTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentSetTargetTokensBySourceTrainingTextQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Token>>> GetDataAsync(GetAlignmentSetTargetTokensBySourceTrainingTextQuery request, CancellationToken cancellationToken)
        {
            return new RequestResult<IEnumerable<Token>>
            (
                await ProjectDbContext.Alignments
                    .Where(a => a.AlignmentSetId == request.AlignmentSetId.Id)
                    .Where(a => a.Deleted == null)
                    .Where(a => a.SourceTokenComponent!.TrainingText == request.SourceTrainingText)
                    .FilterByAlignmentMode(request.AlignmentOriginationFilterMode).AsQueryable()
                    .Select(a => ModelHelper.BuildToken(a.TargetTokenComponent!))
                    .ToListAsync()
            );
        }
    }


}
