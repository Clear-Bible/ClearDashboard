using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Corpora
{
    public class FindTokensBySurfaceTextQueryHandler : ProjectDbContextQueryHandler<
        FindTokensBySurfaceTextQuery,
        RequestResult<IEnumerable<Token>>,
        IEnumerable<Token>>
    {

        public FindTokensBySurfaceTextQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<FindTokensBySurfaceTextQueryHandler> logger) : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<IEnumerable<Token>>> GetDataAsync(FindTokensBySurfaceTextQuery request, CancellationToken cancellationToken)
        {
            var tokenizedCorpus = await ProjectDbContext!.TokenizedCorpora
                .FirstOrDefaultAsync(i => i.Id == request.TokenizedTextCorpusId.Id);

            if (tokenizedCorpus == null)
            {
                return new RequestResult<IEnumerable<Token>>
                (
                    success: false,
                    message: $"TokenizedCorpus not found for TokenizedCorpusId {request.TokenizedTextCorpusId.Id}"
                );
            }

            IQueryable<Models.Token> findTokensQuery = ProjectDbContext!.Tokens
                .Where(e => e.TokenizedCorpusId == request.TokenizedTextCorpusId.Id)
                .Where(e => e.Deleted == null)
                .Where(e => !string.IsNullOrEmpty(e.SurfaceText));

            if (request.IgnoreCase)
            {
                switch (request.WordPart)
                {
                    case WordPart.First:
                        findTokensQuery = findTokensQuery.Where(e => EF.Functions.Like(e.SurfaceText!, $"{request.SearchString}%"));
                        break;
                    case WordPart.Middle:
                        findTokensQuery = findTokensQuery.Where(e => EF.Functions.Like(e.SurfaceText!, $"%{request.SearchString}%"));
                        break;
                    case WordPart.Last:
                        findTokensQuery = findTokensQuery.Where(e => EF.Functions.Like(e.SurfaceText!, $"%{request.SearchString}"));
                        break;
                    default:  /* WordPart.Full */
                        findTokensQuery = findTokensQuery.Where(e => EF.Functions.Like(e.SurfaceText!, $"{request.SearchString}"));
                        break;
                }
            }
            else
            {
                switch (request.WordPart)
                {
                    // NB:  This is now not case-sensitive - due to upgrade to .net 8.  Do we need to fix? 
                    case WordPart.First:
                        findTokensQuery = findTokensQuery.Where(e => e.SurfaceText!.StartsWith(request.SearchString));
                        break;
                    case WordPart.Middle:
                        findTokensQuery = findTokensQuery.Where(e => e.SurfaceText!.Contains(request.SearchString));
                        break;
                    case WordPart.Last:
                        findTokensQuery = findTokensQuery.Where(e => e.SurfaceText!.EndsWith(request.SearchString));
                        break;
                    default:  /* WordPart.Full */
                        findTokensQuery = findTokensQuery.Where(e => e.SurfaceText!.Equals(request.SearchString));
                        break;
                }
            }

            var tokens = await findTokensQuery
                .Select(e => ModelHelper.BuildToken(e))
                .ToListAsync(cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            return new RequestResult<IEnumerable<Token>>(tokens);
        }
    }
}
