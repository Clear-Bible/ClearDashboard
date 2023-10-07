using ClearBible.Engine.Corpora;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class DataUtil
    {
        public const string NOT_NULL = "{NOT_NULL}";
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

        public static void ApplyColumnsToDeleteCommand(DbCommand command, Type type, string[] whereColumns)
        {
            command.CommandText =
                $@"
                DELETE FROM {type.Name}
                WHERE {string.Join(", ", whereColumns.Select(c => c + " = @" + c))}
            ";

            foreach (var column in whereColumns)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{column}";
                command.Parameters.Add(parameter);
            }
        }

        public enum WhereEquality
        {
            Equals,
            Is,
            IsNot
        }

        public static void ApplyColumnsToSelectCommand(DbCommand command, Type type, string[] selectColumns, (string name, WhereEquality whereEquality)[] whereColumns, (string name, int count)[] whereInColumns, bool notIndexed)
        {
            var whereStrings = new List<string>();

            var whereEquals = whereColumns.Where(e => e.whereEquality == WhereEquality.Equals);
            var whereIs = whereColumns.Where(e => e.whereEquality == WhereEquality.Is);
            var whereIsNot = whereColumns.Where(e => e.whereEquality == WhereEquality.IsNot);

            if (whereEquals.Any())
            {
                // E.g.:  " AND TokenizedCorpusId = @TokenizedCorpusId"
                whereStrings.Add($"{string.Join(" AND ", whereEquals.Select(c => c.name + " = @" + c.name))}");
            }
            if (whereIs.Any())
            {
                // E.g.:  " AND Deleted IS @Deleted"
                whereStrings.Add($"{string.Join(" AND ", whereIs.Select(c => c.name + " IS @" + c.name))}");
            }
            if (whereIsNot.Any())
            {
                // E.g.:  " AND Deleted IS @Deleted"
                whereStrings.Add($"{string.Join(" AND ", whereIsNot.Select(c => c.name + " IS NOT @" + c.name))}");
            }
            if (whereInColumns.Any())
            {
                // E.g.:  " AND EngineTokenId IN (@EngineTokenId0, @EngineTokenId1)"
                whereStrings.Add(string.Join(" AND ", whereInColumns
                    .Select(column => column.name + " IN (" + string.Join(", ", Enumerable.Range(0, column.count)
                        .Select(e => $"@{column.name}{e}")) + ")")));
            }

            var notIndexedString = notIndexed ? " NOT INDEXED " : string.Empty;
            command.CommandText =
            $@"
                SELECT {string.Join(", ", selectColumns)}
                FROM {type.Name} {notIndexedString}
                WHERE {string.Join(" AND ", whereStrings)}
            ";
            
            foreach (var column in whereColumns)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{column.name}";
                command.Parameters.Add(parameter);
            }

            foreach (var (columnName, columnCount) in whereInColumns)
            {
                foreach (var nameIndex in Enumerable.Range(0, columnCount))
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = $"@{columnName}{nameIndex}";
                    command.Parameters.Add(parameter);
                }
            }
        }

        private static void AddWhereClauseParameters(DbCommand command, Dictionary<string, object?> whereClause)
        {
            foreach (var kvp in whereClause)
            {
                if (kvp.Value is not null && kvp.Value.GetType().IsAssignableTo(typeof(IEnumerable<object>)))
                {
                    var valueEnumerable = (kvp.Value as IEnumerable)!.Cast<object>();
                    foreach (var column in valueEnumerable.Select((value, nameIndex) => new { value, nameIndex }))
                    {
                        command.Parameters[$"@{kvp.Key}{column.nameIndex}"].Value = column.value;
                    }
                }
                else if (kvp.Value is null || (kvp.Value as string) == NOT_NULL)
                {
                    command.Parameters[$"@{kvp.Key}"].Value = DBNull.Value;
                }
                else
                {
                    command.Parameters[$"@{kvp.Key}"].Value = kvp.Value;
                }
            }
        }

        private static IEnumerable<Dictionary<string, object?>> ReadSelectDbDataReader(DbDataReader reader)
        {
            var results = new List<Dictionary<string, object?>>();

            if (reader.HasRows)
            {
                var columnSchema = reader.GetColumnSchema()
                    .Where(e => e.ColumnOrdinal != null)
                    .Select(e => (e.ColumnName, ColumnOrdinal: e.ColumnOrdinal!.Value))
                    .ToList();

                while (reader.Read())
                {
                    var resultRow = new Dictionary<string, object?>();
                    foreach (var (columnName, columnOrdinal) in columnSchema)
                    {
                        resultRow.Add(
                            columnName,
                            !reader.IsDBNull(columnOrdinal) ? reader.GetValue(columnOrdinal) : null
                        );
                    }
                    results.Add(resultRow);
                }
            }

            return results;
        }

        public static async Task<IEnumerable<Dictionary<string, object?>>> SelectEntityValuesAsync(DbConnection? connection, Type tableType, IEnumerable<string> selectColumns, Dictionary<string, object?> whereClause, bool useNotIndexedInFromClause, CancellationToken cancellationToken)
        {
            if (connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;

            ApplyColumnsToSelectCommand(
                command,
                tableType,
                selectColumns.ToArray(),
                whereClause
                    .Where(e => e.Value == null || !e.Value.GetType().IsAssignableTo(typeof(IEnumerable<object>)))
                    .Select(e => 
                    {
                        if (e.Value == null)
                            return (e.Key, WhereEquality.Is);
                        else if ((e.Value as string) == NOT_NULL)
                            return (e.Key, WhereEquality.IsNot);
                        return (e.Key, WhereEquality.Equals);
                    })
                    .ToArray(),
                whereClause
                    .Where(e => e.Value != null && e.Value.GetType().IsAssignableTo(typeof(IEnumerable<object>)))
                    .Select(e => (name: e.Key, count: (e.Value as IEnumerable)!.Cast<object>().Count()))
                    .ToArray(),
                useNotIndexedInFromClause);

            try
            {
                command.Prepare();
            }
            catch (Exception ex)
            {
                throw new Exception($"Preparing command to get data from table type '{tableType}' failed with the following error: {ex.Message}", ex);
            }

            AddWhereClauseParameters(command, whereClause);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = ReadSelectDbDataReader(reader);

            return results;
        }

        public static async Task<int> DeleteEntityValuesAsync(DbConnection? connection, Type tableType, Dictionary<string, object?> whereClause, CancellationToken cancellationToken)
        {
            if (connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;

            ApplyColumnsToDeleteCommand(
                command,
                tableType,
                whereClause.Keys.ToArray());

            try
            {
                command.Prepare();
            }
            catch (Exception ex)
            {
                throw new Exception($"Preparing command to get data from table type '{tableType}' failed with the following error: {ex.Message}", ex);
            }

            AddWhereClauseParameters(command, whereClause);

            return await command.ExecuteNonQueryAsync(cancellationToken);
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
