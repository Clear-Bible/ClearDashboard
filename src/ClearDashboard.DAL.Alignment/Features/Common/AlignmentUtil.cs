using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using System.Data.Common;
using System.Text.Json;
using ClearDashboard.DAL.Alignment.Features.Translation;
using static ClearDashboard.DAL.Alignment.Features.Corpora.UpdateOrAddVersesInTokenizedCorpusCommandHandler;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models;
using SIL.Machine.SequenceAlignment;
using static ClearDashboard.DAL.Alignment.Features.Common.DataUtil;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class AlignmentUtil
    {
        static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public static DbCommand CreateAlignmentSetInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "ParallelCorpusId", "DisplayName", "SmtModel", "IsSyntaxTreeAlignerRefined", "IsSymmetrized", "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.AlignmentSet), columns);

            command.Prepare();

            return command;
        }

        public static async Task<Guid> PrepareInsertAlignmentSetAsync(Models.AlignmentSet alignmentSet, DbCommand alignmentSetCommand, Guid currentUserId, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            var alignmentSetId = (Guid.Empty != alignmentSet.Id) ? alignmentSet.Id : Guid.NewGuid();

            alignmentSetCommand.Parameters["@Id"].Value = alignmentSetId;
            alignmentSetCommand.Parameters["@ParallelCorpusId"].Value = alignmentSet.ParallelCorpusId;
            alignmentSetCommand.Parameters["@DisplayName"].Value = alignmentSet.DisplayName;
            alignmentSetCommand.Parameters["@SmtModel"].Value = alignmentSet.SmtModel;
            alignmentSetCommand.Parameters["@IsSyntaxTreeAlignerRefined"].Value = alignmentSet.IsSyntaxTreeAlignerRefined;
            alignmentSetCommand.Parameters["@IsSymmetrized"].Value = alignmentSet.IsSymmetrized;
            alignmentSetCommand.Parameters["@Metadata"].Value = JsonSerializer.Serialize(alignmentSet.Metadata);
            alignmentSetCommand.Parameters["@UserId"].Value = Guid.Empty != alignmentSet.UserId ? alignmentSet.UserId : currentUserId;
            alignmentSetCommand.Parameters["@Created"].Value = converter.ConvertToProvider(alignmentSet.Created);

            _ = await alignmentSetCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

            return alignmentSetId;
        }

        public static async Task InsertAlignmentsAsync(IEnumerable<Models.Alignment> alignments, Guid alignmentSetId, DbCommand alignmentCommand, Guid currentUserId, CancellationToken cancellationToken)
        {
            //disconnect since I am not looking to insert a new alignment set
            foreach (var alignment in alignments)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await InsertAlignmentAsync(alignment, alignmentSetId, alignmentCommand, currentUserId, cancellationToken);
            }
        }

        public static DbCommand CreateAlignmentInsertCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "SourceTokenComponentId", "TargetTokenComponentId", "AlignmentVerification", "AlignmentOriginatedFrom", "Score", "AlignmentSetId", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.Alignment), columns);

            command.Prepare();

            return command;
        }

        public static async Task InsertAlignmentAsync(Models.Alignment alignment, Guid alignmentSetId, DbCommand alignmentCommand, Guid currentUserId, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            alignmentCommand.Parameters["@Id"].Value = (Guid.Empty != alignment.Id) ? alignment.Id : Guid.NewGuid();
            alignmentCommand.Parameters["@SourceTokenComponentId"].Value = alignment.SourceTokenComponentId;
            alignmentCommand.Parameters["@TargetTokenComponentId"].Value = alignment.TargetTokenComponentId;
            alignmentCommand.Parameters["@AlignmentVerification"].Value = alignment.AlignmentVerification;
            alignmentCommand.Parameters["@AlignmentOriginatedFrom"].Value = alignment.AlignmentOriginatedFrom;
            alignmentCommand.Parameters["@Score"].Value = alignment.Score;
            alignmentCommand.Parameters["@AlignmentSetId"].Value = alignmentSetId;
            alignmentCommand.Parameters["@UserId"].Value = Guid.Empty != alignment.UserId ? alignment.UserId : currentUserId;
            alignmentCommand.Parameters["@Created"].Value = converter.ConvertToProvider(alignment.Created);
            _ = await alignmentCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

		public static DbCommand CreateTranslationInsertCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { "Id", "SourceTokenComponentId", "TargetText", "TranslationState", "TranslationSetId", "LexiconTranslationId", "UserId", "Created" };

			ApplyColumnsToCommand(command, typeof(Models.Translation), columns);

			command.Prepare();

			return command;
		}

		public static async Task InsertTranslationAsync(Models.Translation translation, Guid translationSetId, DbCommand translationCommand, Guid currentUserId, CancellationToken cancellationToken)
		{
			var converter = new DateTimeOffsetToBinaryConverter();

			translationCommand.Parameters["@Id"].Value = (Guid.Empty != translation.Id) ? translation.Id : Guid.NewGuid();
			translationCommand.Parameters["@SourceTokenComponentId"].Value = translation.SourceTokenComponentId;
			translationCommand.Parameters["@TargetText"].Value = translation.TargetText;
			translationCommand.Parameters["@TranslationState"].Value = translation.TranslationState;
			translationCommand.Parameters["@TranslationSetId"].Value = translationSetId;
			translationCommand.Parameters["@LexiconTranslationId"].Value = (null != translation.LexiconTranslationId) ? translation.LexiconTranslationId : DBNull.Value;
			translationCommand.Parameters["@UserId"].Value = Guid.Empty != translation.UserId ? translation.UserId : currentUserId;
			translationCommand.Parameters["@Created"].Value = converter.ConvertToProvider(translation.Created);
			_ = await translationCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTokenVerseAssociationInsertCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { "Id", "TokenComponentId", "VerseId", "Position", "UserId", "Created" };

			ApplyColumnsToCommand(command, typeof(Models.TokenVerseAssociation), columns);

			command.Prepare();

			return command;
		}

		public static async Task InsertTokenVerseAssociationAsync(Models.TokenVerseAssociation tokenVerseAssociation, DbCommand tvaCommand, Guid currentUserId, CancellationToken cancellationToken)
		{
			var converter = new DateTimeOffsetToBinaryConverter();

			tvaCommand.Parameters["@Id"].Value = (Guid.Empty != tokenVerseAssociation.Id) ? tokenVerseAssociation.Id : Guid.NewGuid();
			tvaCommand.Parameters["@TokenComponentId"].Value = tokenVerseAssociation.TokenComponentId;
			tvaCommand.Parameters["@VerseId"].Value = tokenVerseAssociation.VerseId;
			tvaCommand.Parameters["@Position"].Value = tokenVerseAssociation.Position;
			tvaCommand.Parameters["@UserId"].Value = Guid.Empty != tokenVerseAssociation.UserId ? tokenVerseAssociation.UserId : currentUserId;
			tvaCommand.Parameters["@Created"].Value = converter.ConvertToProvider(tokenVerseAssociation.Created);
			_ = await tvaCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
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

        public static DbCommand CreateDeleteMachineAlignmentsByAlignmentSetIdCommand(DbConnection connection)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Id", "ParallelCorpusId", "DisplayName", "SmtModel", "IsSyntaxTreeAlignerRefined", "IsSymmetrized", "Metadata", "UserId", "Created" };

            ApplyColumnsToCommand(command, typeof(Models.AlignmentSet), columns);

            command.Prepare();

            return command;
        }

        public static async Task DeleteMachineAlignmentsByAlignmentSetIdAsync(Guid alignmentSetId, DbConnection connection, CancellationToken cancellationToken)//IEnumerable<Guid> alignmentTopTargetTrainingTextGuidsToDelete,
        {   
            await using var command = connection.CreateCommand();
            command.CommandText = $@"
                DELETE FROM Alignment
                WHERE AlignmentOriginatedFrom = '0' AND AlignmentSetId = '{alignmentSetId.ToString().ToUpper()}'    
            ";

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

		public static DbCommand CreateAlignmentSourceOrTargetIdSetCommand(DbConnection connection, bool isSource)
		{
			var columnName = isSource
				? nameof(DataAccessLayer.Models.Alignment.SourceTokenComponentId)
				: nameof(DataAccessLayer.Models.Alignment.TargetTokenComponentId);

			var command = connection.CreateCommand();
			var columns = new string[] { columnName };
			var whereColumns = new (string, WhereEquality)[] { (nameof(IdentifiableEntity.Id), WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				typeof(DataAccessLayer.Models.Alignment),
				columns,
				whereColumns,
				Array.Empty<(string, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SetAlignmentSourceIdAsync(Guid alignmentId, Guid sourceTokenComponentId, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(DataAccessLayer.Models.Alignment.SourceTokenComponentId)}"].Value = sourceTokenComponentId;
			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = alignmentId;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static async Task SetAlignmentTargetIdAsync(Guid alignmentId, Guid targetTokenComponentId, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(DataAccessLayer.Models.Alignment.TargetTokenComponentId)}"].Value = targetTokenComponentId;
			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = alignmentId;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTranslationSourceIdSetCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { nameof(DataAccessLayer.Models.Translation.SourceTokenComponentId) };
			var whereColumns = new (string, WhereEquality)[] { (nameof(IdentifiableEntity.Id), WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				typeof(DataAccessLayer.Models.Translation),
				columns,
				whereColumns,
				Array.Empty<(string, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SetTranslationSourceIdAsync(Guid translationId, Guid sourceTokenComponentId, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(DataAccessLayer.Models.Translation.SourceTokenComponentId)}"].Value = sourceTokenComponentId;
			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = translationId;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTVATokenComponentIdSetCommand(DbConnection connection)
		{
			var command = connection.CreateCommand();
			var columns = new string[] { nameof(DataAccessLayer.Models.TokenVerseAssociation.TokenComponentId) };
			var whereColumns = new (string, WhereEquality)[] { (nameof(IdentifiableEntity.Id), WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				typeof(DataAccessLayer.Models.TokenVerseAssociation),
				columns,
				whereColumns,
				Array.Empty<(string, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SetTVATokenComponentIdAsync(Guid tvaId, Guid tokenComponentId, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(DataAccessLayer.Models.TokenVerseAssociation.TokenComponentId)}"].Value = tokenComponentId;
			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = tvaId;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		//Same as ProjectTemplateProcessRunner, how do we do background tasks?
		public static async Task<TrainSmtModelSet> TrainSmtModelAsync(string taskName, bool isTrainedSymmetrizedModel, SmtModelType smtModelType, bool generateAlignedTokenPairs, EngineParallelTextCorpus parallelCorpus, ILogger logger, TranslationCommands translationCommands, CancellationToken cancellationToken)
        {
            var symmetrizationHeuristic = isTrainedSymmetrizedModel
                ? SymmetrizationHeuristic.GrowDiagFinalAnd
                : SymmetrizationHeuristic.None;

            TrainSmtModelSet? trainSmtModelSet = null;

            await semaphoreSlim.WaitAsync(cancellationToken);
            try
            {
                var wordAlignmentModel = await translationCommands.TrainSmtModel(
                    smtModelType,
                    parallelCorpus,
                    new ClearBible.Engine.Utils.DelegateProgress(async status =>
                    {
                        var message =
                            $"Training symmetrized {smtModelType} model: {status.PercentCompleted:P}";
                        //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running, cancellationToken,
                        //    message);
                        logger!.LogInformation(message);

                    }), symmetrizationHeuristic);

                //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                //    cancellationToken, $"Completed SMT Model '{smtModelType}'.");

                IEnumerable<AlignedTokenPairs>? alignedTokenPairs = null;
                if (generateAlignedTokenPairs)
                {
                    alignedTokenPairs =
                        translationCommands.PredictAllAlignedTokenIdPairs(wordAlignmentModel, parallelCorpus)
                            .ToList();

                    //await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                    //    cancellationToken, $"Generated AlignedTokenPairs '{smtModelType}'.");
                }

                trainSmtModelSet = new TrainSmtModelSet(wordAlignmentModel, smtModelType, isTrainedSymmetrizedModel, alignedTokenPairs);
            }
            finally
            {
                semaphoreSlim.Release();
            }

            return trainSmtModelSet;
        }

        public static async Task<RequestResult<IEnumerable<Alignment.Translation.Alignment>>> FillInVerificationAndOriginatedEnums(IEnumerable<Alignment.Translation.Alignment> request, Dictionary<string, AlignmentVerification> verificationTypes, Dictionary<string, AlignmentOriginatedFrom> originatedTypes)
        {
            //var verificationTypes = new Dictionary<string, AlignmentVerification>();
            //var originatedTypes = new Dictionary<string, AlignmentOriginatedFrom>();
            foreach (var al in request)
            {
                if (Enum.TryParse(al.Verification, out AlignmentVerification verificationType))
                {
                    verificationTypes[al.Verification] = verificationType;
                }
                else
                {
                    return new RequestResult<IEnumerable<Alignment.Translation.Alignment>>
                    (
                        success: false,
                        message: $"Invalid alignment verification type '{al.Verification}' found in request"
                    );
                }

                if (Enum.TryParse(al.OriginatedFrom, out AlignmentOriginatedFrom originatedType))
                {
                    originatedTypes[al.OriginatedFrom] = originatedType;
                }
                else
                {
                    return new RequestResult<IEnumerable<Alignment.Translation.Alignment>>
                    (
                        success: false,
                        message: $"Invalid alignment originated from type '{al.OriginatedFrom}' found in request"
                    );
                }
            }

            return new RequestResult<IEnumerable<Alignment.Translation.Alignment>>
            (
                result: request,
                success: true
            );
        }

    }

    public record TrainSmtModelSet
    {
        public TrainSmtModelSet(IWordAlignmentModel wordAlignmentModel, SmtModelType smtModelType, bool isTrainedSymmetrizedModel, IEnumerable<AlignedTokenPairs>? alignedTokenPairs)
        {
            WordAlignmentModel = wordAlignmentModel;
            SmtModelType = smtModelType;
            IsTrainedSymmetrizedModel = isTrainedSymmetrizedModel;
            AlignedTokenPairs = alignedTokenPairs;
        }

        public IWordAlignmentModel WordAlignmentModel { get; set; }
        public SmtModelType SmtModelType { get; set; }
        public bool IsTrainedSymmetrizedModel { get; set; }
        public IEnumerable<AlignedTokenPairs>? AlignedTokenPairs { get; set; }
    }
}