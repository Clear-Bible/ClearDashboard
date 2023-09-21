using System;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClearDashboard.Collaboration.Exceptions;
using ClearDashboard.Collaboration.Builder;

namespace ClearDashboard.Collaboration.Merge;

public class TokenHandler : DefaultMergeHandler<IModelSnapshot<Models.Token>>
{
    public TokenHandler(MergeContext mergeContext): base(mergeContext)
    {
        mergeContext.MergeBehavior.AddEntityTypeDiscriminatorMapping(
            entityType: typeof(Models.Token),
            (TableEntityType: typeof(Models.TokenComponent), DiscriminatorColumnName: "Discriminator", DiscriminatorColumnValue: "Token")
        );

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Token), nameof(Models.Token.VerseRowId)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot.PropertyValues.TryGetValue(TokenBuilder.VERSE_ROW_LOCATION, out var verseRowLocation) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.TokenizedCorpusId), out var tokenizedCorpusId))
                {
                    var verseRowId = projectDbContext.VerseRows
                        .Where(e => (Guid)e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                        .Where(e => (string)e.BookChapterVerse! == (string)verseRowLocation!)
                        .Select(e => e.Id)
                        .FirstOrDefault();

                    logger.LogDebug($"Converted Token TokenizedCorpusId ('{tokenizedCorpusId}') / VerseRowLocation ('{verseRowLocation}') to VerseRowId ('{verseRowId}')");
                    return (verseRowId != Guid.Empty) ? verseRowId : null;
                }
                else
                {
                    throw new PropertyResolutionException($"Token snapshot does not have both TokenizedCorpusId+VerseRowId, which are required for VerseRowLocation conversion.");
                }

            });

        mergeContext.MergeBehavior.AddEntityValueResolver(
            (typeof(Models.Token), nameof(Models.Token.Id)),
            entityValueResolver: (IModelSnapshot modelSnapshot, ProjectDbContext projectDbContext, MergeCache cache, ILogger logger) => {

                if (modelSnapshot is not IModelSnapshot<Models.Token>)
                {
                    throw new ArgumentException($"modelSnapshot must be an instance of IModelSnapshot<Models.Token>");
                }

                if (modelSnapshot.PropertyValues.TryGetValue("Ref", out var refId) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.TokenizedCorpusId), out var tokenizedCorpusId) &&
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.EngineTokenId), out var engineTokenId))
                {
                    modelSnapshot.PropertyValues.TryGetValue(nameof(Models.Token.OriginTokenLocation), out var originTokenLocation);
                    var hasIndex = int.TryParse(((string)refId!).Split("_").LastOrDefault(), out int index);

                    Guid tokenId;
                    if (originTokenLocation is not null && hasIndex)
                    {
                        tokenId = projectDbContext.Tokens
                            .Where(e => e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                            .Where(e => e.OriginTokenLocation == (string)originTokenLocation!)
                            .OrderBy(e => e.OriginTokenLocation)
                            .Skip(index)
                            .Take(1)
                            .Select(e => e.Id)
                            .FirstOrDefault();

                        if (tokenId == default)
                        {
                            tokenId = Guid.NewGuid();
                            logger.LogDebug($"No Token Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / OriginTokenLocation ('{originTokenLocation}') / Index ({index}).  Using: '{tokenId}'");
                        }
                        else
                        {
                            logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / OriginTokenLocation ('{originTokenLocation}') / Index ({index}) to Token Id ('{tokenId}')");
                        }
                    }
                    else
                    {
                        tokenId = projectDbContext.Tokens
                            .Where(e => e.TokenizedCorpusId == (Guid)tokenizedCorpusId!)
                            .Where(e => e.EngineTokenId == (string)engineTokenId!)
                            .Select(e => e.Id)
                            .FirstOrDefault();

                        if (tokenId == default)
                        {
                            tokenId = Guid.NewGuid();
                            logger.LogDebug($"No Token Id match found for TokenizedCorpusId ('{tokenizedCorpusId}') / EngineTokenId ('{engineTokenId}').  Using: '{tokenId}'");
                        }
                        else
                        {
                            logger.LogDebug($"Resolved TokenizedCorpusId ('{tokenizedCorpusId}') / EngineTokenId ('{engineTokenId}') to Token Id ('{tokenId}')");
                        }
                    }

                    return tokenId;
                }
                else
                {
                    throw new PropertyResolutionException($"Token snapshot does not have all:  Ref+TokenizedCorpusId+EngineTokenId, which are required for Id resolution.");
                }
            });

        mergeContext.MergeBehavior.AddIdPropertyNameMapping(
            (typeof(Models.Token), "Ref"),
            new[] { nameof(Models.Token.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Token), "Ref"),
            new[] { nameof(Models.Token.Id) });

        mergeContext.MergeBehavior.AddPropertyNameMapping(
            (typeof(Models.Token), TokenBuilder.VERSE_ROW_LOCATION),
            new[] { nameof(Models.Token.VerseRowId) });
    }
}
