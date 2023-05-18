using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ClearDashboard.Collaboration.Serializer;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;

namespace ClearDashboard.Collaboration;

public class ProjectCommit
{
    public ProjectCommit(Guid id, IEnumerable<Guid>? parentCommitIds, Guid? userId, DateTimeOffset? commitDate = null)
    {
        Id = id;
        ParentCommitIds = parentCommitIds;
        UserId = userId;
        CommitDate = commitDate ?? DateTimeOffset.UtcNow;
    }

    [JsonConstructor]
    public ProjectCommit() { }

    public Guid Id {  get; set; }
    public IEnumerable<Guid>? ParentCommitIds { get; set; }
    public Guid? UserId {  get; set; }
    public DateTimeOffset? CommitDate {  get; set; }

    public ProjectSnapshot? ProjectSnapshot { get; set; }

    public static string Serialize(ProjectCommit commit)
    {
        var serializedCommit = JsonSerializer.Serialize<ProjectCommit>(commit, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            IncludeFields = true,
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver()
        });

        return serializedCommit;
    }

    public static ProjectCommit? Deserialize(string serializedCommit)
    {
        var commit = JsonSerializer.Deserialize<ProjectCommit>(serializedCommit, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            IncludeFields = true,
            WriteIndented = true,
            TypeInfoResolver = new PolymorphicTypeResolver()
        });

        return commit;
    }

}