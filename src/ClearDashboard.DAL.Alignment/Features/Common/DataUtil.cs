using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Data.Common;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class DataUtil
    {
        public static void ApplyColumnsToInsertCommand(DbCommand command, Type type, string[] columns)
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

        public static void ApplyColumnsToUpdateCommand(DbCommand command, Type type, string[] columns, string[] whereColumns)
        {
            command.CommandText =
            $@"
                UPDATE {type.Name}
                SET {string.Join(", ", columns.Select(c => c + " = @" + c))}
                WHERE {string.Join(", ", whereColumns.Select(c => c + " = @" + c))}
            ";

            foreach (var column in columns.Union(whereColumns))
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{column}";
                command.Parameters.Add(parameter);
            }
        }

        public static DbCommand CreateSoftDeleteByIdUpdateCommand(DbConnection connection, Type type)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { "Deleted" };
            var whereColumns = new string[] { "Id" };

            DataUtil.ApplyColumnsToUpdateCommand(command, type, columns, whereColumns);

            command.Prepare();

            return command;
        }

        public static async Task SoftDeleteByIdAsync(DateTimeOffset deleted, Guid id, DbCommand command, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            command.Parameters["@Deleted"].Value = converter.ConvertToProvider(deleted);
            command.Parameters["@Id"].Value = id;

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

    }
}
