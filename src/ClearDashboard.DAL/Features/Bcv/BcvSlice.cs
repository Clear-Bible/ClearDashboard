using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.Bcv
{
    //public record GetBcvDictionariesQuery() : 
    //    IRequest<RequestResult<Dictionary<string, string>>>;

    public record GetBcvDictionariesQuery(string paratextProjectId) : ProjectRequestQuery<Dictionary<string, string>>;

    public class GetBcvDictionariesQueryHandler : ProjectDbContextQueryHandler<
        GetBcvDictionariesQuery,
        RequestResult<Dictionary<string, string>>,
        Dictionary<string, string>>
    {
        private readonly IMediator _mediator;

        public GetBcvDictionariesQueryHandler(IMediator mediator, ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider, ILogger<GetBcvDictionariesQueryHandler> logger)
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<Dictionary<string, string>>> GetDataAsync(GetBcvDictionariesQuery request, CancellationToken cancellationToken)
        {
            var id = ProjectDbContext.Corpa
                .Where(tc => tc.ParatextGuid == request.paratextProjectId)
                .Select(tc => tc.Id)
                .FirstOrDefault();

            var tokenizedCorpusId = ProjectDbContext.TokenizedCorpora
                .Where(tc => tc.CorpusId == id)
                .Select(tc => tc.Id)
                .FirstOrDefault();

            var verses = ProjectDbContext.Tokens
                .Where(tc => tc.TokenizedCorpusId == tokenizedCorpusId)
                .Select(tc => new { tc.BookNumber, tc.ChapterNumber, tc.VerseNumber })
                .Distinct()
                .Select(bcv => new
                {
                    BCV = bcv.BookNumber.ToString().PadLeft(3, '0') + bcv.ChapterNumber.ToString().PadLeft(3, '0') +
                          bcv.VerseNumber.ToString().PadLeft(3, '0')
                })
                .ToDictionary(bcv => bcv.BCV, bcv => bcv.BCV);


            return new RequestResult<Dictionary<string, string>>(verses);
        }
    }
}
