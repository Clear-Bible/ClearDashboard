using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.CQRS;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using System.Data.Common;
using System.Text.Json;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models;
using SIL.Machine.SequenceAlignment;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using static ClearDashboard.DAL.Alignment.Features.Common.DataUtil;
using ClearDashboard.DAL.Interfaces;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class AlignmentUtil
    {
        static readonly SemaphoreSlim semaphoreSlim = new(1, 1);

        public static DbCommand CreateAlignmentSetInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var entityType = metadataModel.ToEntityType(typeof(Models.AlignmentSet));
            var insertProperties = entityType.ToProperties(new List<string>
            {
                nameof(Models.AlignmentSet.Id), 
                nameof(Models.AlignmentSet.ParallelCorpusId), 
                nameof(Models.AlignmentSet.DisplayName), 
                nameof(Models.AlignmentSet.SmtModel), 
                nameof(Models.AlignmentSet.IsSyntaxTreeAlignerRefined), 
                nameof(Models.AlignmentSet.IsSymmetrized), 
                nameof(Models.AlignmentSet.Metadata), 
                nameof(Models.AlignmentSet.UserId), 
                nameof(Models.AlignmentSet.Created) 
            }).ToArray();

            ApplyColumnsToInsertCommand(command, entityType, insertProperties);

            command.Prepare();

            return command;
        }

        public static async Task<Guid> PrepareInsertAlignmentSetAsync(Models.AlignmentSet alignmentSet, DbCommand command, Guid currentUserId, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            object? dt(DateTimeOffset d) => converter.ConvertToProvider(d);
            void setCommandProperty(string propertyName, object? value) => 
                command.Parameters[command.ToParameterName(propertyName)].Value = value;

            var alignmentSetId = (Guid.Empty != alignmentSet.Id) ? alignmentSet.Id : Guid.NewGuid();

            setCommandProperty(nameof(Models.AlignmentSet.Id), alignmentSetId);
            setCommandProperty(nameof(Models.AlignmentSet.ParallelCorpusId), alignmentSet.ParallelCorpusId);
            setCommandProperty(nameof(Models.AlignmentSet.DisplayName), alignmentSet.DisplayName);
            setCommandProperty(nameof(Models.AlignmentSet.SmtModel), alignmentSet.SmtModel);
            setCommandProperty(nameof(Models.AlignmentSet.IsSyntaxTreeAlignerRefined), alignmentSet.IsSyntaxTreeAlignerRefined);
            setCommandProperty(nameof(Models.AlignmentSet.IsSymmetrized), alignmentSet.IsSymmetrized);
            setCommandProperty(nameof(Models.AlignmentSet.Metadata), JsonSerializer.Serialize(alignmentSet.Metadata));
            setCommandProperty(nameof(Models.AlignmentSet.UserId), Guid.Empty != alignmentSet.UserId ? alignmentSet.UserId : currentUserId);
            setCommandProperty(nameof(Models.AlignmentSet.Created), dt(alignmentSet.Created));

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

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

        public static DbCommand CreateAlignmentInsertCommand(DbConnection connection, IModel metadataModel)
        {
            var command = connection.CreateCommand();
            var entityType = metadataModel.ToEntityType(typeof(Models.Alignment));
            var insertProperties = entityType.ToProperties(new List<string>
            {
                nameof(Models.Alignment.Id), 
                nameof(Models.Alignment.SourceTokenComponentId), 
                nameof(Models.Alignment.TargetTokenComponentId), 
                nameof(Models.Alignment.AlignmentVerification), 
                nameof(Models.Alignment.AlignmentOriginatedFrom), 
                nameof(Models.Alignment.Score), 
                nameof(Models.Alignment.AlignmentSetId), 
                nameof(Models.Alignment.UserId), 
                nameof(Models.Alignment.Created) 
            }).ToArray();

            ApplyColumnsToInsertCommand(command, entityType, insertProperties);

            command.Prepare();

            return command;
        }

		public static async Task InsertAlignmentAsync(Models.Alignment alignment, Guid alignmentSetId, DbCommand command, Guid currentUserId, CancellationToken cancellationToken)
		{
			var converter = new DateTimeOffsetToBinaryConverter();

			object? dt(DateTimeOffset d) => converter.ConvertToProvider(d);
			void setCommandProperty(string propertyName, object? value) =>
				command.Parameters[command.ToParameterName(propertyName)].Value = value;

			setCommandProperty(nameof(Models.Alignment.Id), (Guid.Empty != alignment.Id) ? alignment.Id : Guid.NewGuid());
			setCommandProperty(nameof(Models.Alignment.SourceTokenComponentId), alignment.SourceTokenComponentId);
			setCommandProperty(nameof(Models.Alignment.TargetTokenComponentId), alignment.TargetTokenComponentId);
			setCommandProperty(nameof(Models.Alignment.AlignmentVerification), (int)alignment.AlignmentVerification);
			setCommandProperty(nameof(Models.Alignment.AlignmentOriginatedFrom), (int)alignment.AlignmentOriginatedFrom);
			setCommandProperty(nameof(Models.Alignment.Score), alignment.Score);
			setCommandProperty(nameof(Models.Alignment.AlignmentSetId), alignmentSetId);
			setCommandProperty(nameof(Models.Alignment.UserId), Guid.Empty != alignment.UserId ? alignment.UserId : currentUserId);
			setCommandProperty(nameof(Models.Alignment.Created), dt(alignment.Created));

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTranslationInsertCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.Translation));

			var insertProperties = entityType.ToProperties(new List<string>
			{
				nameof(Models.Translation.Id),
				nameof(Models.Translation.SourceTokenComponentId),
				nameof(Models.Translation.TargetText),
				nameof(Models.Translation.TranslationState),
				nameof(Models.Translation.TranslationSetId),
				nameof(Models.Translation.LexiconTranslationId),
				nameof(Models.Translation.UserId),
				nameof(Models.Translation.Created)
			}).ToArray();

			ApplyColumnsToInsertCommand(command, entityType, insertProperties);

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

		public static DbCommand CreateTokenVerseAssociationInsertCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenVerseAssociation));

			var insertProperties = entityType.ToProperties(new List<string>
			{
				nameof(Models.TokenVerseAssociation.Id),
				nameof(Models.TokenVerseAssociation.TokenComponentId),
				nameof(Models.TokenVerseAssociation.VerseId),
				nameof(Models.TokenVerseAssociation.Position),
				nameof(Models.TokenVerseAssociation.UserId),
				nameof(Models.TokenVerseAssociation.Created)
			}).ToArray();

			ApplyColumnsToInsertCommand(command, entityType, insertProperties);

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

        private static void ApplyColumnsToInsertCommand(DbCommand command, IEntityType entityType, IProperty[] columns)
        {
            var tableName = entityType.ToTableName();

            command.CommandText =
            $@"
                INSERT INTO {tableName} ({string.Join(", ", columns.Select(c => c.GetColumnName()))})
                VALUES ({string.Join(", ", columns.Select(c => command.ToParameterName(c.Name)))})
            ";

            foreach (var column in columns)
            {
                command.CreateAddParameter(column);
            }
        }

        public static async Task DeleteMachineAlignmentsByAlignmentSetIdAsync(Guid alignmentSetId, DbConnection connection, IModel metadataModel, CancellationToken cancellationToken)//IEnumerable<Guid> alignmentTopTargetTrainingTextGuidsToDelete,
        {   
            var entityType = metadataModel.ToEntityType(typeof(Models.Alignment));
            var originatedFromColumn = entityType.ToProperty(nameof(Models.Alignment.AlignmentOriginatedFrom));
            var alignmentSetIdColumn = entityType.ToProperty(nameof(Models.Alignment.AlignmentSetId));
                
            await using var command = connection.CreateCommand();
            command.CommandText = $@"
                DELETE FROM {entityType.ToTableName()}
                WHERE 
                    {originatedFromColumn} = '0' AND 
                    {alignmentSetIdColumn}  = '{alignmentSetId.ToString().ToUpper()}'    
            ";

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

		public static DbCommand CreateAlignmentSourceOrTargetIdSetCommand(DbConnection connection, bool isSource, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.Alignment));

			var columnName = isSource
		        ? nameof(DataAccessLayer.Models.Alignment.SourceTokenComponentId)
		        : nameof(DataAccessLayer.Models.Alignment.TargetTokenComponentId);

			var tokenComponentIdPropertyInfo = entityType.ToProperty(columnName);
			var idPropertyInfo = entityType.ToProperty(nameof(Models.IdentifiableEntity.Id));

			var columns = new IProperty[] { tokenComponentIdPropertyInfo };
			var whereColumns = new (IProperty, WhereEquality)[] { (idPropertyInfo, WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

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

		public static DbCommand CreateTranslationSourceIdSetCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.Translation));

			var sourceTokenComponentIdPropertyInfo = entityType.ToProperty(nameof(Models.Translation.SourceTokenComponentId));
			var idPropertyInfo = entityType.ToProperty(nameof(Models.IdentifiableEntity.Id));

			var columns = new IProperty[] { sourceTokenComponentIdPropertyInfo };
			var whereColumns = new (IProperty, WhereEquality)[] { (idPropertyInfo, WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SetTranslationSourceIdAsync(Guid translationId, Guid sourceTokenComponentId, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(DataAccessLayer.Models.Translation.SourceTokenComponentId)}"].Value = sourceTokenComponentId;
			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = translationId;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateTVATokenComponentIdSetCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.TokenVerseAssociation));

			var tokenComponentIdPropertyInfo = entityType.ToProperty(nameof(Models.TokenVerseAssociation.TokenComponentId));
			var idPropertyInfo = entityType.ToProperty(nameof(Models.IdentifiableEntity.Id));

			var columns = new IProperty[] { tokenComponentIdPropertyInfo };
			var whereColumns = new (IProperty, WhereEquality)[] { (idPropertyInfo, WhereEquality.Equals) };

			DataUtil.ApplyColumnsToUpdateCommand(
				command,
				entityType,
				columns,
				whereColumns,
				Array.Empty<(IProperty, int)>());

			command.Prepare();

			return command;
		}

		public static async Task SetTVATokenComponentIdAsync(Guid tvaId, Guid tokenComponentId, DbCommand command, CancellationToken cancellationToken)
		{
			command.Parameters[$"@{nameof(DataAccessLayer.Models.TokenVerseAssociation.TokenComponentId)}"].Value = tokenComponentId;
			command.Parameters[$"@{nameof(IdentifiableEntity.Id)}"].Value = tvaId;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
		}

		public static DbCommand CreateAlignmentDenormalizationTaskInsertCommand(DbConnection connection, IModel metadataModel)
		{
			var command = connection.CreateCommand();
			var entityType = metadataModel.ToEntityType(typeof(Models.AlignmentSetDenormalizationTask));
			var insertProperties = entityType.ToProperties(new List<string>
			{
				nameof(Models.AlignmentSetDenormalizationTask.Id),
				nameof(Models.AlignmentSetDenormalizationTask.AlignmentSetId),
				nameof(Models.AlignmentSetDenormalizationTask.SourceText)
			}).ToArray();

			DataUtil.ApplyColumnsToInsertCommand(command, entityType, insertProperties);

			command.Prepare();

			return command;
		}

		public static async Task<Guid> InsertAlignmentDenormalizationTaskAsync(Models.AlignmentSetDenormalizationTask denormalizationTask, DbCommand command, CancellationToken cancellationToken)
		{
			var id = Guid.NewGuid();

			command.Parameters["@Id"].Value = id;
			command.Parameters["@AlignmentSetId"].Value = denormalizationTask.AlignmentSetId;
			command.Parameters["@SourceText"].Value = !string.IsNullOrEmpty(denormalizationTask.SourceText) ? denormalizationTask.SourceText : DBNull.Value;

			_ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

			return id;
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