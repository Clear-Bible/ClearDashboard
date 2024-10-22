﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.CQRS;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.Handlers
{
    public class GetRowsByParatextPluginIdAndBookIdQueryHandler : IRequestHandler<
        GetRowsByParatextProjectIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>>
    {
        public Task<RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>>
            Handle(GetRowsByParatextProjectIdAndBookIdQuery command, CancellationToken cancellationToken)
        {
            //Impl notes: look at command.Id (which is a string identifying the paratext plugin corpus to obtain) to get at the plugin corpus data,
            // then using this data build a TextRow per verse, with its Segment array as a single string containing the entire contents of the verse (don't parse)
            // and Ref as the VerseRef of the verse then return them through the enumarable.

            return Task.FromResult(
                new RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
                (result: new List<(string chapter, string verse, string text, bool isSentenceStart)>() { ("3", "4", "fee fi fo fum", true) },
                success: true,
                message: "successful result from test"));
        }
    }
}
