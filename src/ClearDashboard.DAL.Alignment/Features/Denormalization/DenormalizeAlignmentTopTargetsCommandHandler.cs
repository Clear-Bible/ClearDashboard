using Autofac.Core;
using ClearDashboard.DAL.Alignment.Features.Translation;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DAL.CQRS.Features;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using SIL.Linq;
using System.Data.Common;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Denormalization
{
    public class DenormalizeAlignmentTopTargetsCommandHandler : ProjectDbContextCommandHandler<DenormalizeAlignmentTopTargetsCommand,
        RequestResult<object>, object>
    {
        private readonly IMediator _mediator;

        public DenormalizeAlignmentTopTargetsCommandHandler(IMediator mediator,
            ProjectDbContextFactory? projectNameDbContextFactory, IProjectProvider projectProvider,
            ILogger<DenormalizeAlignmentTopTargetsCommandHandler> logger) : base(projectNameDbContextFactory, projectProvider,
            logger)
        {
            _mediator = mediator;
        }

        protected override async Task<RequestResult<object>> SaveDataAsync(DenormalizeAlignmentTopTargetsCommand request,
            CancellationToken cancellationToken)
        {
            IQueryable<AlignmentSetDenormalizationTask> tasks = ProjectDbContext.AlignmentSetDenormalizationTasks;

            if (request.AlignmentSetId != Guid.Empty)
            {
                tasks = tasks.Where(t => t.AlignmentSetId == request.AlignmentSetId);
            }

            if (!tasks.Any())
            {
                return new RequestResult<object>(new());
            }

#if DEBUG
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (start)");
#endif

            var tasksByAlignmentSetId = tasks
                .GroupBy(t => t.AlignmentSetId);

            // TODO:  Loop through each alignment set and do all the following
            // logic for each one:

            var alignments = ProjectDbContext!.Alignments
                .Include(a => a.SourceTokenComponent)
                        .Include(a => a.TargetTokenComponent)
                .Where(a => a.AlignmentSetId == request.AlignmentSetId);

            IEnumerable<string>? sourceTrainingTexts = null;
            if (tasks.All(t => t.SourceText != null))
            {
                sourceTrainingTexts = tasks.Select(t => t.SourceText!).Distinct();
                alignments = alignments
                    .Where(a => sourceTrainingTexts.Contains(a.SourceTokenComponent!.TrainingText));
            }
            else
            {
                alignments = alignments.Where(a => a.SourceTokenComponent!.TrainingText != null);
            }

            var sourceTokenIdToTopTargetTrainingText = alignments
                .ToList()
                .GroupBy(a => a.SourceTokenComponent!.TrainingText!)
                .SelectMany(g => g
                    .Select(a => new
                    {
                        alignment = a,
                        SourceTrainingText = g.Key,
                        TopTargetTrainingText = g
                            .Where(a => a.TargetTokenComponent!.TrainingText != null)
                            .GroupBy(a => a.TargetTokenComponent!.TrainingText!)
                            .OrderByDescending(g => g.Count())
                            .First().Key
                    })).ToList();
#if DEBUG
            sw.Stop();
            Logger.LogInformation($"Elapsed={sw.Elapsed} - Querying alignment tokens complete.  Starting insert into denormalization table");
            sw.Restart();
#endif
            cancellationToken.ThrowIfCancellationRequested();

            await ProjectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = ProjectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                await ExecuteAlignmentTopTargetTrainingTextDeleteCommand(
                    request.AlignmentSetId, 
                    sourceTrainingTexts, 
                    connection, 
                    cancellationToken);

                using var insertCommand = CreateAlignmentTopTargetTrainingTextInsertCommand(connection);

                foreach (var a in sourceTokenIdToTopTargetTrainingText)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await InsertAlignmentTopTargetTrainingTextAsync(
                            a.alignment,
                            a.SourceTrainingText,
                            a.TopTargetTrainingText,
                            insertCommand,
                            cancellationToken);
                }

                ProjectDbContext.Remove(tasks);
                await transaction.CommitAsync(cancellationToken);

#if DEBUG
                sw.Stop();
                Logger.LogInformation($"Elapsed={sw.Elapsed} - Handler (end)");
#endif
                return new RequestResult<object>(new object());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error denormalizing AlignmentSet data: {ex.Message}");
                throw;
            }
            finally
            {
                await ProjectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
            }
        }

        private static DbCommand CreateAlignmentTopTargetTrainingTextInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "AlignmentSetId", "AlignmentId", "SourceTokenComponentId", "SourceTrainingText", "TopTargetTrainingText" };

            ApplyColumnsToCommand(command, typeof(Models.AlignmentTopTargetTrainingText), columns);

            return command;
        }

        private async Task<Guid> InsertAlignmentTopTargetTrainingTextAsync(Models.Alignment alignment, string SourceTrainingText, string topTargetTrainingText, DbCommand insertCommand, CancellationToken cancellationToken)
        {
            var id = Guid.NewGuid();

            insertCommand.Parameters["@Id"].Value = id;
            insertCommand.Parameters["@AlignmentSetId"].Value = alignment.AlignmentSetId;
            insertCommand.Parameters["@AlignmentId"].Value = alignment.Id;
            insertCommand.Parameters["@SourceTokenComponentId"].Value = alignment.SourceTokenComponentId;
            insertCommand.Parameters["@SourceTrainingText"].Value = SourceTrainingText;
            insertCommand.Parameters["@TopTargetTrainingText"].Value = topTargetTrainingText;

            _ = await insertCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return id;
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
                    }).ForEach(pi =>
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

    }
}