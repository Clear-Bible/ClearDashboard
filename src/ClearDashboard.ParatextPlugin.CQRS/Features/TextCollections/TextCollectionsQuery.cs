using System;
using System.Collections.Generic;
using System.Text;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using MediatR;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections
{
    public record GetTextCollectionsQuery(bool FetchUsx, bool IsVerseByVerse) : IRequest<RequestResult<List<TextCollection>>>
    {
        public bool FetchUsx { get; } = FetchUsx;

        public bool IsVerseByVerse { get; } = IsVerseByVerse;

    };
}
