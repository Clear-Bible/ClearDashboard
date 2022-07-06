using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;


//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class GetRowsByParatextPluginIdAndBookIdQueryHandler : ProjectDbContextQueryHandler<
        GetRowsByParatextPluginIdAndBookIdQuery,
        RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>,
        IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>
    {

        public GetRowsByParatextPluginIdAndBookIdQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetRowsByParatextPluginIdAndBookIdQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override Task<RequestResult<IEnumerable<(string chapter, string verse, string text, bool isSentenceStart)>>> GetDataAsync(GetRowsByParatextPluginIdAndBookIdQuery request, CancellationToken cancellationToken)
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
