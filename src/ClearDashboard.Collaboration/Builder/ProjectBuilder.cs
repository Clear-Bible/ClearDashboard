using System.Reflection;
using System.Text.Json;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Serializer;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class ProjectBuilder : GeneralModelBuilder<Models.Project>
{
    //public override IReadOnlyDictionary<string, Type> AddedPropertyNamesTypes =>
    //    new Dictionary<string, Type>()
    //    {

    //    };

    public static GeneralModel<Models.Project> BuildModelSnapshot(BuilderContext builderContext)
    {
        var project = GetProject(builderContext.ProjectDbContext);
        return BuildModelSnapshot(project);
    }

    public static GeneralModel<Models.Project> BuildModelSnapshot(Models.Project project)
    {
        var modelSnapshot = ExtractUsingModelIds(
            project,
            new List<string>() {
                nameof(Models.Project.LastMergedCommitSha),
                nameof(Models.Project.DesignSurfaceLayout),
                nameof(Models.Project.WindowTabLayout)
            });

        return modelSnapshot;
    }

    public static Models.Project BuildModel(GeneralModel<Models.Project> modelSnapshot)
    {
        var project = new Models.Project();

        foreach (var propertyInfo in typeof(Models.Project).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (propertyInfo.CanWrite && modelSnapshot.PropertyValues.TryGetValue(propertyInfo.Name, out var value))
            {
                propertyInfo.SetValue(project, value);
            }
        }

        return project;
    }

    public static Models.Project GetProject(ProjectDbContext projectDbContext)
    {
        return projectDbContext.Projects.OrderBy(c => c.Created).First();
    }
}
