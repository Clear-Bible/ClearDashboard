using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DataAccessLayer.Features.Projects
{
	/// <summary>
	/// If ProjectName is an empty string, no ProjectName filtering is done.  
	/// Otherwise, if ProjectNamePrefixMatch is true, any database project name
	/// starting with (ignore case) request.ProjectName will match.  If
	/// ProjectNamePrefixMatch is false, only exact project name matches (ignore 
	/// case) will be returned.
	/// </summary>
	/// <param name="ProjectName"></param>
	/// <param name="ProjectNamePrefixMatch"></param>
	public record GetAllProjectsFromDatabaseQuery(string ProjectName = "", bool ProjectNamePrefixMatch = true) : ProjectRequestQuery<IEnumerable<Models.Project>>;

	public class GetAllProjectsFromDatabaseQueryHandler : ProjectDbContextQueryHandler<GetAllProjectsFromDatabaseQuery,
		RequestResult<IEnumerable<Models.Project>>, IEnumerable<Models.Project>>
	{
		public GetAllProjectsFromDatabaseQueryHandler(
			ProjectDbContextFactory? projectNameDbContextFactory,
			IProjectProvider projectProvider, 
			ILogger<GetAllProjectsFromDatabaseQueryHandler> logger)
			: base(projectNameDbContextFactory, projectProvider, logger)
		{
		}

		protected override async Task<RequestResult<IEnumerable<Models.Project>>> GetDataAsync(GetAllProjectsFromDatabaseQuery request, CancellationToken cancellationToken)
		{
			await ProjectDbContext.Migrate();

			if (string.IsNullOrEmpty(request.ProjectName))
			{
				return new RequestResult<IEnumerable<Models.Project>>(
					await ProjectDbContext.Projects.ToListAsync(cancellationToken: cancellationToken)
				);
			}
			else
			{
				var queryable = ProjectDbContext.Projects 
					.Where(p => !string.IsNullOrEmpty(p.ProjectName));

				if (request.ProjectNamePrefixMatch)
				{
					queryable = queryable
						.Where(e => e.ProjectName!.StartsWith(request.ProjectName));
				}
				else
				{
					queryable = queryable
						.Where(e => string.Equals(e.ProjectName, request.ProjectName));
				}

				return new RequestResult<IEnumerable<Models.Project>>(
					await queryable.ToListAsync(cancellationToken: cancellationToken)
				);
			}
		}
	}
}
