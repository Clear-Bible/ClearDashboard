using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using ClearDashboard.DataAccessLayer.Data;
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Data.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;

namespace ClearDashboard.DAL.Alignment.Features.Common
{
    public static class DataUtil
    {
        public static async Task<T> BulkWriteToDatabaseTransactional<S,T>(
            ProjectDbContext projectDbContext, 
            Func<DbConnection, IModel, CancellationToken, Task<S>> bulkWriteFunc,
            Func<ProjectDbContext, S, Task<T>> postCommitReturnValueGenerator,
            CancellationToken cancellationToken)
        {
            //var connectionWasOpen = ProjectDbContext.Database.GetDbConnection().State == ConnectionState.Open; 
            //if (!connectionWasOpen)
            //{
            var metadataModel = projectDbContext.Model;
            await projectDbContext.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            //}

            try
            {
                // Generally follows https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite/bulk-insert
                // mostly using database connection-level functions, commands, paramters etc.
                using var connection = projectDbContext.Database.GetDbConnection();
                using var transaction = await connection.BeginTransactionAsync(cancellationToken);

                S bulkWriteOutput = await bulkWriteFunc(connection, metadataModel, cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return await postCommitReturnValueGenerator(projectDbContext, bulkWriteOutput);
            }
            finally
            {
                //if (!connectionWasOpen)
                //{
                await projectDbContext.Database.CloseConnectionAsync().ConfigureAwait(false);
                //}
            }
        }

        public const string NOT_NULL = "{NOT_NULL}";
        public static void ApplyColumnsToInsertCommand(DbCommand command, IEntityType entityType, IProperty[] columns)
        {
            var tableName = entityType.ToTableName();
        
            command.CommandText =
            $@"
                INSERT INTO {tableName} ({string.Join(", ", columns.Select(c => c.GetColumnName()))})
                VALUES ({string.Join(", ", columns.Select(c => command.ToParameterName(c.Name)))})
            ";

            AddWhereClauseParameters(command, columns, Array.Empty<(IProperty, int)>());
        }

        public static void ApplyColumnsToUpdateCommand(DbCommand command, IEntityType entityType, IProperty[] columns, (IProperty PropertyInfo, WhereEquality whereEquality)[] whereColumns, (IProperty PropertyInfo, int Count)[] whereInColumns)
        {
            var tableName = entityType.ToTableName();
            var whereStrings = BuildWhereStrings(command, whereColumns, whereInColumns);

            if (!whereStrings.Any())
            {
                throw new Exception($"No where clause column names provided to Update '{tableName}' query");
            }

            command.CommandText =
            $@"
                UPDATE {tableName}
                SET {string.Join(", ", columns.Select(c => c.GetColumnName() + " = " + command.ToParameterName(c.Name)))}
                WHERE {string.Join(" AND ", whereStrings)}
            ";

            AddWhereClauseParameters(
                command, 
                columns.Union(whereColumns.Select(e => e.PropertyInfo)).ToArray(), 
                whereInColumns);
        }

        public static void ApplyColumnsToDeleteCommand(DbCommand command, IEntityType entityType, (IProperty PropertyInfo, WhereEquality WhereEquality)[] whereColumns, (IProperty PropertyInfo, int Count)[] whereInColumns)
        {
            var tableName = entityType.ToTableName();
            var whereStrings = BuildWhereStrings(command, whereColumns, whereInColumns);

            if (!whereStrings.Any())
            {
                throw new Exception($"No where clause column names provided to Delete '{tableName}' query");
            }

            command.CommandText =
                $@"
                DELETE FROM {tableName}
                WHERE {string.Join(" AND ", whereStrings)}
            ";

            AddWhereClauseParameters(command, whereColumns.Select(e => e.PropertyInfo).ToArray(), whereInColumns);
        }

        public enum WhereEquality
        {
            Equals,
            Is,
            IsNot
        }

        public static string BuildSelectCommandText(DbCommand command, IEntityType entityType, string[] selectColumns, (IProperty PropertyInfo, WhereEquality WhereEquality)[] whereColumns, (IProperty PropertyInfo, int Count)[] whereInColumns, (Type JoinType, string JoinColumn, string FromColumn)[] joins, bool notIndexed)
        {
            var tableName = entityType.ToTableName();
            var whereStrings = BuildWhereStrings(command, whereColumns, whereInColumns);

            var notIndexedString = notIndexed ? " NOT INDEXED " : string.Empty;

            var joinStringBuilder = new StringBuilder();
            foreach (var join in joins)
            {
                joinStringBuilder.AppendLine($"JOIN {join.JoinType.Name} ON {join.JoinType.Name}.{join.JoinColumn} = {tableName}.{join.FromColumn}");
            }

            return
            $@"
                SELECT {string.Join(", ", selectColumns)}
                FROM {tableName} {notIndexedString}
                {joinStringBuilder}
                WHERE {string.Join(" AND ", whereStrings)}
            ";
        }

        public static (IProperty PropertyInfo, WhereEquality WhereEquality)[] ExtractWhereColumns(IEnumerable<(IProperty PropertyInfo, object? ColumnValue)> whereClause)
        {
            return whereClause
                .Where(e => 
                    e.ColumnValue == null || 
                    e.ColumnValue!.GetType() == typeof(string) || 
                    e.ColumnValue!.GetType().GetInterface(nameof(System.Collections.IEnumerable)) == null)
                .Select(e =>
                {
                    if (e.ColumnValue == null)
                        return (e.PropertyInfo, WhereEquality.Is);
                    else if ((e.ColumnValue as string) == NOT_NULL)
                        return (e.PropertyInfo, WhereEquality.IsNot);
                    return (e.PropertyInfo, WhereEquality.Equals);
                })
                .ToArray();
        }

        public static (IProperty PropertyInfo, int Count)[] ExtractWhereInColumns(IEnumerable<(IProperty PropertyInfo, object? ColumnValue)> whereClause)
        {
            return whereClause
                .Where(e => e.ColumnValue != null)
                .Where(e => e.ColumnValue!.GetType() != typeof(string))
                .Where(e => e.ColumnValue!.GetType().GetInterface(nameof(System.Collections.IEnumerable)) != null)
                .Select(e => (e.PropertyInfo, count: (e.ColumnValue as IEnumerable)!.Cast<object>().Count()))
                .ToArray();
        }

        public static IEnumerable<string> BuildWhereStrings(DbCommand command, (IProperty PropertyInfo, WhereEquality WhereEquality)[] whereColumns, (IProperty PropertyInfo, int Count)[] whereInColumns)
        {
            var whereStrings = new List<string>();

            var whereEquals = whereColumns.Where(e => e.WhereEquality == WhereEquality.Equals);
            var whereIs = whereColumns.Where(e => e.WhereEquality == WhereEquality.Is);
            var whereIsNot = whereColumns.Where(e => e.WhereEquality == WhereEquality.IsNot);

            if (whereEquals.Any())
            {
                // E.g.:  " AND TokenizedCorpusId = @TokenizedCorpusId"
                whereStrings.Add($"{string.Join(" AND ", whereEquals.Select(c => c.PropertyInfo.GetColumnName() + " = " + command.ToParameterName(c.PropertyInfo.Name)))}");
            }
            if (whereIs.Any())
            {
                // E.g.:  " AND Deleted IS @Deleted"
                whereStrings.Add($"{string.Join(" AND ", whereIs.Select(c => c.PropertyInfo.GetColumnName() + " IS " + command.ToParameterName(c.PropertyInfo.Name)))}");
            }
            if (whereIsNot.Any())
            {
                // E.g.:  " AND Deleted IS @Deleted"
                whereStrings.Add($"{string.Join(" AND ", whereIsNot.Select(c => c.PropertyInfo.GetColumnName() + " IS NOT " + command.ToParameterName(c.PropertyInfo.Name)))}");
            }
            if (whereInColumns.Length != 0)
            {
                // E.g.:  " AND EngineTokenId IN (@EngineTokenId0, @EngineTokenId1)"
                whereStrings.Add(string.Join(" AND ", whereInColumns
                    .Select(column => column.PropertyInfo.GetColumnName() + " IN (" + BuildWhereInParameterString(command, column.PropertyInfo.Name, column.Count) + ")")));
            }

            return whereStrings;
        }

        public static string BuildWhereInParameterString(DbCommand command, string columnName, int count)
        {
            return string.Join(", ", Enumerable.Range(0, count).Select(e => command.ToParameterName(columnName, count)));
        }

        public static void AddWhereClauseParameters(DbCommand command, IProperty[] whereColumns, (IProperty Property, int Count)[] whereInColumns)
        {
            foreach (var property in whereColumns)
            {
                command.CreateAddParameter(property);
            }

            foreach (var (property, columnCount) in whereInColumns)
            {
                foreach (var nameIndex in Enumerable.Range(0, columnCount))
                {
                    command.CreateAddParameter(property, nameIndex);
                }
            }
        }

        public static void AddWhereClauseParameterValues(DbCommand command, IEnumerable<(IProperty PropertyInfo, object? ParameterValue)> whereClause)
        {
            foreach (var item in whereClause)
            {
                if (item.ParameterValue is not null && 
                    item.ParameterValue!.GetType() != typeof(string) && 
                    item.ParameterValue!.GetType().GetInterface(nameof(System.Collections.IEnumerable)) != null)
                {
                    var valueEnumerable = (item.ParameterValue as IEnumerable)!.Cast<object>();
                    foreach (var column in valueEnumerable.Select((value, nameIndex) => new { value, nameIndex }))
                    {
                        command.Parameters[command.ToParameterName(item.PropertyInfo.Name, column.nameIndex)].Value = column.value;
                    }
                }
                else if (item.ParameterValue is null || (item.ParameterValue as string) == NOT_NULL)
                {
                    command.Parameters[command.ToParameterName(item.PropertyInfo.Name)].Value = DBNull.Value;
                }
                else
                {
                    command.Parameters[command.ToParameterName(item.PropertyInfo.Name)].Value = item.ParameterValue;
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

        public static async Task<IEnumerable<Dictionary<string, object?>>> SelectEntityValuesAsync(DbConnection? connection, IEntityType entityType, IEnumerable<string> selectColumns, IEnumerable<(IProperty PropertyInfo, object? ParameterValue)> whereClause, IEnumerable<(Type JoinType, string JoinColumn, string FromColumn)> joins, bool useNotIndexedInFromClause, CancellationToken cancellationToken)
        {
            if (connection is null) throw new Exception($"Database connnection is null - MergeStartAsync must be called prior to calling this method");

            var whereColumns = ExtractWhereColumns(whereClause);
            var whereInColumns = ExtractWhereInColumns(whereClause);

            await using var command = connection.CreateCommand();

            command.CommandType = CommandType.Text;
            command.CommandText = BuildSelectCommandText(
                command,
                entityType,
                selectColumns.ToArray(),
                whereColumns,
                whereInColumns,
                joins.ToArray(),
                useNotIndexedInFromClause);

            AddWhereClauseParameters(command, whereColumns.Select(e => e.PropertyInfo).ToArray(), whereInColumns);

            try
            {
                command.Prepare();
            }
            catch (Exception ex)
            {
                throw new Exception($"Preparing command to select data for entity type '{entityType.DisplayName()}' failed with the following error: {ex.Message}", ex);
            }

            AddWhereClauseParameterValues(command, whereClause);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = ReadSelectDbDataReader(reader);

            return results;
        }

        public static async Task DeleteIdentifiableEntityAsync(DbConnection dbConnection, IEntityType entityType, IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            if (!entityType.ClrType.IsAssignableTo(typeof(Models.IdentifiableEntity)))
            {
                throw new Exception($"Table/entity type '{entityType.DisplayName()}' is not assignable to '{nameof(Models.IdentifiableEntity)}'");
            }

            if (!ids.Any())
            {
                throw new Exception($"Call to delete from table/entity type '{entityType.DisplayName()}' contains no ids");
            }

            var idPropertyInfo = entityType.ToProperty(nameof(Models.IdentifiableEntity.Id));
            var whereClause = new List<(IProperty, object?)>() {
                { (idPropertyInfo, ids) } 
            };

            await using var command = dbConnection.CreateCommand();

            ApplyColumnsToDeleteCommand(
                command, 
                entityType, 
                Array.Empty<(IProperty, WhereEquality)>(), 
                new (IProperty PropertyInfo, int Count)[] { (idPropertyInfo, ids.Count()) });

            try
            {
                command.Prepare();
            }
            catch (Exception ex)
            {
                throw new Exception($"Preparing command to delete data for entity type '{entityType.DisplayName()}' failed with the following error: {ex.Message}", ex);
            }

            AddWhereClauseParameterValues(command, whereClause);

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        public static DbCommand CreateSoftDeleteByIdUpdateCommand(DbConnection connection, IEntityType entityType)
        {
            var command = connection.CreateCommand();

            var deletedPropertyInfo = entityType.ToProperty(nameof(Models.Alignment.Deleted));
            var idPropertyInfo = entityType.ToProperty(nameof(Models.IdentifiableEntity.Id));

            var columns = new IProperty[] { deletedPropertyInfo };
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

        public static async Task SoftDeleteByIdAsync(DateTimeOffset deleted, Guid id, DbCommand command, CancellationToken cancellationToken)
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            command.Parameters[command.ToParameterName(nameof(Models.Alignment.Deleted))].Value = converter.ConvertToProvider(deleted);
            command.Parameters[command.ToParameterName(nameof(Models.IdentifiableEntity.Id))].Value = id;

            _ = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        private static string ToDbCommandParameterName(this string parameterName)
        {
            return parameterName.Replace('.', '_');
        }
    }
}
