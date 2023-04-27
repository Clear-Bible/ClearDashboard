using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Features.User
{
    public record AddUserCommand(Models.User User) : ProjectRequestCommand<bool>;

    public class AddUserCommandHandler : ProjectDbContextCommandHandler<AddUserCommand, RequestResult<bool>, bool>
    {
        public AddUserCommandHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<AddUserCommandHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
            ProjectNameDbContextFactory = projectNameDbContextFactory ?? throw new ArgumentNullException(nameof(projectNameDbContextFactory));
            Logger = logger;
        }

        protected override async Task<RequestResult<bool>> SaveDataAsync(AddUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await ProjectDbContext.Users.AddAsync(request.User, cancellationToken);
                await ProjectDbContext.SaveChangesAsync(cancellationToken);

                return new RequestResult<bool>(true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database while adding user '{request.User.FullName}, {request.User.Id}'");
                return new RequestResult<bool>(false);
            }
        }

       
    }
}
