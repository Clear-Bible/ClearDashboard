using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Builder;

public class BuilderContext
{
    public readonly List<string> CommonIgnoreProperties = new List<string>() { /* "Metadata" */ };

    public ProjectDbContext ProjectDbContext { get; }
    public bool IncludeTokenizedTokens { get; set; }

    private readonly Dictionary<string, Dictionary<Guid, int>> idIndexMappings = new();
    private readonly Dictionary<string, int> modelNameIndexValues = new();

    public BuilderContext(ProjectDbContext projectDbContext)
    {
        ProjectDbContext = projectDbContext;
    }

    public void IncrementModelNameIndexValue(string modelName)
    {
        int index = 0;
        if (modelNameIndexValues.TryGetValue(modelName, out index))
        {
            index++;
            modelNameIndexValues[modelName] = index;
        }
        else
        {
            modelNameIndexValues.TryAdd(modelName, index);
        }
    }

    public int GetModelNameIndexValue(string modelName)
    {
        if (modelNameIndexValues.TryGetValue(modelName, out var index))
        {
            return index;
        }
        else
        {
            throw new BuilderReferenceException($"{nameof(IncrementModelNameIndexValue)} has not yet been called for model name {modelName}");
        }
    }

    public bool TryGetIdToIndexValue(string modelName, Guid id, out int indexValue)
    {
        indexValue = default(int);
        if (!idIndexMappings.TryGetValue(modelName, out var idIndexes))
        {
            return false;
        }

        if (idIndexes.TryGetValue(id, out indexValue))
        {
            return true;
        }
        else
        {
            throw new BuilderReferenceException($"IdIndexMapping not found for model name {modelName} id {id}");
        }
    }

    public void UpsertIdToIndexValue(string modelName, Guid id)
    {
        if (modelNameIndexValues.TryGetValue(modelName, out var index))
        {
            if (!idIndexMappings.ContainsKey(modelName))
            {
                idIndexMappings.Add(modelName, new Dictionary<Guid, int>());
            }

            idIndexMappings[modelName].Add(id, index);
        }
        else
        {
            throw new BuilderReferenceException($"No index value set for model name {modelName}");
        }
    }
}
