using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ModelVerificationType = ClearDashboard.DataAccessLayer.Models.AlignmentVerification;
using ModelOriginatedType = ClearDashboard.DataAccessLayer.Models.AlignmentOriginatedFrom;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Diagnostics;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class CreateAlignmentSetCommandHandler : ProjectDbContextCommandHandler<CreateAlignmentSetCommand,
        RequestResult<AlignmentSet>, AlignmentSet>
    {
        private readonly IMediator _mediator;

        public CreateAlignmentSetCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateAlignmentSetCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<AlignmentSet>> SaveDataAsync(CreateAlignmentSetCommand request,
            CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var sourceTokenIds = request.Alignments.Select(al => al.AlignedTokenPair.SourceToken.TokenId.Id);
            var targetTokenIds = request.Alignments.Select(al => al.AlignedTokenPair.TargetToken.TokenId.Id);

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Query parallel corpus (with {sourceTokenIds.Count()} source tokens) (start)");
            sw.Restart();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Logger.LogInformation($"Private memory usage (BEFORE PARALLELCORPUS + TOKENS QUERY): {proc.PrivateMemorySize64}");
#endif

            var parallelCorpus = ProjectDbContext!.ParallelCorpa
                .Include(pc => pc.User)
                .Include(pc => pc.SourceTokenizedCorpus)
                    .ThenInclude(tc => tc!.User)
                .Include(pc => pc.TargetTokenizedCorpus)
                    .ThenInclude(tc => tc!.User)
                // -------------------------------------------------------
                // For manuscript -> zz_sur, including the tokens in this
                // query was making it take upward of 15 minutes.
                // Removing this means we are unable to validate that all
                // alignment tokens are actually in the given parallel 
                // corpus (as source or target tokens, respectively).
                // -------------------------------------------------------
                //.Include(pc => pc.SourceTokenizedCorpus)
                //    .ThenInclude(stc => stc!.TokenComponents /*.Where(tc => sourceTokenIds.Contains(tc.Id)) */)
                //.Include(pc => pc.TargetTokenizedCorpus)
                //    .ThenInclude(ttc => ttc!.TokenComponents /*.Where(tc => targetTokenIds.Contains(tc.Id)) */)
                .FirstOrDefault(c => c.Id == request.ParallelCorpusId.Id);

#if DEBUG
            proc.Refresh();
            Logger.LogInformation($"Private memory usage (AFTER PARALLELCORPUS + TOKENS QUERY):  {proc.PrivateMemorySize64}");

            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Query parallel corpus (end)");
#endif

            if (parallelCorpus == null)
            {
                return new RequestResult<AlignmentSet>
                (
                    success: false,
                    message: $"Invalid ParallelCorpusId '{request.ParallelCorpusId.Id}' found in request"
                );
            }

#if DEBUG
            //sw.Restart();
#endif

            //var notFoundSourceTokens = sourceTokenIds
            //    .Except(parallelCorpus!.SourceTokenizedCorpus!.TokenComponents
            //        .Select(tc => tc.Id));

            //if (notFoundSourceTokens.Any())
            //{
            //    return new RequestResult<AlignmentSet>
            //    (
            //        success: false,
            //        message: $"Requested alignment token pair source Id(s) not found in parallel corpus: '{string.Join(",", notFoundSourceTokens)}'"
            //    );
            //}

            //var notFoundTargetTokens = targetTokenIds
            //    .Except(parallelCorpus!.TargetTokenizedCorpus!.TokenComponents
            //        .Select(tc => tc.Id));

            //if (notFoundTargetTokens.Any())
            //{
            //    return new RequestResult<AlignmentSet>
            //    (
            //        success: false,
            //        message: $"Requested alignment token pair target Id(s) not found in parallel corpus: '{string.Join(",", notFoundTargetTokens)}'"
            //    );
            //}

#if DEBUG
            //sw.Stop();
            //Logger.LogInformation($"Elapsed={sw.Elapsed} - Check source/target tokens (end)");
            sw.Restart();

            proc.Refresh();
            Logger.LogInformation($"Private memory usage (BEFORE BULK INSERT): {proc.PrivateMemorySize64}");
#endif

            var verificationTypes = new Dictionary<string, ModelVerificationType>();
            var originatedTypes = new Dictionary<string, ModelOriginatedType>();
            foreach (var al in request.Alignments)
            {
                if (Enum.TryParse(al.Verification, out ModelVerificationType verificationType))
                {
                    verificationTypes[al.Verification] = verificationType;
                }
                else
                {
                    return new RequestResult<AlignmentSet>
                    (
                        success: false,
                        message: $"Invalid alignment verification type '{al.Verification}' found in request"
                    );
                }

                if (Enum.TryParse(al.OriginatedFrom, out ModelOriginatedType originatedType))
                {
                    originatedTypes[al.OriginatedFrom] = originatedType;
                }
                else
                {
                    return new RequestResult<AlignmentSet>
                    (
                        success: false,
                        message: $"Invalid alignment originated from type '{al.OriginatedFrom}' found in request"
                    );
                }
            }

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var alignmentSetModel = new Models.AlignmentSet
                {
                    ParallelCorpusId = request.ParallelCorpusId.Id,
                    DisplayName = request.DisplayName,
                    SmtModel = request.SmtModel,
                    IsSyntaxTreeAlignerRefined = request.IsSyntaxTreeAlignerRefined,
                    Metadata = request.Metadata,
                    //DerivedFrom = ,
                    //EngineWordAlignment = ,
                    Alignments = request.Alignments
                        .Select(al => new Models.Alignment
                        {
                            SourceTokenComponentId = al.AlignedTokenPair.SourceToken.TokenId.Id,
                            TargetTokenComponentId = al.AlignedTokenPair.TargetToken.TokenId.Id,
                            Score = al.AlignedTokenPair.Score,
                            AlignmentVerification = verificationTypes[al.Verification],
                            AlignmentOriginatedFrom = originatedTypes[al.OriginatedFrom]
                        }).ToList()
                };

                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var transaction = await ProjectDbContext.Database.GetDbConnection().BeginTransactionAsync(cancellationToken);

                using var alignmentSetInsertCommand = CreateAlignmentSetInsertCommand();
                using var alignmentInsertCommand = CreateAlignmentInsertCommand();

                var alignmentSetId = await InsertAlignmentSetAsync(
                    alignmentSetModel, 
                    alignmentSetInsertCommand, 
                    alignmentInsertCommand, 
                    cancellationToken);

                await transaction.CommitAsync(cancellationToken);

#if DEBUG
                proc.Refresh();
                Logger.LogInformation($"Private memory usage (AFTER BULK INSERT): {proc.PrivateMemorySize64}");

                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

                var alignmentSetFromDb = ProjectDbContext!.AlignmentSets
                    .Include(ast => ast.User)
                    .First(ast => ast.Id == alignmentSetId);

                var parallelCorpusId = ModelHelper.BuildParallelCorpusId(parallelCorpus);

                return new RequestResult<AlignmentSet>(new AlignmentSet(
                    ModelHelper.BuildAlignmentSetId(alignmentSetFromDb, parallelCorpusId, alignmentSetFromDb.User!),
                    parallelCorpusId,
                    _mediator));

            }
            catch (Exception e)
            {
                return new RequestResult<Alignment.Translation.AlignmentSet>
                (
                    success: false,
                    message: e.Message
                );
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }

        private DbCommand CreateAlignmentSetInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "ParallelCorpusId", "DisplayName", "SmtModel", "IsSyntaxTreeAlignerRefined", "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.AlignmentSet), columns);

            return command;
        }

        private async Task<Guid> InsertAlignmentSetAsync(Models.AlignmentSet alignmentSet, DbCommand alignmentSetCommand, DbCommand alignmentCommand, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            var alignmentSetId = (Guid.Empty != alignmentSet.Id) ? alignmentSet.Id : Guid.NewGuid();

            alignmentSetCommand.Parameters["@Id"].Value = alignmentSetId;
            alignmentSetCommand.Parameters["@ParallelCorpusId"].Value = alignmentSet.ParallelCorpusId;
            alignmentSetCommand.Parameters["@DisplayName"].Value = alignmentSet.DisplayName;
            alignmentSetCommand.Parameters["@SmtModel"].Value = alignmentSet.SmtModel;
            alignmentSetCommand.Parameters["@IsSyntaxTreeAlignerRefined"].Value = alignmentSet.IsSyntaxTreeAlignerRefined;
            alignmentSetCommand.Parameters["@Metadata"].Value = JsonSerializer.Serialize(alignmentSet.Metadata);
            alignmentSetCommand.Parameters["@UserId"].Value = Guid.Empty != alignmentSet.UserId ? alignmentSet.UserId : ProjectDbContext.UserProvider!.CurrentUser!.Id;
            alignmentSetCommand.Parameters["@Created"].Value = converter.ConvertToProvider(alignmentSet.Created);

            _ = await alignmentSetCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            foreach (var alignment in alignmentSet.Alignments)
            {
                await InsertAlignmentAsync(alignment, alignmentSetId, alignmentCommand, cancellationToken);
            }

            return alignmentSetId;
        }

        private DbCommand CreateAlignmentInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "SourceTokenComponentId", "TargetTokenComponentId", "AlignmentVerification", "AlignmentOriginatedFrom", "Score", "AlignmentSetId", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.Alignment), columns);

            return command;
        }

        private async Task InsertAlignmentAsync(Models.Alignment alignment, Guid alignmentSetId, DbCommand alignmentCommand, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            alignmentCommand.Parameters["@Id"].Value = (Guid.Empty != alignment.Id) ? alignment.Id : Guid.NewGuid();
            alignmentCommand.Parameters["@SourceTokenComponentId"].Value = alignment.SourceTokenComponentId;
            alignmentCommand.Parameters["@TargetTokenComponentId"].Value = alignment.TargetTokenComponentId;
            alignmentCommand.Parameters["@AlignmentVerification"].Value = alignment.AlignmentVerification.ToString();
            alignmentCommand.Parameters["@AlignmentOriginatedFrom"].Value = alignment.AlignmentOriginatedFrom.ToString();
            alignmentCommand.Parameters["@Score"].Value = alignment.Score;
            alignmentCommand.Parameters["@AlignmentSetId"].Value = alignmentSetId;
            alignmentCommand.Parameters["@UserId"].Value = Guid.Empty != alignment.UserId ? alignment.UserId : ProjectDbContext.UserProvider!.CurrentUser!.Id;
            alignmentCommand.Parameters["@Created"].Value = converter.ConvertToProvider(alignment.Created);
            _ = await alignmentCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static void ApplyColumnsToCommand(DbCommand command, Type type, string[] columns)
        {
            command.CommandText =
            $@"
                INSERT INTO {type.Name} ({string.Join(", ", columns)})
                VALUES ({string.Join(", ", columns.Select(c => "@" + c))})
            ";

            foreach (var column in columns)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{column}";
                command.Parameters.Add(parameter);
            }
        }
    }
}