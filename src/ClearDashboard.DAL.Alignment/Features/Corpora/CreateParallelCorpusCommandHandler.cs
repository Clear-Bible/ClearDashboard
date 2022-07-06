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
    public class CreateParallelCorpusCommandHandler : ProjectDbContextCommandHandler<CreateParallelCorpusCommand, RequestResult<ParallelCorpus>, ParallelCorpus>
    {

        public CreateParallelCorpusCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateParallelCorpusCommandHandler> logger) : base(projectNameDbContextFactory,projectProvider,  logger)
        {
        }

        protected override async Task<RequestResult<ParallelCorpus>> SaveDataAsync(CreateParallelCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            //1. Create a new record in ParallelCorpus, save ParallelCorpusId
            //2. Create VerseMappings with verses
            //3. return created ParallelCorpus based on ParallelCorpusId

            var parallelCorpus = await ParallelCorpus.Get(null, new ParallelCorpusId(new Guid()));


            return new RequestResult<ParallelCorpus>
                    (result: parallelCorpus,
                    success: true,
                    message: "successful result from test");
        }
    }
}
