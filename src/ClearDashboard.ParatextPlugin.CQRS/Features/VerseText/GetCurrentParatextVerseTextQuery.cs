using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using System.Net;
using System.Runtime.InteropServices;

namespace ClearDashboard.ParatextPlugin.CQRS.Features.VerseText
{
    public record GetParatextVerseTextQuery
        (int BookNum, int ChapterNum, int VerseNum, bool ReturnBackTranslation = false) : IRequest<RequestResult<AssignedUser>>
    {
        public int BookNum { get; } = BookNum;
        public int ChapterNum { get; } = ChapterNum;
        public int VerseNum { get; } = VerseNum;
        public bool ReturnBackTranslation { get; } = ReturnBackTranslation;
    }

    
}
