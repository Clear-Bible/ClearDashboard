using ClearDashboard.Collaboration.Model;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class UserBuilder : GeneralModelBuilder<Models.User>
{
    public override IEnumerable<GeneralModel<Models.User>> BuildModelSnapshots(BuilderContext builderContext)
    {
        var modelSnapshots = new GeneralListModel<GeneralModel<Models.User>>();

        builderContext.ProjectDbContext.Users
            .ToList()
            .ForEach(e => modelSnapshots.Add(ExtractUsingModelIds(e, builderContext.CommonIgnoreProperties)));

        return modelSnapshots;
    }
}
