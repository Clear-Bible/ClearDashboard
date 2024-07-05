using System;
using System.Data.Common;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class GrammarHandler : DefaultMergeHandler<IModelSnapshot<Models.Grammar>>
{
    public static readonly Func<string, ProjectDbContext, ILogger, Task<Guid>> GrammarShortNameToId = async (grammarShortName, projectDbContext, logger) =>
    {
        var grammarId = await projectDbContext.Grammars
            .Where(e => e.ShortName! == grammarShortName)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        logger.LogDebug($"Converted Grammar ShortName ('{grammarShortName}') to GrammarId ('{grammarId}')");
        return grammarId;
    };

    public GrammarHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Grammar), nameof(Models.Grammar.Id)),
            entityValueResolver: async (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, DbConnection dbConnection, MergeCache cache, ILogger logger) => {

                var shortName = LexiconHandler.ExtractStringProperty(modelSnapshot, nameof(Models.Grammar.ShortName));

                var grammarId = await GrammarShortNameToId(shortName, projectDbContext, logger);
                return (grammarId != default) ? grammarId : null;

            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Grammar), GrammarBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Grammar.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Grammar), GrammarBuilder.BuildPropertyRefName()),
            new[] { nameof(Models.Grammar.Id) });
    }

}