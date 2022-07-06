using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;


namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class UpdateParallelCorpusCommandHandler : ProjectDbContextCommandHandler<UpdateParallelCorpusCommand, RequestResult<ParallelCorpus>, ParallelCorpus>
    {

        public UpdateParallelCorpusCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateParallelCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
        }

        protected override async Task<RequestResult<ParallelCorpus>> SaveDataAsync(UpdateParallelCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. find parallelCorpus based on request.ParallelCorpusId
            //2. Update the verse mappings

            var parallelCorpus = await ParallelCorpus.Get(null, new ParallelCorpusId(new Guid()));


            return new RequestResult<ParallelCorpus>
                    (result: parallelCorpus,
                    success: true,
                    message: "successful result from test");
        }
    }
}
