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

            var bookAbbreviationsToIds =
                FileGetBookIds.BookIds.ToDictionary(x => x.silCannonBookAbbrev, x => int.Parse(x.silCannonBookNum));

            // FIXME:  handle TokenVerseAssociations!

            parallelCorpusModel.VerseMappings.AddRange(request.VerseMappings
                .Select(vm =>
                {
                    var verseMapping = new Models.VerseMapping
                    {
                        ParallelCorpus = parallelCorpusModel
                    };
                    verseMapping.VerseMappingVerseAssociations.AddRange(vm.SourceVerses
                        .Select(v => new Models.VerseMappingVerseAssociation
                        {
                            Verse = new Models.Verse
                            {
                                VerseNumber = v.VerseNum,
                                BookNumber = bookAbbreviationsToIds[v.Book],
                                ChapterNumber = v.ChapterNum,
                                CorpusId = sourceTokenizedCorpus!.CorpusId
                            }
                        }));
                    verseMapping.VerseMappingVerseAssociations.AddRange(vm.TargetVerses
                        .Select(v => new Models.VerseMappingVerseAssociation
                        {
                            Verse = new Models.Verse
                            {
                                VerseNumber = v.VerseNum,
                                BookNumber = bookAbbreviationsToIds[v.Book],
                                ChapterNumber = v.ChapterNum,
                                CorpusId = targetTokenizedCorpus!.CorpusId
                            }
                        }));
                    return verseMapping;
                })
            );

            ProjectDbContext.ParallelCorpa.Add(parallelCorpusModel);
            await ProjectDbContext.SaveChangesAsync();

            var parallelCorpus = await ParallelCorpus.Get(_mediator, new ParallelCorpusId(parallelCorpusModel.Id));

            return new RequestResult<ParallelCorpus>(parallelCorpus);
        }
    }
}