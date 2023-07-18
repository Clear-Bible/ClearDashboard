using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DAL.Alignment.Features.Notes
{
    public class GetLabelGroupNamesLabelsByNamesQueryHandler : ProjectDbContextQueryHandler<
        GetLabelGroupNamesLabelsByNamesQuery,
        RequestResult<IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>>,
        IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>>
    {
        private readonly IMediator _mediator;

        public GetLabelGroupNamesLabelsByNamesQueryHandler(IMediator mediator, 
            ProjectDbContextFactory? projectNameDbContextFactory, 
            IProjectProvider projectProvider, 
            ILogger<GetLabelGroupNamesLabelsByNamesQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>>> GetDataAsync(GetLabelGroupNamesLabelsByNamesQuery request, CancellationToken cancellationToken)
        {
            var databaseLabelGroups = ProjectDbContext.LabelGroups
                .Include(e => e.Labels)
                .AsQueryable();

            if (request.LabelGroupNames is not null && request.LabelGroupNames.Any())
            {
                databaseLabelGroups = databaseLabelGroups.Where(e => request.LabelGroupNames.Contains(e.Name));
            }

            IDictionary<string, IEnumerable<(string, string? TemplateText)>> labelGroupNamesLabels = await databaseLabelGroups
                .ToDictionaryAsync(
                    e => e.Name, 
                    e => e.Labels.ToList().Select(l => (l.Text!, l.TemplateText)), 
                    cancellationToken: cancellationToken);

            return new RequestResult<IDictionary<string, IEnumerable<(string Text, string? TemplateText)>>>
            (
                result: labelGroupNamesLabels
            );
        }
    }
}
