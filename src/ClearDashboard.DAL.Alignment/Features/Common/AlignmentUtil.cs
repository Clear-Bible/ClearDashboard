﻿using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SIL.Machine.Corpora;
using SIL.Scripture;
using System.Data.Common;
using System.Text.Json;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class AlignmentUtil
    {
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