using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Features.Grammar
{

    public record GetGrammarSuggestionsQuery(string projectName) : ProjectRequestQuery<List<Models.Grammar>>;
 
    public class GetGrammarSuggestionsQueryHandler : ProjectDbContextQueryHandler<GetGrammarSuggestionsQuery, RequestResult<List<Models.Grammar>>, List<Models.Grammar>>
{
    public GetGrammarSuggestionsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider? projectProvider, ILogger<GetGrammarSuggestionsQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
    {
    }

    protected override async Task<RequestResult<List<Models.Grammar>>> GetDataAsync(GetGrammarSuggestionsQuery request, CancellationToken cancellationToken)
    {
        // need an await to get the compiler to be 'quiet'
        //await Task.CompletedTask;

        var grammars = await ProjectDbContext.Grammars.ToListAsync(cancellationToken);

        return new RequestResult<List<Models.Grammar>>(grammars);


    }
}
}
