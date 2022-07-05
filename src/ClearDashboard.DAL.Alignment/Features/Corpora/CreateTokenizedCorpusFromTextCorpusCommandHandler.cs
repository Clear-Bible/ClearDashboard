using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateTokenizedCorpusFromTextCorpusCommandHandler : AlignmentDbContextCommandHandler<
        CreateTokenizedCorpusFromTextCorpusCommand,
        RequestResult<TokenizedTextCorpus>,
        TokenizedTextCorpus>
    {
        private readonly IMediator _mediator;

        public CreateTokenizedCorpusFromTextCorpusCommandHandler(IMediator mediator, ProjectNameDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<CreateTokenizedCorpusFromTextCorpusCommandHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }
       
        protected override Task<RequestResult<TokenizedTextCorpus>> SaveData(CreateTokenizedCorpusFromTextCorpusCommand request, CancellationToken cancellationToken)
        {
            //DB Impl notes:
            // 1. creates a new Corpus,
            // 2. creates a new associated CorpusVersion,
            // 3. creates a new associated TokenizedCorpus,
            // 4. then iterates through command.TextCorpus, casting to TokensTextRow, extracting tokens, and inserting associated to TokenizedCorpus into the Tokens table.
            //Assert.All(command.TextCorpus, tc => Assert.IsType<TokensTextRow>(tc));

            return Task.FromResult(
                new RequestResult<TokenizedTextCorpus>
                (result: Task.Run(() => TokenizedTextCorpus.Get(request.ProjectName, _mediator, new TokenizedCorpusId(new Guid())), cancellationToken).GetAwaiter().GetResult(),
                    //run async from sync like constructor: good desc. https://stackoverflow.com/a/40344759/13880559
                    success: true,
                    message: "successful result from test"));
        }
    }

}
