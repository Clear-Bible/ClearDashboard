using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
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

            AddWhereClauseParameters(command, columns, Array.Empty<(string, int)>());
        }

        public static void ApplyColumnsToUpdateCommand(DbCommand command, Type type, string[] columns, (string name, WhereEquality whereEquality)[] whereColumns, (string name, int count)[] whereInColumns)
        {
            var whereStrings = BuildWhereStrings(whereColumns, whereInColumns);

            if (!whereStrings.Any())
            {
                throw new Exception($"No where clause column names provided to Update '{type.ShortDisplayName()}' query");
            }

            command.CommandText =
            $@"
                UPDATE {type.Name}
                SET {string.Join(", ", columns.Select(c => c + " = @" + c))}
                WHERE {string.Join(" AND ", whereStrings)}
            ";

            AddWhereClauseParameters(command, whereColumns.Select(e => e.name).ToArray(), whereInColumns);
        }

        public static void ApplyColumnsToDeleteCommand(DbCommand command, Type type, (string name, WhereEquality whereEquality)[] whereColumns, (string name, int count)[] whereInColumns)
        {
            var whereStrings = BuildWhereStrings(whereColumns, whereInColumns);

            if (!whereStrings.Any())
            {
                throw new Exception($"No where clause column names provided to Delete '{type.ShortDisplayName()}' query");
            }

            command.CommandText =
                $@"
                DELETE FROM {type.Name}
                WHERE {string.Join(" AND ", whereStrings)}
            ";

            AddWhereClauseParameters(command, whereColumns.Select(e => e.name).ToArray(), whereInColumns);
        }

        public enum WhereEquality
        {
            Equals,
            Is,
            IsNot
        }

        public static string BuildSelectCommandText(Type type, string[] selectColumns, (string name, WhereEquality whereEquality)[] whereColumns, (string name, int count)[] whereInColumns, (Type JoinType, string JoinColumn, string FromColumn)[] joins, bool notIndexed)
        {
            var whereStrings = BuildWhereStrings(whereColumns, whereInColumns);

            var notIndexedString = notIndexed ? " NOT INDEXED " : string.Empty;

            var joinStringBuilder = new StringBuilder();
            foreach (var join in joins)
            {
                joinStringBuilder.AppendLine($"JOIN {join.JoinType.Name} ON {join.JoinType.Name}.{join.JoinColumn} = {type.Name}.{join.FromColumn}");
            }

            return
            $@"
                SELECT {string.Join(", ", selectColumns)}
                FROM {type.Name} {notIndexedString}
                {joinStringBuilder}
                WHERE {string.Join(" AND ", whereStrings)}
            ";
        }

        public static (string name, WhereEquality whereEquality)[] ExtractWhereColumns(Dictionary<string, object?> whereClause)
        {
            return whereClause
                .Where(e => e.Value == null || !e.Value.GetType().IsAssignableTo(typeof(IEnumerable<object>)))
                .Select(e =>
                {
                    if (e.Value == null)
                        return (e.Key, WhereEquality.Is);
                    else if ((e.Value as string) == NOT_NULL)
                        return (e.Key, WhereEquality.IsNot);
                    return (e.Key, WhereEquality.Equals);
                })
                .ToArray();
        }

        public static (string name, int count)[] ExtractWhereInColumns(Dictionary<string, object?> whereClause)
        {
            return whereClause
                .Where(e => e.Value != null && e.Value.GetType().IsAssignableTo(typeof(IEnumerable<object>)))
                .Select(e => (name: e.Key, count: (e.Value as IEnumerable)!.Cast<object>().Count()))
                .ToArray();
        }

        public static IEnumerable<string> BuildWhereStrings((string name, WhereEquality whereEquality)[] whereColumns, (string name, int count)[] whereInColumns)
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
                    .Select(column => column.name + " IN (" + BuildWhereInParameterString(column.name, column.count) + ")")));
            }

            return whereStrings;
        }

        public static string BuildWhereInParameterString(string columnName, int count)
        {
            return string.Join(", ", Enumerable.Range(0, count).Select(e => $"@{columnName}{e}"));
        }

        public static void AddWhereClauseParameters(DbCommand command, string[] whereColumnNames, (string name, int count)[] whereInColumns)
        {
            foreach (var columnName in whereColumnNames)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{columnName}";
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

        public static void AddWhereClauseParameterValues(DbCommand command, Dictionary<string, object?> whereClause)
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

        public static IEnumerable<Dictionary<string, object?>> ReadSelectDbDataReader(DbDataReader reader)
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

        public static async Task<IEnumerable<Dictionary<string, object?>>> SelectEntityValuesAsync(DbConnection? connection, Type tableType, IEnumerable<string> selectColumns, Dictionary<string, object?> whereClause, IEnumerable<(Type JoinType, string JoinColumn, string FromColumn)> joins, bool useNotIndexedInFromClause, CancellationToken cancellationToken)
        {
            if (connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");

            var whereColumns = ExtractWhereColumns(whereClause);
            var whereInColumns = ExtractWhereInColumns(whereClause);

            await using var command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = BuildSelectCommandText(
                tableType,
                selectColumns.ToArray(),
                whereColumns,
                whereInColumns,
                joins.ToArray(),
                useNotIndexedInFromClause);

            AddWhereClauseParameters(command, whereColumns.Select(e => e.name).ToArray(), whereInColumns);

            try
            {
                command.Prepare();
            }
            catch (Exception ex)
            {
                throw new Exception($"Preparing command to get data from table type '{tableType}' failed with the following error: {ex.Message}", ex);
            }

            AddWhereClauseParameterValues(command, whereClause);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = ReadSelectDbDataReader(reader);

            return results;
        }

        public static async Task DeleteIdentifiableEntityAsync(DbConnection dbConnection, Type tableType, IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            if (!tableType.IsAssignableTo(typeof(Models.IdentifiableEntity)))
            {
                throw new Exception($"Table/entity type '{tableType.Name}' is not assignable to '{nameof(Models.IdentifiableEntity)}'");
            }

            if (!ids.Any())
            {
                throw new Exception($"Call to delete from table/entity type '{tableType.Name}' contains no ids");
            }

            var whereClause = new Dictionary<string, object?>() {
                { nameof(Models.IdentifiableEntity.Id), ids } 
            };

            await using var command = dbConnection.CreateCommand();

            ApplyColumnsToDeleteCommand(
                command, 
                tableType, 
                Array.Empty<(string, WhereEquality)>(), 
                new (string name, int count)[] { (whereClause.Keys.First(), ids.Count()) });

            try
            {
                command.Prepare();
            }
            catch (Exception ex)
            {
                throw new Exception($"Preparing command to get data from table type '{tableType}' failed with the following error: {ex.Message}", ex);
            }

            AddWhereClauseParameterValues(command, whereClause);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateSoftDeleteByIdUpdateCommand(DbConnection connection, Type type)
        {
            var command = connection.CreateCommand();
            var columns = new string[] { nameof(Models.Alignment.Deleted) };
            var whereColumns = new (string, WhereEquality)[] { (nameof(Models.IdentifiableEntity.Id), WhereEquality.Equals) };

            DataUtil.ApplyColumnsToUpdateCommand(
                command, 
                type, 
                columns, 
                whereColumns,
                Array.Empty<(string, int)>());

            command.Prepare();

            return command;
        }

        public static async Task SoftDeleteByIdAsync(DateTimeOffset deleted, Guid id, DbCommand command, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            command.Parameters[$"@{nameof(Models.Alignment.Deleted)}"].Value = converter.ConvertToProvider(deleted);
            command.Parameters[$"@{nameof(Models.IdentifiableEntity.Id)}"].Value = id;

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

    }
}
