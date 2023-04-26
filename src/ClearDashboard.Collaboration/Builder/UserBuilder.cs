using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class UserBuilder : GeneralModelBuilder<Models.User>
{
    public Func<ProjectDbContext, IEnumerable<Models.User>> GetUsers = (projectDbContext) =>
    {
        return projectDbContext.Users.ToList();
    };

    public override IEnumerable<GeneralModel<Models.User>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.User>>();

        var modelItems = GetUsers(builderContext.ProjectDbContext);
        foreach (var item in modelItems)
        {
            modelSnapshots.Add(BuildModelSnapshot(item, builderContext));
        }

        return modelSnapshots;
    }

    public static GeneralModel<Models.User> BuildModelSnapshot(Models.User user, BuilderContext builderContext)
    {
        var modelSnapshot = ExtractUsingModelIds(user, builderContext.CommonIgnoreProperties);
        return modelSnapshot;
    }
}
