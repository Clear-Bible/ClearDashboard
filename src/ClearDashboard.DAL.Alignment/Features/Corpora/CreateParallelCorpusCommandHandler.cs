using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SIL.Extensions;

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
            //DB Impl notes:
            //1. Create a new record in ParallelCorpus, save ParallelCorpusId
            //2. Create VerseMappings with verses
            //3. return created ParallelCorpus based on ParallelCorpusId
            //var parallelCorpus = await ParallelCorpus.Get(_mediator, new ParallelCorpusId(new Guid()));

            var sourceTokenizedCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.SourceTokenizedCorpusId.Id);
            var targetTokenizedCorpus = ProjectDbContext.TokenizedCorpora.FirstOrDefault(tc => tc.Id == request.TargetTokenizedCorpusId.Id);

            if (sourceTokenizedCorpus == null || targetTokenizedCorpus == null)
            {
                return new RequestResult<ParallelCorpus>
                (
                    success: false,
                    message: sourceTokenizedCorpus == null ?
                        $"SourceTokenizedCorpus not found for TokenizedCorpusId {request.SourceTokenizedCorpusId.Id}" :
                        $"TargetTokenizedCorpus not found for TokenizedCorpusId {request.TargetTokenizedCorpusId.Id}"
                );
            }

            // Create and Save the Parallel Corpus Model
            // + with Verse Mappings
            var parallelCorpusModel = new Models.ParallelCorpus
            {
                SourceTokenizedCorpus = sourceTokenizedCorpus,
                TargetTokenizedCorpus = targetTokenizedCorpus
            };

            var bookAbbreviationsToNumbers =
                FileGetBookIds.BookIds.ToDictionary(x => x.silCannonBookAbbrev, x => int.Parse(x.silCannonBookNum), StringComparer.OrdinalIgnoreCase);

            // FIXME:  handle TokenVerseAssociations!

            try
            {
                parallelCorpusModel.VerseMappings.AddRange(request.VerseMappings
                    .Select(vm =>
                    {
                        var verseMapping = new Models.VerseMapping
                        {
                            ParallelCorpus = parallelCorpusModel
                        };
                        verseMapping.Verses.AddRange(vm.SourceVerses
                            .Select(v => {
                                int bookNumber;
                                if (!bookAbbreviationsToNumbers.TryGetValue(v.Book, out bookNumber))
                                {
                                    throw new NullReferenceException($"Invalid book '{v.Book}' found in engine source verse. ");
                                }
                                return new Models.Verse
                                {
                                    VerseNumber = v.VerseNum,
                                    BookNumber = bookNumber,
                                    ChapterNumber = v.ChapterNum,
                                    CorpusId = sourceTokenizedCorpus!.CorpusId
                                };
                            }
                            ));
                        verseMapping.Verses.AddRange(vm.TargetVerses
                            .Select(v => {
                                int bookNumber;
                                if (!bookAbbreviationsToNumbers.TryGetValue(v.Book, out bookNumber))
                                {
                                    throw new NullReferenceException($"Invalid book '{v.Book}' found in engine target verse. ");
                                }
                                return new Models.Verse
                                {
                                    VerseNumber = v.VerseNum,
                                    BookNumber = bookNumber,
                                    ChapterNumber = v.ChapterNum,
                                    CorpusId = targetTokenizedCorpus!.CorpusId
                                };
                            }
                            ));
                        return verseMapping;
                    })
                );
            }
            catch (NullReferenceException e)
            {
                return new RequestResult<ParallelCorpus>
                (
                    success: false,
                    message: e.Message
                );
            }

            ProjectDbContext.ParallelCorpa.Add(parallelCorpusModel);
            await ProjectDbContext.SaveChangesAsync();

            var parallelCorpus = await ParallelCorpus.Get(_mediator, new ParallelCorpusId(parallelCorpusModel.Id));

            return new RequestResult<ParallelCorpus>(parallelCorpus);
        }
    }
}