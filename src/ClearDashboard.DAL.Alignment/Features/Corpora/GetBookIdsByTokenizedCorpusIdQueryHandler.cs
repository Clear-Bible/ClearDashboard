using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using System.Text;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Extensions;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public class GetBookIdsByTokenizedCorpusIdQueryHandler : ProjectDbContextQueryHandler<
    GetBookIdsByTokenizedCorpusIdQuery,
    RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>,
    (IEnumerable<string> bookId, CorpusId corpusId)>
{
    private readonly IMediator _mediator;

    public GetBookIdsByTokenizedCorpusIdQueryHandler(IMediator mediator,
        ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
        ILogger<GetBookIdsByTokenizedCorpusIdQueryHandler> logger)
        : base(projectNameDbContextFactory, projectProvider, logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>> GetDataAsync(
        GetBookIdsByTokenizedCorpusIdQuery request, CancellationToken cancellationToken)
    {
        //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
        // pull out its parent CorpusId
        //Then iterate tokenization.Corpus(parent).Verses(child) and find unique bookAbbreviations and return as IEnumerable<string>
        var tokenizedCorpus =
            ProjectDbContext.TokenizedCorpora.Include(tc => tc.Tokens).Include(tc => tc.Corpus).FirstOrDefault(i => i.Id == request.TokenizedCorpusId.Id);

        if (tokenizedCorpus == null)
        {
            return new RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>
            (
                // NB:  better to return default(T) which is the default on the constructor.
                //result: (new List<string>(), new CorpusId(new Guid())),
                success: false,
                message: $"TokenizedCorpus not found for TokenizedCorpusId {request.TokenizedCorpusId.Id}"
            );
        }

        var bookNumbers = tokenizedCorpus.Tokens.GroupBy(token => token.BookNumber).Select(g => g.Key);
        var bookNumbersToAbbreviations =
            FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum),
                x => x.silCannonBookAbbrev);

        var bookAbbreviations = new List<string>();
        foreach (var bookNumber in bookNumbers)
        {
            if (!bookNumbersToAbbreviations.TryGetValue(bookNumber, out string? bookAbbreviation))
            {
                return new RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>
                (
                    result: (new List<string>(), new CorpusId(new Guid())),
                    success: false,
                    message: $"Book number '{bookNumber}' not found in FileGetBooks.BookIds"
                );
            }

            bookAbbreviations.Add(bookAbbreviation);
        }

        return new RequestResult<(IEnumerable<string> bookId, CorpusId corpusId)>
            ((bookAbbreviations, new CorpusId(tokenizedCorpus.Corpus.Id)));
    }
}