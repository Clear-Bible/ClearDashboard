using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.DataAccessLayer.Extensions
{

    /// <summary>
    /// Adds Dapper support to an EFCore DbContext
    /// </summary>
    /// <remarks>
    /// https://medium.com/geekculture/integrating-dapper-with-entity-framework-core-55aacc94b5b0
    /// </remarks>
    public static class DapperDbContextExtensions
    {
        public static async Task<IEnumerable<T>> QueryAsync<T>(
            this DbContext context,
            CancellationToken ct,
            string text,
            object parameters = null,
            int? timeout = null,
            CommandType? type = null
        )
        {
            using var command = new DapperEFCoreCommand(
                context,
                text,
                parameters,
                timeout,
                type,
                ct
            );

            var connection = context.Database.GetDbConnection();
            return await connection.QueryAsync<T>(command.Definition);
        }

        public static async Task<int> ExecuteAsync(
            this DbContext context,
            CancellationToken ct,
            string text,
            object parameters = null,
            int? timeout = null,
            CommandType? type = null
        )
        {
            using var command = new DapperEFCoreCommand(
                context,
                text,
                parameters,
                timeout,
                type,
                ct
            );

            var connection = context.Database.GetDbConnection();
            return await connection.ExecuteAsync(command.Definition);
        }
    }

    public readonly struct DapperEFCoreCommand : IDisposable
    {
        private readonly ILogger<DapperEFCoreCommand> _logger;

        public DapperEFCoreCommand(
            DbContext context,
            string text,
            object parameters,
            int? timeout,
            CommandType? type,
            CancellationToken ct
        )
        {
            _logger = context.GetService<ILogger<DapperEFCoreCommand>>();

            var transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            var commandType = type ?? CommandType.Text;
            var commandTimeout = timeout ?? context.Database.GetCommandTimeout() ?? 30;

            Definition = new CommandDefinition(
                text,
                parameters,
                transaction,
                commandTimeout,
                commandType,
                cancellationToken: ct
            );

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    @"Executing DbCommand [CommandType='{commandType}', CommandTimeout='{commandTimeout}']
{commandText}", Definition.CommandType, Definition.CommandTimeout, Definition.CommandText);
            }
        }

        public CommandDefinition Definition { get; }

        public void Dispose()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    @"Executed DbCommand [CommandType='{commandType}', CommandTimeout='{commandTimeout}']
{commandText}", Definition.CommandType, Definition.CommandTimeout, Definition.CommandText);
            }
        }
    }
}
