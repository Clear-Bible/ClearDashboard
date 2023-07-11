using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Documents;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class GetAlignmentVerseContextsQueryHandler : ProjectDbContextQueryHandler<
        GetAlignmentVerseContextsQuery,
        RequestResult<(IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )> VerseContexts, string Cursor, bool HasNextPage)>,
        (IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )> VerseContexts, string Cursor, bool HasNextPage)>
    {

        public GetAlignmentVerseContextsQueryHandler(ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider, ILogger<GetAlignmentVerseContextsQueryHandler> logger) 
            : base(projectNameDbContextFactory, projectProvider, logger)
        {
        }

        protected override async Task<RequestResult<(IEnumerable<(
            Alignment.Translation.Alignment alignment,
            IEnumerable<Token> sourceVerseTokens,
            uint sourceVerseTokensIndex,
            IEnumerable<Token> targetVerseTokens,
            uint targetVerseTokensIndex
        )> VerseContexts, string Cursor, bool HasNextPage)>> GetDataAsync(GetAlignmentVerseContextsQuery request, CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var alignmentSet = await ProjectDbContext!.AlignmentSets
                .Include(e => e.ParallelCorpus)
                    .ThenInclude(e => e!.SourceTokenizedCorpus)
                .Include(e => e.ParallelCorpus)
                    .ThenInclude(e => e!.TargetTokenizedCorpus)
                .Where(e => e.Id == request.AlignmentSetId.Id)
                .FirstOrDefaultAsync();

            if (alignmentSet == null)
            {
                //sw.Stop();
               // Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end) [error]");

                return new RequestResult<(IEnumerable<(
                    Alignment.Translation.Alignment alignment,
                    IEnumerable<Token> sourceVerseTokens,
                    uint sourceVerseTokensIndex,
                    IEnumerable<Token> targetVerseTokens,
                    uint targetVerseTokensIndex
                )> VerseContexts, string Cursor, bool HasNextPage)>
                (
                    success: false,
                    message: $"AlignmentSet not found for AlignmentSetId '{request.AlignmentSetId.Id}'"
                );
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - AlignmentSet database query", sw.Elapsed);
            sw.Restart();
#endif

            string afterSourceEngineTokenId = string.Empty;
            string afterTargetEngineTokenId = string.Empty;
            try
            {
                (afterSourceEngineTokenId, afterTargetEngineTokenId) = ExtractCursorValues(request.Cursor);
            }
            catch (ArgumentException ex)
            {
                return new RequestResult<(IEnumerable<(
                    Alignment.Translation.Alignment alignment,
                    IEnumerable<Token> sourceVerseTokens,
                    uint sourceVerseTokensIndex,
                    IEnumerable<Token> targetVerseTokens,
                    uint targetVerseTokensIndex
                )> VerseContexts, string Cursor, bool HasNextPage)>
                (
                    success: false,
                    message: ex.Message
                );
            }

            var filteredDatabaseAlignments = (request.HasLimit() || !string.IsNullOrEmpty(afterSourceEngineTokenId) && !string.IsNullOrEmpty(afterTargetEngineTokenId))
                ? await GetAlignmentsLimitCursorAsync(alignmentSet, request, afterSourceEngineTokenId, afterTargetEngineTokenId, cancellationToken)
                : await GetAlignmentsNoCursorAsync(alignmentSet, request, cancellationToken);

            var endCursor = string.Empty;
            var hasNextPage = false;

            if (request.HasLimit())
            {
                (endCursor, hasNextPage) = BuildEndCursorValues(filteredDatabaseAlignments, request);
                filteredDatabaseAlignments = filteredDatabaseAlignments.Take(request.Limit!.Value);
            }

            var alignments = filteredDatabaseAlignments
                .Select(e => new Alignment.Translation.Alignment(
                    ModelHelper.BuildAlignmentId(
                        e,
                        alignmentSet.ParallelCorpus!.SourceTokenizedCorpus!,
                        alignmentSet.ParallelCorpus!.TargetTokenizedCorpus!,
                        e.SourceTokenComponent!),
                    new AlignedTokenPairs(
                        ModelHelper.BuildToken(e.SourceTokenComponent!),
                        ModelHelper.BuildToken(e.TargetTokenComponent!),
                        e.Score),
                    e.AlignmentVerification.ToString(),
                    e.AlignmentOriginatedFrom.ToString()))
                .ToList();

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Convert to API alignments", sw.Elapsed);
            sw.Restart();
#endif

            /// Weird possible scenario - is this valid?
            /// VerseMapping
            ///     Verse 001002004 (source) - TokenVerseAssociation - [some token - "TokenB"]
            ///     Verse 001002004 (target)
            ///
            /// Such that there is a source corpus token "TokenA" that would normally be
            /// in the source verse (by having a matching BBBCCCVVV value), but isn't because of the
            /// TokenVerseAssociation.  So what do I return if one of the Alignments' SourceTokens
            /// points to "TokenA"? There won't be a VerseContext since "TokenA" effectively isn't
            /// in the verse mappings...

            var alignmentSourceTokens = alignments.Select(e => e.AlignedTokenPair.SourceToken).Distinct().ToList();
            var alignmentTargetTokens = alignments.Select(e => e.AlignedTokenPair.TargetToken).Distinct().ToList();

            var sourceTokenVerseContexts = TokenVerseContextFinder.GetTokenVerseContexts(
                alignmentSet.ParallelCorpus!,
                alignmentSourceTokens,
                true,
                ProjectDbContext!,
                Logger);

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Source token verse contexts [count: {1}]", sw.Elapsed, sourceTokenVerseContexts.Count);
            sw.Restart();
#endif

            var targetTokenVerseContexts = TokenVerseContextFinder.GetTokenVerseContexts(
                alignmentSet.ParallelCorpus!,
                alignmentTargetTokens,
                false,
                ProjectDbContext!,
                Logger);

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Target token verse contexts [count: {1}]", sw.Elapsed, targetTokenVerseContexts.Count);
            sw.Restart();
#endif

            List<(
                Alignment.Translation.Alignment Alignment,
                IEnumerable<Token> sourceVerseTokens,
                uint sourceVerseTokensIndex,
                IEnumerable<Token> targetVerseTokens,
                uint targetVerseTokensIndex)> alignmentVerseContexts = new();

            foreach (var alignment in alignments)
            {
                if (sourceTokenVerseContexts.TryGetValue(alignment.AlignedTokenPair.SourceToken.TokenId, out var sourceVerseContext) &&
                    targetTokenVerseContexts.TryGetValue(alignment.AlignedTokenPair.TargetToken.TokenId, out var targetVerseContext))
                {
                    alignmentVerseContexts.Add((
                        alignment,
                        sourceVerseContext.VerseTokens,
                        sourceVerseContext.VerseTokensIndex,
                        targetVerseContext.VerseTokens,
                        targetVerseContext.VerseTokensIndex));
                }
                else
                {
                    if (!sourceTokenVerseContexts.ContainsKey(alignment.AlignedTokenPair.SourceToken.TokenId))
                    {
                        Logger.LogError($"No verse context found for alignment source token {alignment.AlignedTokenPair.SourceToken.TokenId}");
                    }
                    if (!targetTokenVerseContexts.ContainsKey(alignment.AlignedTokenPair.TargetToken.TokenId))
                    {
                        Logger.LogError($"No verse context found for alignment target token {alignment.AlignedTokenPair.TargetToken.TokenId}");
                    }
                }
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif
            return new RequestResult<(IEnumerable<(
                Alignment.Translation.Alignment alignment,
                IEnumerable<Token> sourceVerseTokens,
                uint sourceVerseTokensIndex,
                IEnumerable<Token> targetVerseTokens,
                uint targetVerseTokensIndex
            )> VerseContexts, string Cursor, bool HasNextPage)>((VerseContexts: alignmentVerseContexts, Cursor: endCursor, HasNextPage: hasNextPage));
        }

        private async Task<IEnumerable<Models.Alignment>> GetAlignmentsLimitCursorAsync(Models.AlignmentSet alignmentSet, GetAlignmentVerseContextsQuery request, string afterSourceEngineTokenId, string afterTargetEngineTokenId, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} -  GetAlignmentsLimitCursorAsync", sw.Elapsed);
            sw.Restart();
#endif
            var alignmentsById = new Dictionary<Guid, Models.Alignment>();
            var tokenCompositeIdToAlignmentIdMapping = new Dictionary<Guid, (bool IsSource, Guid AlignmentId)>();

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                await using var connection = ProjectDbContext.Database.GetDbConnection();

                var alignmentCommand = BuildAlignmentSelectQueryCommand(connection, request, afterSourceEngineTokenId, afterTargetEngineTokenId); ;
                
                
                await using (DbDataReader dataReader = await alignmentCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                {

#if DEBUG
                    sw.Stop();
                    Logger.LogInformation("Elapsed={0} -  GetAlignmentsLimitCursorAsync, Executed ReaderAsync", sw.Elapsed);
                    sw.Restart();
#endif
                    while (await dataReader.ReadAsync(cancellationToken))
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            await dataReader.CloseAsync();
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        var alignmentId = dataReader.GetGuid(0);
                        if (!alignmentsById.TryGetValue(alignmentId, out var alignment))
                        {
                            alignment = new Models.Alignment
                            {
                                Id = dataReader.GetGuid(0),
                                AlignmentSetId = dataReader.GetGuid(1),
                                AlignmentOriginatedFrom = Enum.Parse<Models.AlignmentOriginatedFrom>(dataReader.GetString(2)),
                                AlignmentVerification = Enum.Parse<Models.AlignmentVerification>(dataReader.GetString(3))
                            };

                            alignmentsById.Add(alignment.Id, alignment);
                        }

                        var sourceTokenComponentId = dataReader.GetGuid(4);
                        if (dataReader.GetString(6) == "Token")
                        {
                            var sourceToken = new Models.Token
                            {
                                Id = sourceTokenComponentId,
                                EngineTokenId = dataReader.GetString(5),
                                BookNumber = dataReader.GetInt32(7),
                                ChapterNumber = dataReader.GetInt32(8),
                                VerseNumber = dataReader.GetInt32(9),
                                WordNumber = dataReader.GetInt32(10),
                                SubwordNumber = dataReader.GetInt32(11)
                            };

                            alignment.SourceTokenComponentId = sourceToken.Id;
                            alignment.SourceTokenComponent = sourceToken;
                        }
                        else
                        {
                            tokenCompositeIdToAlignmentIdMapping.Add(sourceTokenComponentId, (true, alignment.Id));
                        }

                        var targetTokenComponentId = dataReader.GetGuid(12);
                        if (dataReader.GetString(14) == "Token")
                        {
                            var targetToken = new Models.Token
                            {
                                Id = targetTokenComponentId,
                                EngineTokenId = dataReader.GetString(13),
                                BookNumber = dataReader.GetInt32(15),
                                ChapterNumber = dataReader.GetInt32(16),
                                VerseNumber = dataReader.GetInt32(17),
                                WordNumber = dataReader.GetInt32(18),
                                SubwordNumber = dataReader.GetInt32(19)
                            };

                            alignment.TargetTokenComponentId = targetToken.Id;
                            alignment.TargetTokenComponent = targetToken;
                        }
                        else
                        {
                            tokenCompositeIdToAlignmentIdMapping.Add(targetTokenComponentId, (false, alignment.Id));
                        }
                    }

                    await dataReader.CloseAsync();

#if DEBUG
                    sw.Stop();
                    Logger.LogInformation("Elapsed={0} -  GetAlignmentsLimitCursorAsync, Closed Reader", sw.Elapsed);
                    sw.Restart();
#endif
                }

                if (tokenCompositeIdToAlignmentIdMapping.Any())
                {
                    var compositeCommand = BuildTokenCompositeSelectQueryCommand(connection, tokenCompositeIdToAlignmentIdMapping.Keys);
                    var tokenCompositesById = await BuildTokenCompositesByIdFromReader(compositeCommand, cancellationToken);

                    foreach (var kvp in tokenCompositeIdToAlignmentIdMapping)
                    {
                        if (!tokenCompositesById.TryGetValue(kvp.Key, out var tokenComposite))
                        {
                            throw new Exception($"TokenComposite having Id '{kvp.Key}' not found in query result list");
                        }

                        if (kvp.Value.IsSource)
                        {
                            alignmentsById[kvp.Value.AlignmentId].SourceTokenComponent = tokenComposite;
                            alignmentsById[kvp.Value.AlignmentId].SourceTokenComponentId = tokenComposite.Id;
                        }
                        else
                        {
                            alignmentsById[kvp.Value.AlignmentId].TargetTokenComponent = tokenComposite;
                            alignmentsById[kvp.Value.AlignmentId].TargetTokenComponentId = tokenComposite.Id;
                        }
                    }
                }

#if DEBUG
                sw.Stop();
                Logger.LogInformation("Elapsed={0} -  GetAlignmentsLimitCursorAsync, Begin filtering", sw.Elapsed);
                sw.Restart();
#endif

                IEnumerable<Models.Alignment> filteredDatabaseAlignments = alignmentsById.Values
                    .WhereAlignmentTypesFilter(request.AlignmentTypesToInclude)
                    .OrderBy(e => e.SourceTokenComponent!.EngineTokenId)
                    .ThenBy(e => e.TargetTokenComponent!.EngineTokenId)
                    .ThenBy(e => (int)e.AlignmentOriginatedFrom);

#if DEBUG
                sw.Stop();
                Logger.LogInformation("Elapsed={0} -  GetAlignmentsLimitCursorAsync, Filtering complete", sw.Elapsed);
                sw.Restart();
#endif

                return filteredDatabaseAlignments;
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }

        private async Task<Dictionary<Guid, Models.TokenComposite>> BuildTokenCompositesByIdFromReader(DbCommand compositeCommand, CancellationToken cancellationToken)
        {
            var tokenCompositesById = new Dictionary<Guid, Models.TokenComposite>();

            await using (DbDataReader dataReader = await compositeCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
            {
                while (await dataReader.ReadAsync(cancellationToken))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        await dataReader.CloseAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    var tokenCompositeId = dataReader.GetGuid(1);
                    if (!tokenCompositesById.TryGetValue(tokenCompositeId, out var tokenComposite))
                    {
                        tokenComposite = new Models.TokenComposite
                        {
                            Id = tokenCompositeId,
                            EngineTokenId = dataReader.GetString(2),
                            Tokens = new List<Models.Token>(),
                            TokenCompositeTokenAssociations = new List<Models.TokenCompositeTokenAssociation>()
                        };
                        tokenCompositesById.Add(tokenCompositeId, tokenComposite);
                    }

                    var token = new Models.Token
                    {
                        Id = dataReader.GetGuid(3),
                        EngineTokenId = dataReader.GetString(4),
                        BookNumber = dataReader.GetInt32(5),
                        ChapterNumber = dataReader.GetInt32(6),
                        VerseNumber = dataReader.GetInt32(7),
                        WordNumber = dataReader.GetInt32(8),
                        SubwordNumber = dataReader.GetInt32(9)
                    };

                    var targetTokenCompositeAssociation = new Models.TokenCompositeTokenAssociation
                    {
                        Id = dataReader.GetGuid(0),
                        TokenId = token.Id,
                        Token = token,
                        TokenCompositeId = tokenComposite.Id,
                        TokenComposite = tokenComposite
                    };

                    tokenComposite.Tokens.Add(token);
                    tokenComposite.TokenCompositeTokenAssociations.Add(targetTokenCompositeAssociation);

                    token.TokenComposites.Add(tokenComposite);
                    token.TokenCompositeTokenAssociations.Add(targetTokenCompositeAssociation);
                }

                await dataReader.CloseAsync();
            }

            return tokenCompositesById;
        }

        private async Task<IEnumerable<Models.Alignment>> GetAlignmentsNoCursorAsync(Models.AlignmentSet alignmentSet, GetAlignmentVerseContextsQuery request, CancellationToken cancellationToken)
        {
            Stopwatch sw = new();
            sw.Start();

            // If having the Where(e.AlignmentSetId == ) clause results in the query 
            // running significantly slower, run "ANALYZE IX_Alignment_AlignmentSetId"
            // using a Sqlite database client.
            var databaseAlignmentsQueryable = ProjectDbContext.Alignments
                .Include(e => e.SourceTokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Include(e => e.TargetTokenComponent!)
                    .ThenInclude(e => ((Models.TokenComposite)e).Tokens)
                .Where(e => e.AlignmentSetId == request.AlignmentSetId.Id)
                .Where(e => e.Deleted == null);

            if (request.StringsAreTraining)
            {
                databaseAlignmentsQueryable = databaseAlignmentsQueryable
                    .Where(e => e.SourceTokenComponent!.TrainingText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.TrainingText == request.TargetString);
            }
            else
            {
                databaseAlignmentsQueryable = databaseAlignmentsQueryable
                    .Where(e => e.SourceTokenComponent!.SurfaceText == request.SourceString)
                    .Where(e => e.TargetTokenComponent!.SurfaceText == request.TargetString);
            }

            if (request.BookNumber is not null)
            {
                databaseAlignmentsQueryable = databaseAlignmentsQueryable
                    .Where(e => e.SourceTokenComponent!.GetType() != typeof(Models.Token) || ((Models.Token)e.SourceTokenComponent!).BookNumber == request.BookNumber)
                    .Where(e => e.SourceTokenComponent!.GetType() != typeof(Models.TokenComposite) || ((Models.TokenComposite)e.SourceTokenComponent!).Tokens.Any(t => t.BookNumber == request.BookNumber));
            }

            var databaseAlignments = await databaseAlignmentsQueryable
                .AsNoTrackingWithIdentityResolution()
                .ToListAsync(cancellationToken);

#if DEBUG
            sw.Stop();
            Logger.LogInformation("Elapsed={0} - Alignments+Tokens database query [count: {1}]", sw.Elapsed, databaseAlignments.Count);
            sw.Restart();
#endif

            var filteredDatabaseAlignments = databaseAlignments
                .WhereAlignmentTypesFilter(request.AlignmentTypesToInclude)
                .OrderBy(e => e.SourceTokenComponent!.EngineTokenId)
                .ThenBy(e => e.TargetTokenComponent!.EngineTokenId)
                .ThenBy(e => (int)e.AlignmentOriginatedFrom);

            return filteredDatabaseAlignments;
        }

        private static (string AfterSourceEngineTokenId, string AfterTargetEngineTokenId) ExtractCursorValues(string? cursor)
        {
            string afterSourceEngineTokenId = string.Empty;
            string afterTargetEngineTokenId = string.Empty;
            if (!string.IsNullOrEmpty(cursor))
            {
                var base64EncodedBytes = System.Convert.FromBase64String(cursor);
                var decodedCursorString = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);

                var parts = decodedCursorString.Split('_');
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Invalid cursor provided to GetAlignmentVerseContexts '{cursor}'");
                }

                afterSourceEngineTokenId = parts[0];
                afterTargetEngineTokenId = parts[1];
            }

            return (afterSourceEngineTokenId, afterTargetEngineTokenId);
        }

        private static (string EndCursor, bool HasNextPage) BuildEndCursorValues(IEnumerable<Models.Alignment> alignments, GetAlignmentVerseContextsQuery request)
        {
            string endCursor = string.Empty;
            bool hasNextPage = false;

            Models.Alignment? endCursorAlignment;
            if (request.HasLimit())
            {
                hasNextPage = alignments.Count() > request.Limit!.Value;
                endCursorAlignment = alignments.Skip(request.Limit!.Value - 1).FirstOrDefault();
            }
            else
            {
                endCursorAlignment = alignments.LastOrDefault();
            }

            if (endCursorAlignment is not null)
            {
                byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes($"{endCursorAlignment.SourceTokenComponent!.EngineTokenId!}_{endCursorAlignment.TargetTokenComponent!.EngineTokenId!}");
                endCursor = Convert.ToBase64String(toEncodeAsBytes);
            }

            return (endCursor, hasNextPage);
        }

        private static DbCommand BuildAlignmentSelectQueryCommand(DbConnection connection, GetAlignmentVerseContextsQuery request, string afterSourceEngineTokenId, string afterTargetEngineTokenId)
        {
            var command = connection.CreateCommand();

            var doBookNumber = request.BookNumber is not null && request.BookNumber.HasValue && request.BookNumber.Value > 0;
            var doCursor = !string.IsNullOrEmpty(afterSourceEngineTokenId) && !string.IsNullOrEmpty(afterTargetEngineTokenId);
            var doLimit = request.Limit is not null && request.Limit.HasValue && request.Limit.Value > 0;

            string? querySuffix;
            if (request.StringsAreTraining)
            {
                querySuffix =
                    " AND st.TrainingText = @SourceString" +
                    " AND tt.TrainingText = @TargetString";
            }
            else
            {
                querySuffix =
                    " AND st.SurfaceText = @SourceString" +
                    " AND tt.SurfaceText = @TargetString";
            }

            if (doCursor)
                querySuffix += " AND (st.EngineTokenId > @AfterSourceEngineTokenId OR (st.EngineTokenId = @AfterSourceEngineTokenId AND tt.EngineTokenId > @AfterTargetEngineTokenId))";

            if (doBookNumber)
                querySuffix += @" 
                    AND (st.BookNumber == @BookNumber OR
                    EXISTS (
                        SELECT 1
                        FROM TokenCompositeTokenAssociation AS ta
                        INNER JOIN (
                            SELECT ti.Id, ti.BookNumber
                            FROM TokenComponent AS ti
                            WHERE ti.Discriminator == 'Token'
                        ) AS tai ON ta.TokenId == tai.Id
                        WHERE (st.Id != NULL AND st.Id == ta.TokenCompositeId) AND tai.BookNumber == @BookNumber))";

            querySuffix += " ORDER BY SourceTokenComponentEngineTokenId, TargetTokenComponentEngineTokenId, AlignmentOriginatedFrom";

            if (doLimit)
                querySuffix += " LIMIT @Limit";

            command.CommandText = CommonAlignmentQuery + querySuffix;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@AlignmentSetId";
            command.Parameters.Add(parameter);

            parameter = command.CreateParameter();
            parameter.ParameterName = "@SourceString";
            command.Parameters.Add(parameter);

            parameter = command.CreateParameter();
            parameter.ParameterName = "@TargetString";
            command.Parameters.Add(parameter);

            if (doCursor)
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = "@AfterSourceEngineTokenId";
                command.Parameters.Add(parameter);
                parameter = command.CreateParameter();
                parameter.ParameterName = "@AfterTargetEngineTokenId";
                command.Parameters.Add(parameter);
            }

            if (doBookNumber)
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = "@BookNumber";
                command.Parameters.Add(parameter);
            }

            if (doLimit)
            {
                parameter = command.CreateParameter();
                parameter.ParameterName = "@Limit";
                command.Parameters.Add(parameter);
            }

            command.Prepare();

            command.Parameters["@AlignmentSetId"].Value = request.AlignmentSetId.Id;
            command.Parameters["@SourceString"].Value = request.SourceString;
            command.Parameters["@TargetString"].Value = request.TargetString;

            if (doCursor)
            {
                command.Parameters["@AfterSourceEngineTokenId"].Value = afterSourceEngineTokenId;
                command.Parameters["@AfterTargetEngineTokenId"].Value = afterTargetEngineTokenId;
            }

            if (doBookNumber)
                command.Parameters["@BookNumber"].Value = request.BookNumber;

            if (doLimit)
                command.Parameters["@Limit"].Value = request.Limit!.Value * 2 + 1;

            return command;
        }

        private static string CommonAlignmentQuery =>
            @"
            SELECT 
              a.Id, 
              a.AlignmentSetId,
              a.AlignmentOriginatedFrom, 
              a.AlignmentVerification, 
  
              st.Id as SourceTokenComponentId,
              st.EngineTokenId as SourceTokenComponentEngineTokenId, 
              st.Discriminator as SourceTokenComponentDiscriminator,
 
              st.BookNumber as SourceBookNumber, 
              st.ChapterNumber as SourceChapterNumber,
              st.VerseNumber as SourceVerseNumber,
              st.WordNumber as SourceWordNumber,
              st.SubwordNumber as SourceSubwordNumber,

              tt.Id as TargetTokenComponentId,
              tt.EngineTokenId as TargetTokenComponentEngineTokenId, 
              tt.Discriminator as TargetTokenComponentDiscriminator,
  
              tt.BookNumber as TargetBookNumber, 
              tt.ChapterNumber as TargetChapterNumber,
              tt.VerseNumber as TargetVerseNumber,
              tt.WordNumber as TargetWordNumber,
              tt.SubwordNumber as TargetSubwordNumber
            FROM Alignment a
            INNER JOIN TokenComponent st on st.Id = a.SourceTokenComponentId
            INNER JOIN TokenComponent tt on tt.Id = a.TargetTokenComponentId
            WHERE a.Deleted IS NULL
            AND a.AlignmentSetId = @AlignmentSetId";

        private static DbCommand BuildTokenCompositeSelectQueryCommand(DbConnection connection, IEnumerable<Guid> tokenCompositeIds)
        {
            if (connection is not SqliteConnection)
                throw new NotSupportedException("Querying via multiple token composite ids is not supported for non-SqliteConnections");

            var command = (SqliteCommand)connection.CreateCommand();

            if (tokenCompositeIds.Any())
            {
                var parameters = new string[tokenCompositeIds.Count()];

                var i = 0;
                foreach (var id in tokenCompositeIds)
                {
                    parameters[i] = string.Format("@TokenCompositeId{0}", i);
                    command.Parameters.AddWithValue(parameters[i], id);
                }

                command.CommandText = string.Format(CommonTokenCompositeQuery, string.Join(", ", parameters));
            }

            return command;
        }

        private static string CommonTokenCompositeQuery =>
            @"
            SELECT ta.Id, ta.TokenCompositeId, tc.EngineTokenId, tac.Id as ChildTokenId, tac.EngineTokenId as ChildEngineTokenId, tac.BookNumber as ChildBookNumber, tac.ChapterNumber as ChildChapterNumber, tac.VerseNumber as ChildVerseNumber, tac.WordNumber as ChildWordNumber, tac.SubwordNumber as ChildSubwordNumber
            FROM TokenCompositeTokenAssociation ta
            INNER JOIN TokenComponent tc on tc.Id = ta.TokenCompositeId
            INNER JOIN (
                SELECT tat.Id, tat.EngineTokenId, tat.BookNumber, tat.ChapterNumber, tat.VerseNumber, tat.WordNumber, tat.SubwordNumber
                FROM TokenComponent tat
                WHERE tat.Discriminator = 'Token'
            ) tac ON ta.TokenId = tac.Id
	        WHERE ta.TokenCompositeId in ({0})";
    }
}
