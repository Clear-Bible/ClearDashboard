using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class CorpusHandler : DefaultMergeHandler
{
    public CorpusHandler(MergeContext mergeContext) : base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Corpus), nameof(Models.Corpus.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Corpus>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Corpus>");
                }

                var corpusId = (Guid)modelSnapshot.GetId();

                if (CorpusBuilder.CorpusManuscriptIds.ContainsValue(corpusId) &&
                    !projectDbContext.Corpa.Any(e => e.Id == corpusId))
                {
                    var corpusType = CorpusBuilder.CorpusManuscriptIds
                        .First(x => x.Value == corpusId)
                        .Key;

                    var id = projectDbContext.Corpa
                        .Where(e => e.CorpusType == corpusType)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    if (id != default)
                    {
                        corpusId = id;
                    }
                }

                return corpusId;
            });
    }
}
