using System.Text.Json.Serialization;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration;

public class LexiconCommit
{
    public LexiconCommit(Guid id, IEnumerable<Guid>? parentCommitIds, Guid? userId, DateTimeOffset? commitDate = null)
    {
        Id = id;
        ParentCommitIds = parentCommitIds;
        UserId = userId;
        CommitDate = commitDate ?? DateTimeOffset.UtcNow;
    }

    [JsonConstructor]
    public LexiconCommit() { }

    public Guid Id { get; set; }
    public IEnumerable<Guid>? ParentCommitIds { get; set; }
    public Guid? UserId { get; set; }
    public DateTimeOffset? CommitDate { get; set; }

    public LexiconSnapshot? LexiconSnapshot { get; set; }

    public static string Serialize(ProjectCommit commit)
    {
        var serializedCommit = JsonSerializer.Serialize(commit, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            IncludeFields = true
        });

        return serializedCommit;
    }

    public static ProjectCommit? Deserialize(string serializedCommit)
    {
        var commit = JsonSerializer.Deserialize<ProjectCommit>(serializedCommit, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            IncludeFields = true
        });

        return commit;
    }
}