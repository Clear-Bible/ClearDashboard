using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace ParaTextPlugin.Data.Features.UnifiedScripture
{
    public record GetUsxCommand() : IRequest<QueryResult<string>>
    {

    }

    public record GetUsfmCommand() : IRequest<QueryResult<string>>
    {

    }
}
