using MediatR;

namespace ParaTextPlugin.Data.Features.Verse
{
    public record GetCurrentVerseCommand() : IRequest<QueryResult<string>>;
}
