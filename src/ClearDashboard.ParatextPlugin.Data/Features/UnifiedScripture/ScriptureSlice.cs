using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace ParaTextPlugin.Data.Features.UnifiedScripture
{
    public record GetUsxQuery() : IRequest<QueryResult<string>>
    {

    }

    public record GetUsfmQuery() : IRequest<QueryResult<string>>
    {

    }
}
