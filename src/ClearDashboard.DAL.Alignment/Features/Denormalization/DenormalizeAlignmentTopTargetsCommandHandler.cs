using ClearDashboard.DAL.Alignment.Features.Events;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Resources;
using System.Text.Json;
using System.Threading;
using ClearDashboard.DataAccessLayer.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClearDashboard.DAL.Alignment.Features.Denormalization
{
    public class DenormalizeAlignmentTopTargetsCommandHandler : ProjectDbContextCommandHandler<DenormalizeAlignmentTopTargetsCommand,
        RequestResult<int>, int>
    {
        private readonly IMediator _mediator;

        private class AlignmentTopTargetTrainingTextEqualityComparer : IEqualityComparer<AlignmentTopTargetTrainingText>
        {
            public bool Equals(AlignmentTopTargetTrainingText? x, AlignmentTopTargetTrainingText? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x is null || y is null) return false;
                return
                    x.AlignmentSetId == y.AlignmentSetId &&
                    x.SourceTokenComponentId == y.SourceTokenComponentId &&
                    x.SourceTrainingText == y.SourceTrainingText &&
                    x.TopTargetTrainingText == y.TopTargetTrainingText;
            }
            public int GetHashCode(AlignmentTopTargetTrainingText e) => HashCode.Combine(
                e.AlignmentSetId,
                e.SourceTokenComponentId,
                e.SourceTrainingText,
                e.TopTargetTrainingText);
        }

        public DenormalizeAlignmentTopTargetsCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory,
            IProjectProvider projectProvider,
            ILogger<DenormalizeAlignmentTopTargetsCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<int>> SaveDataAsync(DenormalizeAlignmentTopTargetsCommand request,
            CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            IQueryable<AlignmentSetDenormalizationTask> tasks = ProjectDbContext.AlignmentSetDenormalizationTasks;

            if (request.AlignmentSetId != Guid.Empty)
            {
                tasks = tasks.Where(t => t.AlignmentSetId == request.AlignmentSetId);
            }

            if (!tasks.Any())
            {
                return new RequestResult<int>(0);
            }

#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            try
            {
                var reporter = new PhasedProgressReporter(request.Progress,
                    new Phase(LocalizationStrings.Get("Denormalization_ProcessingTasks", Logger)));

                using (PhaseProgress phaseProgress = reporter.StartNextPhase())
                    //PublishWorking("Denormalization_ProcessingTasks", Array.Empty<object>(), cancellationToken);
                    foreach (var g in tasks
                        .ToList()
                        .GroupBy(t => t.AlignmentSetId))
                    {
                        await ProcessAlignmentSet(
                            g.Key, 
                            request.AlignmentTypesToInclude, 
                            g.Select(g => g), 
                            phaseProgress, 
                            cancellationToken);
                    }

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif

                request.Progress.ReportCompleted();
                //PublishCompleted(cancellationToken);
                return new RequestResult<int>(tasks.Count());
            }
            catch (OperationCanceledException)
            {
                request.Progress.ReportCancelRequestReceived(LocalizationStrings.Get("Denormalization_AlignmentTopTargets_RunCancelled", Logger));
                return new RequestResult<int>
                (
                    success: false,
                    message: $"AlignmentSet data denormalization cancelled"
                );
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Alignment top targets denormalization exception");
                request.Progress.ReportException(ex);
                //PublishException(ex, cancellationToken);
                return new RequestResult<int>
                (
                    success: false,
                    message: $"Error denormalizing AlignmentSet data: {ex.Message}"
                );
            }
        }

        private async Task ProcessAlignmentSet(
            Guid alignmentSetId,
            AlignmentTypes alignmentTypesToInclude,
            IEnumerable<AlignmentSetDenormalizationTask> tasks,
            IProgress<ProgressStatus> progressStatus,
            CancellationToken cancellationToken)
        {
#if DEBUG
            Stopwatch sw = new();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Process alignment set '{alignmentSetId}' (start)");
#endif

            var reporter = new PhasedProgressReporter(progressStatus,
                    new Phase(LocalizationStrings.Get("Denormalization_QueryingData", Logger)),
                    new Phase(LocalizationStrings.Get("Denormalization_CreatingData", Logger)));

            IEnumerable<string>? sourceTrainingTextsFromTasks = null;
            IEnumerable<AlignmentTopTargetTrainingText> alignmentTopTargetTrainingTextsToCreate = Enumerable.Empty<AlignmentTopTargetTrainingText>();
            IEnumerable<Guid>? alignmentTopTargetTrainingTextGuidsToDelete = null;

            using (PhaseProgress phaseProgress = reporter.StartNextPhase())
                (sourceTrainingTextsFromTasks, alignmentTopTargetTrainingTextsToCreate, alignmentTopTargetTrainingTextGuidsToDelete) =
                    await QueryDataToDenormalize(alignmentSetId, alignmentTypesToInclude, tasks, phaseProgress, cancellationToken);

#if DEBUG
            sw.Stop();
            if (!alignmentTopTargetTrainingTextsToCreate.Any() && 
                 alignmentTopTargetTrainingTextGuidsToDelete is not null && !alignmentTopTargetTrainingTextGuidsToDelete.Any())
            {
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Process alignment set '{alignmentSetId}' (end) - no denormalization records to insert or delete");
            }
            else
            {
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Querying alignment tokens complete.  Starting deletes/inserts into denormalization table");
                sw.Restart();
            }
#endif

            if (alignmentTopTargetTrainingTextsToCreate.Any() || 
                alignmentTopTargetTrainingTextGuidsToDelete is null || 
                alignmentTopTargetTrainingTextGuidsToDelete.Any())
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (PhaseProgress phaseProgress = reporter.StartNextPhase())
                    await CreateDenormalizedData(
                        alignmentSetId,
                        tasks,
                        sourceTrainingTextsFromTasks,
                        alignmentTopTargetTrainingTextsToCreate,
                        alignmentTopTargetTrainingTextGuidsToDelete,
                        phaseProgress,
                        cancellationToken);
            }
            else
            {
                ProjectDbContext.RemoveRange(tasks);
                await ProjectDbContext.SaveChangesAsync(cancellationToken);
            }

#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Process alignment set (end)");
#endif
        }

        private async Task<(IEnumerable<string>?, IEnumerable<AlignmentTopTargetTrainingText>, IEnumerable<Guid>?)> QueryDataToDenormalize(
            Guid alignmentSetId,
            AlignmentTypes alignmentTypesToInclude,
            IEnumerable<AlignmentSetDenormalizationTask> tasks,
            IProgress<ProgressStatus> progress,
            CancellationToken cancellationToken)
        {
            IEnumerable<string>? sourceTrainingTextsFromTasks = null;
            IEnumerable<AlignmentTopTargetTrainingText> alignmentTopTargetTrainingTextsToCreate = Enumerable.Empty<AlignmentTopTargetTrainingText>();
            IEnumerable<Guid>? alignmentTopTargetTrainingTextGuidsToDelete = null;

            var alignmentSet = ProjectDbContext!.AlignmentSets
                .Include(ast => ast.ParallelCorpus)
                .FirstOrDefault(ast => ast.Id == alignmentSetId);

            if (alignmentSet is null)
            {
                Logger.LogError($"AlignmentSet not found for id '{alignmentSetId}'");
                return (sourceTrainingTextsFromTasks, alignmentTopTargetTrainingTextsToCreate, alignmentTopTargetTrainingTextGuidsToDelete);
            }

            progress.Report(new ProgressStatus(0, string.Format(
                LocalizationStrings.Get("Denormalization_AlignmentTopTargets_FindingAlignments", Logger),
                new Object[] { alignmentSet!.DisplayName ?? string.Empty }
            )));

            var alignments = ProjectDbContext!.Alignments
                .Include(a => a.SourceTokenComponent)
                .Include(a => a.TargetTokenComponent)
                .Where(a => a.Deleted == null)
                .Where(a => a.AlignmentSetId == alignmentSetId);

            List<Models.Alignment>? alignmentResults = null;
            if (tasks.All(t => t.SourceText != null))
            {
                // If all of the denormalization tasks are for specific SourceText values:
                sourceTrainingTextsFromTasks = tasks.Select(t => t.SourceText!).Distinct();
                alignmentResults = await alignments
                    .Where(a => sourceTrainingTextsFromTasks.Contains(a.SourceTokenComponent!.TrainingText))
                    .ToListAsync(cancellationToken);
            }
            else
            {
                // Or, if any are for NULL SourceText, this means for the entire alignment set:
                alignmentResults = await alignments
                    .Where(a => a.SourceTokenComponent!.TrainingText != null)
                    .ToListAsync(cancellationToken);
            }

            if (!alignmentResults.Any())
            {
                return (sourceTrainingTextsFromTasks, alignmentTopTargetTrainingTextsToCreate, alignmentTopTargetTrainingTextGuidsToDelete);
            }

            alignmentResults = alignmentResults.WhereAlignmentTypesFilter(alignmentTypesToInclude).ToList();

            progress.Report(new ProgressStatus(0, string.Format(
                LocalizationStrings.Get("Denormalization_AlignmentTopTargets_FindingTopTargetTextPerSourceText", Logger),
                new Object[] { alignmentResults.Count(), alignments.First()!.AlignmentSet!.DisplayName ?? string.Empty }
            )));

            // AlignmentResults are used to calculate "top" target text for each TrainingText:
            var sourceTrainingTextToTopTarget = alignmentResults
                .GroupBy(a => a.SourceTokenComponent!.TrainingText!)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => a)
                        .Where(a => a.TargetTokenComponent!.TrainingText != null)
                        .GroupBy(a => a.TargetTokenComponent!.TrainingText!)
                        .OrderByDescending(g => g.Count())
                        .First().Key
                );

            // Query tokenComponents from TokenizedCorpus on 'source' side of the AlignmentSet's ParallelCorpus:
            var tokenComponents = ProjectDbContext.TokenComponents
                .Where(tc => tc.TokenizedCorpusId == alignmentSet.ParallelCorpus!.SourceTokenizedCorpusId)
                .Where(tc => tc.TrainingText != null);

            if (sourceTrainingTextsFromTasks is not null)
            {
                // Filter tokenComponents by SourceTrainingText values from relevant denormalization task(s):
                tokenComponents = tokenComponents.Where(tc => sourceTrainingTextsFromTasks.Contains(tc.TrainingText));
            }

            var sourceTrainingTextKeys = sourceTrainingTextToTopTarget.Keys.ToList();

            var alignmentTopTargetTrainingTextsForTokens = tokenComponents
                .Select(tc => new { tc.Id, tc.TrainingText })
                .Where(i => sourceTrainingTextKeys.Contains(i.TrainingText!))
                .Select(i =>
                    new AlignmentTopTargetTrainingText
                    {
                        AlignmentSetId = alignmentSetId,
                        SourceTokenComponentId = i.Id,
                        SourceTrainingText = i.TrainingText!,
                        TopTargetTrainingText = sourceTrainingTextToTopTarget[i.TrainingText!]
                    }
                )
                .ToList();

            var existingTopTargetTrainingTexts = ProjectDbContext.AlignmentTopTargetTrainingTexts
                .Where(e => e.AlignmentSetId == alignmentSetId)
                .Where(e => sourceTrainingTextKeys.Contains(e.SourceTrainingText))
                .ToList();

            alignmentTopTargetTrainingTextGuidsToDelete = existingTopTargetTrainingTexts
                .Except(alignmentTopTargetTrainingTextsForTokens, new AlignmentTopTargetTrainingTextEqualityComparer())
                .Select(e => e.Id)
                .ToList();

            if (alignmentTopTargetTrainingTextGuidsToDelete.Count() <= 20)
            {
                // If the number to delete is relatively small, delete by specific id(s)
                // and create only the ones that aren't already there.  This optimization
                // most commonly helps with recalculation of a SourceTrainingText value
                // for which the TopTargetTrainingText comes out the same as before
                alignmentTopTargetTrainingTextsToCreate = alignmentTopTargetTrainingTextsForTokens
                    .Except(existingTopTargetTrainingTexts, new AlignmentTopTargetTrainingTextEqualityComparer())
                    .ToList();
            }
            else
            {
                // If the number to delete is larger, delete by source training text values (or by
                // AlignmentSetId only, if SourceTrainingTextsFromTasks == null) and create all of them:
                alignmentTopTargetTrainingTextsToCreate = alignmentTopTargetTrainingTextsForTokens;
                alignmentTopTargetTrainingTextGuidsToDelete = null;  // Delete per sourceTrainingTextsFromTasks
            }

            return (sourceTrainingTextsFromTasks, alignmentTopTargetTrainingTextsToCreate, alignmentTopTargetTrainingTextGuidsToDelete);
        }

        private async Task CreateDenormalizedData(
            Guid alignmentSetId,
            IEnumerable<AlignmentSetDenormalizationTask> tasks,
            IEnumerable<string>? sourceTrainingTextsFromTasks,
            IEnumerable<AlignmentTopTargetTrainingText> sourceTokenIdToTopTargetTrainingTexts,
            IEnumerable<Guid>? alignmentTopTargetTrainingTextGuidsToDelete,
            IProgress<ProgressStatus> progress,
            CancellationToken cancellationToken)
        {
            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                progress.Report(new ProgressStatus(
                    0,
                    LocalizationStrings.Get("Denormalization_AlignmentTopTargets_DeletingPreviousRecords", Logger)));
                //PublishWorking("Denormalization_AlignmentTopTargets_DeletingPreviousRecords", Array.Empty<object>(), cancellationToken);

                if (alignmentTopTargetTrainingTextGuidsToDelete is null)
                {
                    await ExecuteAlignmentTopTargetTrainingTextDeleteCommand(
                        alignmentSetId,
                        sourceTrainingTextsFromTasks,
                        connection,
                        cancellationToken);
                }
                else if (alignmentTopTargetTrainingTextGuidsToDelete.Any())
                {
                    await ExecuteAlignmentTopTargetTrainingTextByGuidDeleteCommand(
                        alignmentSetId,
                        alignmentTopTargetTrainingTextGuidsToDelete,
                        connection,
                        cancellationToken);
                }

                using var insertCommand = CreateAlignmentTopTargetTrainingTextInsertCommand(connection, ProjectDbContext.Model);

                var completed = 0;
                foreach (var a in sourceTokenIdToTopTargetTrainingTexts)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (completed % 10000 == 0)
                    {
                        progress.Report(new ProgressStatus(
                            completed, sourceTokenIdToTopTargetTrainingTexts.Count(),
                            string.Format(
                                LocalizationStrings.Get("Denormalization_AlignmentTopTargets_InsertingRecords", Logger),
                                new Object[] { sourceTokenIdToTopTargetTrainingTexts.Count() })
                            ));
                        //var percentComplete = Convert.ToInt32(completed * 100 / sourceTokenIdToTopTargetTrainingTexts.Count());
                        //PublishWorking("Denormalization_AlignmentTopTargets_InsertingRecords", new object[] { percentComplete }, cancellationToken);
                    }

                    await InsertAlignmentTopTargetTrainingTextAsync(
                            a.AlignmentSetId,
                            a.SourceTokenComponentId,
                            a.SourceTrainingText,
                            a.TopTargetTrainingText,
                            insertCommand,
                            cancellationToken);

                    completed++;
                }

                progress.Report(new ProgressStatus(0,
                    LocalizationStrings.Get("Denormalization_AlignmentTopTargets_SavingChanges", Logger)));
                //PublishWorking("Denormalization_AlignmentTopTargets_SavingChanges", Array.Empty<object>(), cancellationToken);

                ProjectDbContext.RemoveRange(tasks);

                ProjectDbContext.Database.UseTransaction(transaction);
                await ProjectDbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                ProjectDbContext.Database.UseTransaction(null);
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }

        private static DbCommand CreateAlignmentTopTargetTrainingTextInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "AlignmentSetId", "SourceTokenComponentId", "SourceTrainingText", "TopTargetTrainingText" };

            ApplyColumnsToCommand(command, metadataModel.ToEntityType(typeof(Models.AlignmentTopTargetTrainingText)), columns);

            return command;
        }

        private static async Task<Guid> InsertAlignmentTopTargetTrainingTextAsync(Guid alignmentSetId, Guid sourceTokenComponentId, string SourceTrainingText, string topTargetTrainingText, DbCommand insertCommand, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();

            insertCommand.Parameters["@Id"].Value = id;
            insertCommand.Parameters["@AlignmentSetId"].Value = alignmentSetId;
            insertCommand.Parameters["@SourceTokenComponentId"].Value = sourceTokenComponentId;
            insertCommand.Parameters["@SourceTrainingText"].Value = SourceTrainingText;
            insertCommand.Parameters["@TopTargetTrainingText"].Value = topTargetTrainingText;

            _ = await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return id;
        }

        private static void ApplyColumnsToCommand(DbCommand command, IEntityType entityType, string[] columns)
        {
            var tableName = entityType.ToTableName();

            command.CommandText =
            $@"
                INSERT INTO {tableName} ({string.Join(", ", columns)})
                VALUES ({string.Join(", ", columns.Select(c => "@" + c))})
            ";

            foreach (var column in columns)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{column}";
                command.Parameters.Add(parameter);
            }
        }

        private static async Task ExecuteAlignmentTopTargetTrainingTextDeleteCommand(Guid alignmentSetId, IEnumerable<string>? sourceTrainingTexts, DbConnection connection, CancellationToken cancellationToken)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                DELETE FROM {typeof(Models.AlignmentTopTargetTrainingText).Name}
                WHERE AlignmentSetId = @AlignmentSetId
            ";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@AlignmentSetId";
            command.Parameters.Add(parameter);
            command.Parameters["@AlignmentSetId"].Value = alignmentSetId;

            if (sourceTrainingTexts is not null && sourceTrainingTexts.Any())
            {
                command.CommandText += "AND SourceTrainingText IN (";

                sourceTrainingTexts
                    .Select((t, index) => new
                    {
                        index,
                        name = "@t" + index,
                        value = t
                    })
                    .ToList()
                    .ForEach(pi =>
                    {
                        command.CommandText += (pi.index > 0) ? $", {pi.name}" : pi.name;

                        var parameter = command.CreateParameter();
                        parameter.ParameterName = pi.name;
                        command.Parameters.Add(parameter);
                        command.Parameters[pi.name].Value = pi.value;
                    });

                command.CommandText += ")";
            }

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static async Task ExecuteAlignmentTopTargetTrainingTextByGuidDeleteCommand(Guid alignmentSetId, IEnumerable<Guid> alignmentTopTargetTrainingTextGuidsToDelete, DbConnection connection, CancellationToken cancellationToken)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                DELETE FROM {typeof(Models.AlignmentTopTargetTrainingText).Name}
                WHERE AlignmentSetId = @AlignmentSetId
            ";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@AlignmentSetId";
            command.Parameters.Add(parameter);
            command.Parameters["@AlignmentSetId"].Value = alignmentSetId;

            if (alignmentTopTargetTrainingTextGuidsToDelete.Any())
            {
                command.CommandText += "AND Id IN (";

                alignmentTopTargetTrainingTextGuidsToDelete
                    .Select((t, index) => new
                    {
                        index,
                        name = "@t" + index,
                        value = t
                    })
                    .ToList()
                    .ForEach(pi =>
                    {
                        command.CommandText += (pi.index > 0) ? $", {pi.name}" : pi.name;

                        var parameter = command.CreateParameter();
                        parameter.ParameterName = pi.name;
                        command.Parameters.Add(parameter);
                        command.Parameters[pi.name].Value = pi.value;
                    });

                command.CommandText += ")";
            }

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private async void PublishWorking(string key, object[] args, CancellationToken cancellationToken)
        {
            await _mediator.Publish(
                new DenormalizationProgressEvent(
                    status: LongRunningTaskStatus.Running,
                    name: LocalizationStrings.Get("Denormalization_AlignmentTopTargets_BackgroundTaskName", Logger),
                    description: string.Format(LocalizationStrings.Get(key, Logger), args) ?? string.Empty),
                cancellationToken);
        }
        private async void PublishCompleted(CancellationToken cancellationToken)
        {
            await _mediator.Publish(
                new DenormalizationProgressEvent(
                    status: LongRunningTaskStatus.Completed,
                    name: LocalizationStrings.Get("Denormalization_AlignmentTopTargets_BackgroundTaskName", Logger)),
                cancellationToken);
        }
        private async void PublishException(Exception ex, CancellationToken cancellationToken)
        {
            await _mediator.Publish(
                new DenormalizationProgressEvent(
                    status: LongRunningTaskStatus.Completed,
                    name: LocalizationStrings.Get("Denormalization_AlignmentTopTargets_BackgroundTaskName", Logger),
                    exception: ex),
                cancellationToken);
        }
        private async void PublishCancelRequested(CancellationToken cancellationToken)
        {
            await _mediator.Publish(
                new DenormalizationProgressEvent(
                    status: LongRunningTaskStatus.CancellationRequested,
                    name: LocalizationStrings.Get("Denormalization_AlignmentTopTargets_BackgroundTaskName", Logger),
                    description: LocalizationStrings.Get("Denormalization_AlignmentTopTargets_RunCancelled", Logger)),
                cancellationToken);
        }
    }

    // Very temporary - until the real LocalizedStrings implementation is put into 
    // DI or some common project Alignment has access to:
    public static class LocalizationStrings
    {
        static readonly Dictionary<string, string> _mappings;
        static LocalizationStrings()
        {
            _mappings = new Dictionary<string, string>()
            {
                { "Denormalization_AlignmentTopTargets_BackgroundTaskName", "Alignment Denormalization" },
                { "Denormalization_ProcessingTasks", "Processing tasks" },
                { "Denormalization_QueryingData", "Querying data" },
                { "Denormalization_CreatingData", "Creating data" },
                { "Denormalization_AlignmentTopTargets_FindingAlignments", "Finding targeted alignments in set '{0}'" },
                { "Denormalization_AlignmentTopTargets_FindingTopTargetTextPerSourceText", "Finding top target training text per source training text for {0} alignments in set '{1}'" },
                { "Denormalization_AlignmentTopTargets_DeletingPreviousRecords", "Deleting previous records" },
                { "Denormalization_AlignmentTopTargets_InsertingRecords", "Inserting {0} records - percent completed {{PercentCompleted:P0}}" },
                { "Denormalization_AlignmentTopTargets_SavingChanges", "Saving changes" },
                { "Denormalization_AlignmentTopTargets_RunCancelled", "Run cancelled" },
            };
        }
        public static string Get(string key, ILogger logger)
        {
            string localizedString;
            try
            {
                localizedString = _mappings[key];
            }
            catch (Exception e)
            {
                logger.LogCritical($"Localization string missing for key '{key}' {e.Message} {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }

            if (localizedString == null)
            {
                logger.LogCritical($"Localization string missing for key '{key}' {Thread.CurrentThread.CurrentUICulture.Name}");
                localizedString = key;
            }
            return localizedString;
        }
    }
}