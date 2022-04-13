using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Common.Models;
using MediatR;

namespace ClearDashboard.DataAccessLayer.Slices.ManuscriptVerses
{
    public record GetManuscriptVerseByIdQuery(string VerseId) : IRequest<List<CoupleOfStrings>>;
    

    public class GetManuscriptVerseByIdHandler : IRequestHandler<GetManuscriptVerseByIdQuery, List<CoupleOfStrings>>
    {
        public GetManuscriptVerseByIdHandler()
        {
            // inject Database Context here...
        }

        public Task<List<CoupleOfStrings>> Handle(GetManuscriptVerseByIdQuery request, CancellationToken cancellationToken)
        {
            var list = new List<CoupleOfStrings> { new CoupleOfStrings {stringA = $"String A from {request.VerseId}", stringB = $"String B from {request.VerseId}"} };

            return Task.FromResult(list);
        }
    }
}
