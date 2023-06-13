namespace ClearDashboard.Collaboration.Services;

internal static class RepositoryExtensions
{
    internal static IEnumerable<string> ToTopLevelPropertiesPaths(this IEnumerable<string> paths)
    {
        return  paths.Select(e => e.Split(Path.DirectorySeparatorChar)[0])
            .Distinct()
            .Select(e => $"{e}{Path.DirectorySeparatorChar}_Properties")
            .ToList();
    }

    internal static IEnumerable<Guid> ToProjectIds(this IEnumerable<string> paths)
    {
        return paths
            .Select(e => e.Split(Path.DirectorySeparatorChar)[0].Substring("Project_".Length))
            .Distinct()
            .Select(e => (success: Guid.TryParse(e, out var id), id: id))
            .Where(parsed => parsed.success)
            .Select(parsed => parsed.id)
            .ToList();
    }
}