using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using DalStuff = ClearDashboard.DAL.Alignment.Corpora;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class CreateParallelCorpusCommandHandler : ProjectDbContextCommandHandler<CreateParallelCorpusCommand,
        RequestResult<ParallelCorpus>, ParallelCorpus>
    {
        private readonly IMediator _mediator;

        public CreateParallelCorpusCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateParallelCorpusCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<ParallelCorpus>> SaveDataAsync(CreateParallelCorpusCommand request,
            CancellationToken cancellationToken)
        {
            // Code Smell: This handler is doing too many things.
            //DB Impl notes:
            //1. Create a new record in ParallelCorpus, save ParallelCorpusId
            //2. Create VerseMappings with verses
            //3. return created ParallelCorpus based on ParallelCorpusId
            //var parallelCorpus = await ParallelCorpus.Get(_mediator, new ParallelCorpusId(new Guid()));

            // Create and Save the Parallel Corpus Model
            // + with Verse Mappings
            var parallelCorpusModel = new Models.ParallelCorpus
            {
                SourceTokenizedCorpusId = request.SourceTokenizedCorpusId.Id,
                TargetTokenizedCorpusId = request.TargetTokenizedCorpusId.Id,
                VerseMappings = request.VerseMappings.Select(engineMapping => new Models.VerseMapping()).ToList(),
            };

            ProjectDbContext.ParallelCorpa.Add(parallelCorpusModel);
            await ProjectDbContext.SaveChangesAsync();

            // Create a `ParallelCorpus` to be returned
            var sourceTokenizedCorpus = ProjectDbContext.TokenizedCorpora
                .Include(tc => tc.Corpus)
                .Include(tc => tc.Tokens)
                .FirstOrDefault(tc => tc.Id == request.SourceTokenizedCorpusId.Id);

            var targetTokenizedCorpus = ProjectDbContext.TokenizedCorpora
                .Include(tc => tc.Corpus)
                .Include(tc => tc.Tokens)
                .FirstOrDefault(tc => tc.Id == request.TargetTokenizedCorpusId.Id);

            if (sourceTokenizedCorpus == null || targetTokenizedCorpus == null ||
                sourceTokenizedCorpus.Corpus == null || targetTokenizedCorpus.Corpus == null)
            {
                return new RequestResult<ParallelCorpus>(
                    result: null,
                    success: false,
                    message: "Something went horribly wrong.");
            }

            var sourceTokenizedCorpusId =
                new DalStuff.TokenizedCorpusId(sourceTokenizedCorpus.Id);
            var targetTokenizedCorpusId =
                new DalStuff.TokenizedCorpusId(targetTokenizedCorpus.Id);

            var sourceCorpusId = new DalStuff.CorpusId(sourceTokenizedCorpus.Corpus.Id);
            var targetCorpusId = new DalStuff.CorpusId(targetTokenizedCorpus.Corpus.Id);

            var sourceTokenizedTextCorpus =
                new TokenizedTextCorpus(sourceTokenizedCorpusId, sourceCorpusId, _mediator,
                    FilterBookAbbreviationsForTokens(sourceTokenizedCorpus.Tokens));
            var targetTokenizedTextCorpus =
                new TokenizedTextCorpus(targetTokenizedCorpusId, targetCorpusId, _mediator,
                    FilterBookAbbreviationsForTokens(targetTokenizedCorpus.Tokens));

            // Type confusion: If this `ParallelCorpus` is a domain object, can we change the naming convention?
            // Perhaps models should be plain nouns (i.e. `ParallelCorpus`)
            // ... and domain models can have a verb in the name (i.e. `?`)
            // ... or *something*?
            var parallelCorpusId = new DalStuff.ParallelCorpusId(parallelCorpusModel.Id);

            return new RequestResult<ParallelCorpus>
            (result: new DalStuff.ParallelCorpus(sourceTokenizedTextCorpus,
                targetTokenizedTextCorpus, request.VerseMappings, parallelCorpusId)
            );
        }

        private IEnumerable<string> FilterBookAbbreviationsForTokens(IEnumerable<Models.Token> tokens)
        {
            var distinctBookInts = tokens
                .Select(t => t.BookNumber)
                .Distinct();

            var distinctBookAbbreviations = distinctBookInts.Select(
                bookInt => FileGetBookIds.BookIds.Find(bId => bId.clearTreeBookNum == bookInt.ToString())
            ).Select(bookId => bookId.silCannonBookAbbrev);

            return distinctBookAbbreviations;
        }
    }
}