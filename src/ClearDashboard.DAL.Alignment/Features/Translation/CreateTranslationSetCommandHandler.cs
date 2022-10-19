using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Translation
{
    public class CreateTranslationSetCommandHandler : ProjectDbContextCommandHandler<CreateTranslationSetCommand,
        RequestResult<TranslationSet>, TranslationSet>
    {
        private readonly IMediator _mediator;

        public CreateTranslationSetCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<CreateTranslationSetCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<TranslationSet>> SaveDataAsync(CreateTranslationSetCommand request,
            CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var parallelCorpus = ModelHelper.AddIdIncludesParallelCorpaQuery(ProjectDbContext!)
                .FirstOrDefault(c => c.Id == request.ParallelCorpusId.Id);

#if DEBUG
            sw.Stop();
#endif

            if (parallelCorpus == null)
            {
                return new RequestResult<TranslationSet>
                (
                    success: false,
                    message: $"Invalid ParallelCorpusId '{request.ParallelCorpusId.Id}' found in request"
                );
            }

#if DEBUG
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Insert TranslationSet '{request.DisplayName}' and model (start) [translation model entry count: {request.TranslationModel?.Count() ?? 0}]");
            sw.Restart();
            Process proc = Process.GetCurrentProcess();

            proc.Refresh();
            Logger.LogInformation($"Private memory usage (BEFORE BULK INSERT): {proc.PrivateMemorySize64}");
#endif

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var translationSet = new Models.TranslationSet
                {
                    ParallelCorpusId = request.ParallelCorpusId.Id,
                    AlignmentSetId = request.alignmentSetId.Id,
                    DisplayName = request.DisplayName,
                    //SmtModel = request.SmtModel,
                    Metadata = request.Metadata,
                    //DerivedFrom = ,
                    //EngineWordAlignment = ,
                    TranslationModel = request.TranslationModel?
                        .Select(tm => new Models.TranslationModelEntry
                        {
                            SourceText = tm.Key,
                            TargetTextScores = tm.Value
                                .Select(tts => new Models.TranslationModelTargetTextScore
                                {
                                    Text = tts.Key,
                                    Score = tts.Value
                                }).ToList()
                        }).ToList() ?? new()
                };

                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var transaction = await ProjectDbContext.Database.GetDbConnection().BeginTransactionAsync(cancellationToken);

                using var translationSetInsertCommand = CreateTranslationSetInsertCommand();
                using var translationModelEntryInsertCommand = CreateTranslationModelEntryInsertCommand();
                using var translationModelTargetTextScoreInsertCommand = CreateTranslationModelTargetTextScoreInsertCommand();

                var translationSetId = await InsertTranslationSetAsync(
                    translationSet, 
                    translationSetInsertCommand, 
                    cancellationToken);

                foreach (var translationModelEntry in translationSet.TranslationModel)
                {
                    await InsertTranslationModelEntryAsync(
                        translationModelEntry,
                        translationSetId,
                        translationModelEntryInsertCommand,
                        translationModelTargetTextScoreInsertCommand,
                        cancellationToken);
                }

                await transaction.CommitAsync(cancellationToken);

#if DEBUG
                proc.Refresh();
                Logger.LogInformation($"Private memory usage (AFTER BULK INSERT): {proc.PrivateMemorySize64}");

                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

                var translationSetFromDb = ProjectDbContext!.TranslationSets
                    .Include(ts => ts.AlignmentSet)
                        .ThenInclude(ast => ast!.User)
                    .Include(ts => ts.User)
                    .First(ts => ts.Id == translationSetId);

                var parallelCorpusId = ModelHelper.BuildParallelCorpusId(parallelCorpus);

                return new RequestResult<TranslationSet>(new TranslationSet(
                    ModelHelper.BuildTranslationSetId(translationSetFromDb, parallelCorpusId, translationSetFromDb.User!),
                    parallelCorpusId,
                    ModelHelper.BuildAlignmentSetId(translationSetFromDb.AlignmentSet!, parallelCorpusId, translationSetFromDb.AlignmentSet!.User!),
                    request.TranslationModel != null ? true : false,
                    _mediator));

            }
            catch (Exception e)
            {
                return new RequestResult<Alignment.Translation.TranslationSet>
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

        private DbCommand CreateTranslationSetInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "ParallelCorpusId", "AlignmentSetId", /*"DerivedFrom", "EngineWordAlignmentId", */ "DisplayName", /*"SmtModel",*/ "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.TranslationSet), columns);

            return command;
        }

        private async Task<Guid> InsertTranslationSetAsync(Models.TranslationSet translationSet, DbCommand translationSetCommand, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            var translationSetId = (Guid.Empty != translationSet.Id) ? translationSet.Id : Guid.NewGuid();

            translationSetCommand.Parameters["@Id"].Value = translationSetId;
            translationSetCommand.Parameters["@ParallelCorpusId"].Value = translationSet.ParallelCorpusId;
            translationSetCommand.Parameters["@AlignmentSetId"].Value = translationSet.AlignmentSetId;
            //translationSetCommand.Parameters["@DerivedFrom"].Value = translationSet.DerivedFrom;
            //translationSetCommand.Parameters["@EngineWordAlignmentId"].Value = translationSet.EngineWordAlignmentId;
            translationSetCommand.Parameters["@DisplayName"].Value = translationSet.DisplayName;
            //translationSetCommand.Parameters["@SmtModel"].Value = translationSet.SmtModel;
            translationSetCommand.Parameters["@Metadata"].Value = JsonSerializer.Serialize(translationSet.Metadata);
            translationSetCommand.Parameters["@UserId"].Value = Guid.Empty != translationSet.UserId ? translationSet.UserId : ProjectDbContext.UserProvider!.CurrentUser!.Id;
            translationSetCommand.Parameters["@Created"].Value = converter.ConvertToProvider(translationSet.Created);

            _ = await translationSetCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return translationSetId;
        }

        private DbCommand CreateTranslationModelEntryInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "TranslationSetId", "SourceText" };

            ApplyColumnsToCommand(command, typeof(Models.TranslationModelEntry), columns);

            return command;
        }

        private async Task InsertTranslationModelEntryAsync(Models.TranslationModelEntry translationModelEntry, Guid translationSetId, DbCommand translationModelEntryCommand, DbCommand translationModelTargetTextScoreCommand, CancellationToken cancellationToken)
        {
            var translationModelEntryId = (Guid.Empty != translationModelEntry.Id) ? translationModelEntry.Id : Guid.NewGuid();

            translationModelEntryCommand.Parameters["@Id"].Value = translationModelEntryId;
            translationModelEntryCommand.Parameters["@TranslationSetId"].Value = translationSetId;
            translationModelEntryCommand.Parameters["@SourceText"].Value = translationModelEntry.SourceText;
            _ = await translationModelEntryCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            foreach (var translationModelTargetTextScore in translationModelEntry.TargetTextScores)
            {
                await InsertTranslationModelTargetTextScoreAsync(translationModelTargetTextScore, translationModelEntryId, translationModelTargetTextScoreCommand, cancellationToken);
            }
        }

        private DbCommand CreateTranslationModelTargetTextScoreInsertCommand()
        {
            var command = ProjectDbContext.Database.GetDbConnection().CreateCommand();
            var columns = new string[] { "Id", "TranslationModelEntryId", "Text", "Score" };

            ApplyColumnsToCommand(command, typeof(Models.TranslationModelTargetTextScore), columns);

            return command;
        }

        private async Task InsertTranslationModelTargetTextScoreAsync(Models.TranslationModelTargetTextScore translationModelTargetTextScore, Guid translationModelEntryId, DbCommand translationModelTargetTextScoreCommand, CancellationToken cancellationToken)
        {
            var translationTargetTextScoreId = (Guid.Empty != translationModelTargetTextScore.Id) ? translationModelTargetTextScore.Id : Guid.NewGuid();

            translationModelTargetTextScoreCommand.Parameters["@Id"].Value = translationTargetTextScoreId;
            translationModelTargetTextScoreCommand.Parameters["@TranslationModelEntryId"].Value = translationModelEntryId;
            translationModelTargetTextScoreCommand.Parameters["@Text"].Value = translationModelTargetTextScore.Text;
            translationModelTargetTextScoreCommand.Parameters["@Score"].Value = translationModelTargetTextScore.Score;
            _ = await translationModelTargetTextScoreCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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