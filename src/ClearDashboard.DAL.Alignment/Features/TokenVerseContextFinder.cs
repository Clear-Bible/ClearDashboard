using System.Diagnostics;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Persistence;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.FiniteState;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features
{
    public static class TokenVerseContextFinder
    {
        public static (IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex) GetTokenVerseRowContext(Token token, ProjectDbContext projectDbContext, ILogger logger)
        {
            if (token is CompositeToken token1)
            {
                // Not sure what to return here.  The tokens/index of the first one?
                // A composite may not be associated with a VerseRow...maybe have tokens in multiple VerseRows

                var tokenComponentIds = token1.Tokens.Select(e => e.TokenId.Id).ToList();

                var tokenChildren = projectDbContext.TokenComponents
                    .Include(e => e.VerseRow)
                        .ThenInclude(e => e!.TokenComponents)
                            .ThenInclude(e => ((Models.Token)e!).TokenComposites.Where(tc => tc.ParallelCorpusId == null))
                    .Where(e => tokenComponentIds.Contains(e.Id))
                    .ToList();

                var tokenComponent = tokenChildren.FirstOrDefault();
                if (tokenComponent is null)
                {
                    throw new Exception($"Unable to find token verse contexts due to no child tokens found for Token Id '{token.TokenId.Id}'");
                }

                var tokensForVerseRow = FindTokensForVerseRow(tokenComponent.VerseRow!);
                var tokenIndex = FindTokenIndex(tokenComponent.Id, tokensForVerseRow);
                if (tokenIndex < 0)
                {
                    // This makes no sense
                    throw new Exception($"Unable to find token '{token.TokenId.Id}' in VerseRow '{tokenComponent.VerseRow!.Id}'");
                }

                return (tokensForVerseRow.Select(t => ModelHelper.BuildToken(t)), (uint)tokenIndex);
            }
            else
            {
                var tokenComponent = projectDbContext.TokenComponents
                    .Include(e => e.VerseRow)
                        .ThenInclude(e => e!.TokenComponents)
                            .ThenInclude(e => ((Models.Token)e!).TokenComposites.Where(tc => tc.ParallelCorpusId == null))
                    .Where(e => e.Id == token.TokenId.Id)
                    .FirstOrDefault();

                if (tokenComponent is null)
                {
                    throw new Exception($"Unable to find token verse contexts due to invalid TokenComponent Id '{token.TokenId.Id}'");
                }

                var tokensForVerseRow = FindTokensForVerseRow(tokenComponent.VerseRow!);
                var tokenIndex = FindTokenIndex(token.TokenId.Id, tokensForVerseRow);
                if (tokenIndex < 0)
                {
                    // This makes no sense
                    throw new Exception($"Unable to find token '{token.TokenId.Id}' in VerseRow '{tokenComponent.VerseRow.Id}'");
                }

                return (tokensForVerseRow.Select(t => ModelHelper.BuildToken(t)), (uint)tokenIndex);
            }
        }

        private static List<Models.TokenComponent> FindTokensForVerseRow(Models.VerseRow verseRow)
        {
            List<Models.TokenComponent> tokensForVerseRow = verseRow.Tokens
                .SelectMany(t => t.TokenComposites)
                .DistinctBy(tc => tc.Id)
                .Cast<Models.TokenComponent>()
                .ToList();

            tokensForVerseRow.AddRange(verseRow.Tokens.Where(t => !t.TokenComposites.Any()));

            return tokensForVerseRow.OrderBy(e => e.EngineTokenId).ToList();
        }

        /// <summary>
        /// Gets all Verse contexts by TokenId
        /// </summary>
        /// <param name="parallelCorpusId"></param>
        /// <param name="tokens"></param>
        /// <param name="isSource"></param>
        /// <param name="projectDbContext"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static IDictionary<IId, (IEnumerable<Token> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex)> GetTokenVerseContexts(Models.ParallelCorpus parallelCorpus, IEnumerable<Token> tokens, bool isSource, ProjectDbContext projectDbContext, ILogger logger)
        {
            Stopwatch sw = new();
            sw.Start();

            var corpusId = isSource ? parallelCorpus.SourceTokenizedCorpus!.CorpusId : parallelCorpus.TargetTokenizedCorpus!.CorpusId;
            var tokenizedCorpusId = isSource ? parallelCorpus.SourceTokenizedCorpusId : parallelCorpus.TargetTokenizedCorpusId;

            // Here we query the database for TokenVerseAssociation data that
            // includes related VerseMappings and all their child Verses (we're
            // pulling TokenVerseAssociations from the incoming Token side):
            var tokenIdTVAs = GetAllRelatedTVAs(tokens, projectDbContext);
            var tokenTVAs = tokenIdTVAs.SelectMany(e => e.Value.Select(tva => tva));

            sw.Stop();
            logger.LogInformation("Elapsed={0} - TokenVerseAssociation (by Token Ids) database query [count: {1}]", sw.Elapsed, tokenTVAs.Count());
            sw.Restart();

            // For TokenComposite tokens, the Id of the composite together with
            // the BCV for each child token is returned.  (Filtering out any
            // tokens that have a TVA relationship)
            var tokenIdBCVs = GetAllRelatedBCVs(tokens, tokenIdTVAs.Keys);
            var tokenBCVs = tokenIdBCVs.SelectMany(e => e.Value.Select(v => v)).Distinct().ToList();

            // To avoid making two similar queries with heavily overlapping results,
            // look for all VerseMappings having any Verses (source or target) that
            // have any of the source or target token BCV values:
            var allTokenBCVVerseMappings = projectDbContext.VerseMappings
                .Include(e => e.Verses)
                .Where(e => e.ParallelCorpusId == parallelCorpus.Id)
                .Where(e => e.Verses.Any(v => tokenBCVs.Contains(v.BBBCCCVVV!)))
                .AsNoTrackingWithIdentityResolution()
                .ToList();

            // Separate query results into source and target:
            var tokenBCVVerseMappings = allTokenBCVVerseMappings
                .Where(e => e.Verses
                    .Where(v => v.CorpusId == corpusId)
                    .Where(v => tokenBCVs.Contains(v.BBBCCCVVV!))
                    .Any())
                .ToList();

            sw.Stop();
            logger.LogInformation("Elapsed={0} - VerseMappings (by bcv) database query [count: {1}]", sw.Elapsed, allTokenBCVVerseMappings.Count);
            sw.Restart();

            // VerseIds from both TVAs and BCVs:
            var allVerseIds = tokenBCVVerseMappings
                .Union(tokenTVAs.Select(t => t.Verse!.VerseMapping))
                .SelectMany(e => e!.Verses.Select(v => v.Id));

            // Now we pull all TokenVerseAssociations from the VerseMapping/Verse side
            // Include Composite parent/children in both directions (if this query ends
            // up being a perforance bottleneck, its probably because of TokenComponent
            // 'including' in both directions):
            var allVerseTVAs = projectDbContext.TokenVerseAssociations
                .Include(e => e.Verse)
                .Include(e => e.TokenComponent)
                    .ThenInclude(e => ((Models.TokenComposite)e!).Tokens)
                .Include(e => e.TokenComponent)
                    .ThenInclude(e => ((Models.Token)e!).TokenComposites
                        .Where(e => e.ParallelCorpusId == null || e.ParallelCorpusId == parallelCorpus.Id)
                        .Where(e => e.TokenizedCorpusId == tokenizedCorpusId))
                .Where(e => allVerseIds.Contains(e.VerseId))
                .ToList();

            sw.Stop();
            logger.LogInformation("Elapsed={0} - TokenVerseAssociation (by VerseMapping Verse Id) database query [count: {1}]", sw.Elapsed, allVerseTVAs.Count);
            sw.Restart();

            // Now we extract all the source and target BCV values from the
            // entire set of VerseMapping Verses (excluding any having
            // TokenVerseAssociations) so we can query the Tokens table with them.

            var verseMappingsByBCV = tokenBCVVerseMappings
                .SelectMany(e => e!.Verses
                    .Where(v => v.CorpusId == corpusId)
                    .Where(v => tokenBCVs.Contains(v.BBBCCCVVV!))
                    .Select(v => v))
                .Where(v => !allVerseTVAs.Select(e => e.VerseId).Contains(v.Id))
                .Select(v => (v.VerseMappingId, BBBCCCVVV: v.BBBCCCVVV!))
                .GroupBy(v => v.BBBCCCVVV)
                .ToDictionary(g => g.Key, g => g.Select(v => v.VerseMappingId).Distinct());

            // Now we have the full universe of relevant VerseMappings (found by BCV relationship
            // and TokenVerseAssociation relationship to source and target tokens), captured in:
            // - verseMappingsByBCV
            // - tokenTVAs

            // Now that we have a set of VerseMappings that relate (by BCV or TVA)
            // back to the initial source and target tokens, query ALL tokens related
            // to these VerseMappings by BCV (TokenVerseAssociation related Tokens
            // are already in "allVerseTVAs") so that we can provide a full verse
            // context worth of tokens

            // Retrieve all of the source tokenized corpus tokens (related to the
            // VerseMappings by BCV) and join them with their BCV's VerseMappingIds
            var tokensForVerseBCVs = projectDbContext.Tokens
                .Include(e => e.TokenComposites)
                .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                .Where(e => verseMappingsByBCV.Keys.Distinct().Contains(e.EngineTokenId!.Substring(0, 9)))
                .AsNoTrackingWithIdentityResolution()
                .ToList()
                .Join(
                    verseMappingsByBCV,
                    t => t.EngineTokenId!.Substring(0, 9),
                    sv => sv.Key,
                    (t, vm) => (Token: t, vm.Value)
                )
                .ToList();

            sw.Stop();
            logger.LogInformation("Elapsed={0} - All tokens for relevant VerseMappings by BCV database query [count: {1}]", sw.Elapsed, tokensForVerseBCVs.Count);
            sw.Restart();

            var tokensByVerseMappingId = AssembleVerseMappingTokens(
                tokensForVerseBCVs,
                allVerseTVAs,
                tokenizedCorpusId);

            sw.Stop();
            logger.LogInformation("Elapsed={0} - Assemble VerseMapping Tokens", sw.Elapsed);
            sw.Restart();

            var tokenComponentIdToVerseMappingIds = BuildTokenComponentIdToVerseMappingIds(tokenIdBCVs, verseMappingsByBCV);

            var verseContexts = GetVerseContexts(tokens.Select(e => e.TokenId), tokenIdTVAs, tokenComponentIdToVerseMappingIds, tokensByVerseMappingId)
                .ToDictionary(
                    e => e.Item1,
                    e => (e.Item2.TokenTrainingTextVerseTokens.Select(t => ModelHelper.BuildToken(t)), e.Item2.TokenTrainingTextTokensIndex),
                    new IIdEqualityComparer());

            return verseContexts!;
        }


        private static IEnumerable<(TokenId, (IEnumerable<Models.TokenComponent> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex))> GetVerseContexts(
            IEnumerable<TokenId> tokenIds,
            IDictionary<Guid, IEnumerable<Models.TokenVerseAssociation>> tokenComponentIdTVAs,
            IDictionary<Guid, IEnumerable<Guid>> tokenComponentIdVerseMappingIds,
            IDictionary<Guid, List<Models.TokenComponent>> verseMappingIdTokens)
        {
            var verseContextsByTokenComponentId = new List<(TokenId, (IEnumerable<Models.TokenComponent> TokenTrainingTextVerseTokens, uint TokenTrainingTextTokensIndex))>();

            foreach (var tokenId in tokenIds)
            {
                if (tokenComponentIdTVAs.TryGetValue(tokenId.Id, out var tvas))
                {
                    if (verseMappingIdTokens.TryGetValue(tvas.First().Verse!.VerseMappingId, out var tokenTrainingTextVerseTokens))
                    {
                        tokenTrainingTextVerseTokens = tokenTrainingTextVerseTokens.OrderBy(e => e.EngineTokenId).ToList();
                        var tokenIndex = FindTokenIndex(tokenId.Id, tokenTrainingTextVerseTokens);
                        if (tokenIndex >= 0)
                        {
                            verseContextsByTokenComponentId.Add((tokenId, (tokenTrainingTextVerseTokens, (uint)tokenIndex)));
                            continue;
                        }
                    }

                }
                else if (tokenComponentIdVerseMappingIds.TryGetValue(tokenId.Id, out var verseMappingIds))
                {
                    // Else if BCV, find the first match in source/targetVerseMappingIdBCVPairs
                    // to get VerseMapping Id and use the source-or-target tokens from that
                    // VerseMapping
                    if (verseMappingIdTokens.TryGetValue(verseMappingIds.First(), out var tokenTrainingTextVerseTokens))
                    {
                        tokenTrainingTextVerseTokens = tokenTrainingTextVerseTokens.OrderBy(e => e.EngineTokenId).ToList();
                        var tokenIndex = FindTokenIndex(tokenId.Id, tokenTrainingTextVerseTokens);
                        if (tokenIndex >= 0)
                        {
                            verseContextsByTokenComponentId.Add((tokenId, (tokenTrainingTextVerseTokens, (uint)tokenIndex)));
                            continue;
                        }
                    }
                }

                // Maybe log this instead?
                //throw new Exception($"Token component of type {tokenComponent.GetType().Name} having Id '{tokenComponent.Id}' not found in any verse context!");
            }

            return verseContextsByTokenComponentId;
        }

        /// <summary>
        /// Any incoming tokens of type TokenComposite should have their
        /// child Tokens included
        /// </summary>
        /// <param name="tokenId"></param>
        /// <param name="tokenComponents"></param>
        /// <returns></returns>
        private static int FindTokenIndex(Guid tokenId, List<Models.TokenComponent> tokenComponents)
        {
            var tokenIndex = tokenComponents.FindIndex(t => t.Id == tokenId);
            if (tokenIndex == -1)
            {
                foreach (var (e, i) in tokenComponents.Select((e, i) => (e, i)))
                {
                    if (e.GetType() == typeof(Models.TokenComposite) &&
                        ((Models.TokenComposite)e).Tokens.ToList().FindIndex(e => e.Id == tokenId) >= 0)
                    {
                        tokenIndex = i;
                        break;
                    }
                }
            }

            return tokenIndex;
        }

        private static IDictionary<Guid, IEnumerable<Guid>> BuildTokenComponentIdToVerseMappingIds(IDictionary<Guid, IEnumerable<string>> tokenIdBCVs, IDictionary<string, IEnumerable<Guid>> verseMappingsByBCV)
        {
            var tokenComponentIdToVerseMappingIds = tokenIdBCVs
                .SelectMany(e => e.Value
                    .Select(bcv => (TokenComponentId: e.Key, BCV: bcv)))
                .Join(
                    verseMappingsByBCV,
                    st => st.BCV,
                    sv => sv.Key,
                    (st, sv) => (st.TokenComponentId, VerseMappingIds: sv.Value)
                )
                .ToDictionary(e => e.TokenComponentId, e => e.VerseMappingIds);

            return tokenComponentIdToVerseMappingIds;
        }

        private static IDictionary<Guid, IEnumerable<Models.TokenVerseAssociation>> GetAllRelatedTVAs(IEnumerable<Token> tokens, ProjectDbContext projectDbContext)
        {
            // Combine them all so we can find TokenVerseAssociations
            // related to any/all of them:
            var allTokenComponentIds = tokens
                .Select(e => e.TokenId.Id)
                .Union(tokens
                    .Where(e => e.GetType() == typeof(CompositeToken))
                    .SelectMany(e => ((CompositeToken)e).Tokens.Select(t => t.TokenId.Id)))
                .Distinct();

            // Here we query the database for TokenVerseAssociation data that
            // includes related VerseMappings and all their child Verses (we're
            // pulling TokenVerseAssociations from the incoming Token side):

            // It might turn out to be more efficient to query ALL TokenVerseAssociations
            // and filter by token after materializing using ToList()
            var allTVAsByTokenComponentId = projectDbContext.TokenVerseAssociations
                .Include(e => e.Verse)
                    .ThenInclude(e => e!.VerseMapping)
                        .ThenInclude(e => e!.Verses)
                .Include(e => e.TokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => allTokenComponentIds.Contains(e.TokenComponentId))
                .GroupBy(e => e.TokenComponentId)
                .ToDictionary(g => g.Key, g => g.Select(e => e));

            return allTVAsByTokenComponentId;
        }

        private static IDictionary<Guid, IEnumerable<string>> GetAllRelatedBCVs(IEnumerable<Token> tokens, IEnumerable<Guid> tokenComponentIdsForTVAs)
        {
            var tokenComponentBCVs = tokens
                .Where(t => t.GetType() == typeof(Token))
                .Where(t => !tokenComponentIdsForTVAs.Contains(t.TokenId.Id))
                .Select(t => (TokenComponentId: t.TokenId.Id, BCV: $"{t.TokenId.BookNumber:000}{t.TokenId.ChapterNumber:000}{t.TokenId.VerseNumber:000}"));

            // Give composites all the BCAs of their children:
            var compositeChildrenBCVs = tokens
                .Where(tc => tc.GetType() == typeof(CompositeToken))
                .SelectMany(tc => ((CompositeToken)tc).Tokens
                    .Where(t => !tokenComponentIdsForTVAs.Contains(t.TokenId.Id))
                    .Select(t => (TokenComponentId: tc.TokenId.Id, BCV: $"{t.TokenId.BookNumber:000}{t.TokenId.ChapterNumber:000}{t.TokenId.VerseNumber:000}")));

            var relatedBCVsByTokenComponentId = tokenComponentBCVs.Union(compositeChildrenBCVs)
                .GroupBy(bcv => bcv.TokenComponentId)
                .ToDictionary(g => g.Key, g => g.Select(bcv => bcv.BCV));

            return relatedBCVsByTokenComponentId;
        }

        private static IDictionary<Guid, List<Models.TokenComponent>> AssembleVerseMappingTokens(List<(Models.Token Token, IEnumerable<Guid> VerseMappingIds)> tokensForVerseBCVs, List<Models.TokenVerseAssociation> allVerseTVAs, Guid tokenizedCorpusId)
        {
            // Collect all the VerseMapping source tokens by VerseMapping Id
            // (several steps):
            // 1: TVA composites:
            var verseMappingIdSourceTokens = allVerseTVAs
                .Where(tva => tva.TokenComponent!.GetType() == typeof(Models.TokenComposite))
                .Where(tva => tva.TokenComponent!.TokenizedCorpusId == tokenizedCorpusId)
                .Select(tva => (tva.Verse!.VerseMappingId, TokenComponent: tva.TokenComponent!))
                .ToList();

            // 2: TVA tokens as composite children to composites:
            verseMappingIdSourceTokens.AddRange(allVerseTVAs
                .Where(tva => tva.TokenComponent!.GetType() == typeof(Models.Token))
                .Where(tva => ((Models.Token)tva.TokenComponent!).TokenComposites.Any())
                .Where(tva => tva.TokenComponent!.TokenizedCorpusId == tokenizedCorpusId)
                .SelectMany(tva => ((Models.Token)tva.TokenComponent!).TokenComposites
                    .Select(tc => (tva.Verse!.VerseMappingId, (Models.TokenComponent)tc))));

            // 3: TVA tokens not part of any composites:
            verseMappingIdSourceTokens.AddRange(allVerseTVAs
                .Where(tva => tva.TokenComponent!.GetType() == typeof(Models.Token))
                .Where(tva => !((Models.Token)tva.TokenComponent!).TokenComposites.Any())
                .Where(tva => tva.TokenComponent!.TokenizedCorpusId == tokenizedCorpusId)
                .Select(tva => (tva.Verse!.VerseMappingId, TokenComponent: tva.TokenComponent!)));

            // 4: BCV tokens as composite children to composites:
            verseMappingIdSourceTokens.AddRange(tokensForVerseBCVs
                .Where(e => e.Token.TokenComposites.Any())
                .SelectMany(e => e.Token.TokenComposites
                    .Select(tc => (e.VerseMappingIds, TokenComponent: (Models.TokenComponent)tc)))
                .SelectMany(e => e.VerseMappingIds
                    .Select(v => (VerseMappingId: v, e.TokenComponent))));

            // 5. BCV tokens not part of any composites (regular tokens - likely
            //    this is most/all of the VerseMapping tokens:
            verseMappingIdSourceTokens.AddRange(tokensForVerseBCVs
                .Where(e => !e.Token.TokenComposites.Any())
                .SelectMany(t => (t.VerseMappingIds.Select(v => (VerseMappingId: v, TokenComponent: (Models.TokenComponent)t.Token)))));

            verseMappingIdSourceTokens = verseMappingIdSourceTokens
                .DistinctBy(e => (e.VerseMappingId, e.TokenComponent.Id))
                .ToList();

            var tokensByVerseMappingId = verseMappingIdSourceTokens
                .GroupBy(e => e.VerseMappingId)
                .ToDictionary(g => g.Key, g => g.Select(e => e.TokenComponent).ToList());

            //var versesTokensByVerseMappingId = verseMappingIdSourceTokens
            //    .GroupBy(e => e.VerseMappingId)
            //    .GroupJoin(
            //        verseMappingIdBCVPairs,
            //        g => g.Key,
            //        g2 => g2.VerseMappingId,
            //        (g, g2) => (VerseMappingId: g.Key, BCVs: g2.Select(v => v.BBBCCCVVV!), TokenComponents: g.Select(e => e.TokenComponent))
            //    )
            //    .ToDictionary(g => g.VerseMappingId, g => (g.BCVs, g.TokenComponents));

            return tokensByVerseMappingId;
        }

    }
}
