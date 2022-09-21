﻿using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Linq;

namespace ClearDashboard.DAL.Alignment.Features.Corpora;

public class GetBookIdsByTokenizedCorpusIdQueryHandler : ProjectDbContextQueryHandler<
    GetBookIdsByTokenizedCorpusIdQuery,
    RequestResult<(IEnumerable<string> bookId, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>,
    (IEnumerable<string> bookId, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>
{
    private readonly IMediator _mediator;

    public GetBookIdsByTokenizedCorpusIdQueryHandler(IMediator mediator,
        ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
        ILogger<GetBookIdsByTokenizedCorpusIdQueryHandler> logger)
        : base(projectNameDbContextFactory, projectProvider, logger)
    {
        _mediator = mediator;
    }

    protected override async Task<RequestResult<(IEnumerable<string> bookId, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>> GetDataAsync(
        GetBookIdsByTokenizedCorpusIdQuery request, CancellationToken cancellationToken)
    {
        //DB Impl notes: look at command.TokenizedCorpusId and find in TokenizedCorpus table.
        // pull out its parent CorpusId
        //Then iterate tokenization.Corpus(parent).Verses(child) and find unique bookAbbreviations and return as IEnumerable<string>
        var tokenizedCorpus =
            ProjectDbContext.TokenizedCorpora
            .Include(tc => tc.Corpus)
            .Include(tc => tc.User)
            .FirstOrDefault(i => i.Id == request.TokenizedTextCorpusId.Id);

        if (tokenizedCorpus == null)
        {
            return new RequestResult<(IEnumerable<string> bookId, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>
            (
                // NB:  better to return default(T) which is the default on the constructor.
                //result: (new List<string>(), new CorpusId(new Guid())),
                success: false,
                message: $"TokenizedCorpus not found for TokenizedCorpusId {request.TokenizedTextCorpusId.Id}"
            );
        }

        var bookNumbers = ProjectDbContext.Tokens
             .Where(tc => tc.TokenizationId == request.TokenizedTextCorpusId.Id)
             .Select(tc => tc.BookNumber).Distinct();

        var bookNumbersToAbbreviations =
            FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum),
                x => x.silCannonBookAbbrev);


        var bookAbbreviations = new List<string>();
        foreach (var bookNumber in bookNumbers)
        {
            if (!bookNumbersToAbbreviations.TryGetValue(bookNumber, out string? bookAbbreviation))
            {
                return new RequestResult<(IEnumerable<string> bookId, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>
                (
                    success: false,
                    message: $"Book number '{bookNumber}' not found in FileGetBooks.BookIds"
                );
            }

            bookAbbreviations.Add(bookAbbreviation);
        }

        return new RequestResult<(IEnumerable<string> bookId, TokenizedTextCorpusId tokenizedTextCorpusId, CorpusId corpusId)>
            ((
                bookAbbreviations, 
                ModelHelper.BuildTokenizedTextCorpusId(tokenizedCorpus),
                ModelHelper.BuildCorpusId(tokenizedCorpus)
            ));
    }
}