using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetParallelCorpusByParallelCorpusIdQueryHandler : ProjectDbContextQueryHandler<
        GetParallelCorpusByParallelCorpusIdQuery,
        RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
            TokenizedCorpusId targetTokenizedCorpusId,
            IEnumerable<VerseMapping> verseMappings,
            ParallelCorpusId parallelCorpusId)>,
        (TokenizedCorpusId sourceTokenizedCorpusId,
        TokenizedCorpusId targetTokenizedCorpusId,
        IEnumerable<VerseMapping> verseMappings,
        ParallelCorpusId parallelCorpusId)>
    {

        public GetParallelCorpusByParallelCorpusIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetParallelCorpusByParallelCorpusIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId, 
            TokenizedCorpusId targetTokenizedCorpusId, 
            IEnumerable<VerseMapping> verseMappings, 
            ParallelCorpusId parallelCorpusId)>> GetDataAsync(GetParallelCorpusByParallelCorpusIdQuery request, CancellationToken cancellationToken)

        {
            //DB Impl notes: use command.ParallelCorpusId to retrieve from ParallelCorpus table and return
            //1. the result of gathering all the VerseMappings to build an VerseMapping list.
            //2. associated source and target TokenizedCorpusId

#nullable disable
            // FIXME:  added the #nullable disable to keep studio from complaining - I think - about
            // how vm.Verses might be null.  Is it really possible for VerseMappings to have null Verses?
            var parallelCorpus =
                ProjectDbContext.ParallelCorpa
                    .Include(pc => pc.SourceTokenizedCorpus)
                    .Include(pc => pc.TargetTokenizedCorpus)
                    .Include(pc => pc.VerseMappings)
                    .ThenInclude(vm => vm.Verses)
                    .ThenInclude(v => v.TokenVerseAssociations)
                    .ThenInclude(tva => tva.Token)
                    .FirstOrDefault(pc => pc.Id == request.ParallelCorpusId.Id);
#nullable restore

            var invalidArgMsg = "";
            if (parallelCorpus == null)
            {
                invalidArgMsg = $"ParallelCorpus not found for ParallelCorpusId {request.ParallelCorpusId.Id}";
            }

            if (parallelCorpus!.SourceTokenizedCorpus == null || parallelCorpus.TargetTokenizedCorpus == null)
            {
                invalidArgMsg = $"ParallelCorpus {request.ParallelCorpusId.Id} has null source or target tokenized corpus";
            }

            if (!string.IsNullOrEmpty(invalidArgMsg))
            {
                return new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                    TokenizedCorpusId targetTokenizedCorpusId,
                    IEnumerable<VerseMapping> verseMappings,
                    ParallelCorpusId parallelCorpusId)>
                (
                    success: false,
                    message: invalidArgMsg
                );
            }

            var bookNumbersToAbbreviations =
                FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum), x => x.silCannonBookAbbrev);

            var sourceCorpusId = parallelCorpus.SourceTokenizedCorpus!.CorpusId;
            var targetCorpusId = parallelCorpus.TargetTokenizedCorpus!.CorpusId;

            try
            {
                var verseMappings = parallelCorpus.VerseMappings
                    .Where(vm => vm.Verses != null)
                    .Select(vm =>
                    {
                        var sourceVerses = vm.Verses!
                            .Where(v => v.CorpusId == sourceCorpusId)
                            .Select(v => ValidateBuildVerse(v, bookNumbersToAbbreviations));
                        var targetVerses = vm.Verses!
                            .Where(v => v.CorpusId == targetCorpusId)
                            .Select(v => ValidateBuildVerse(v, bookNumbersToAbbreviations));

                        return new VerseMapping(sourceVerses, targetVerses);
                    });


                return new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                        TokenizedCorpusId targetTokenizedCorpusId,
                        IEnumerable<VerseMapping> verseMappings,
                        ParallelCorpusId parallelCorpusId)>
                    ((
                        new TokenizedCorpusId(parallelCorpus.SourceTokenizedCorpusId),
                        new TokenizedCorpusId(parallelCorpus.TargetTokenizedCorpusId),
                        verseMappings,
                        new ParallelCorpusId(parallelCorpus.Id)
                    ));
            }
            catch (NullReferenceException e)
            {
                return new RequestResult<(TokenizedCorpusId sourceTokenizedCorpusId,
                        TokenizedCorpusId targetTokenizedCorpusId,
                        IEnumerable<VerseMapping> verseMappings,
                        ParallelCorpusId parallelCorpusId)>
                (
                    success: false,
                    message: e.Message
                );
            }
        }

        private Verse ValidateBuildVerse(Models.Verse verse, Dictionary<int, string> bookNumbersToAbbreviations)
        {
            string? bookAbbreviation;
            if (verse.BookNumber == null || verse.ChapterNumber == null || verse.VerseNumber == null)
            {
                throw new NullReferenceException($"Source verse {verse.Id} in database has null book, chapter or verse number.  Unable to convert to engine Verse");
            }
            else if (!bookNumbersToAbbreviations.TryGetValue((int)verse.BookNumber, out bookAbbreviation))
            {
                throw new NullReferenceException($"Source verse {verse.Id} in database has unknown book number {verse.BookNumber}.  Unable to determine book abbreviation in order to convert to engine Verse");
            }
            
            return new Verse(
                (string)bookAbbreviation, 
                (int)verse.ChapterNumber, 
                (int)verse.VerseNumber,
                verse.TokenVerseAssociations.Where(tva => tva.Token != null).Select(tva => 
                    new TokenId(tva.Token!.BookNumber, tva.Token!.ChapterNumber, tva.Token!.VerseNumber, tva.Token!.WordNumber, tva.Token!.SubwordNumber))
            );
        }
    }
}
