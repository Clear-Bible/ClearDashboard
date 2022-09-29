using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using ClearDashboard.DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Data.Interceptors
{

    public class SqliteDatabaseConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly ProjectDbContextFactory _projectDbContextFactory;
        private readonly ILogger<SqliteDatabaseConnectionInterceptor> _logger;

        public SqliteDatabaseConnectionInterceptor(ILogger<SqliteDatabaseConnectionInterceptor> logger,  ProjectDbContextFactory projectDbContextFactory)
        {
            _logger = logger;
            _projectDbContextFactory = projectDbContextFactory;
        }
        
        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            // NB:  Adding "DataSource=" to the connection string is required, otherwise we get an unhelpful error:
            //       "Format of the initialization string does not conform to specification starting at index 0."
            connection.ConnectionString = $"DataSource={_projectDbContextFactory.ProjectAssets.DataContextPath};Pooling=true;Mode=ReadWriteCreate";
            _logger.LogDebug($"Open database with: {connection.ConnectionString}");
            return base.ConnectionOpening(connection, eventData, result);
        }

        public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
        {
            _logger.LogDebug($"Closing database for '{_projectDbContextFactory.ProjectAssets.ProjectName}'");
            base.ConnectionClosed(connection, eventData);
        }
    }
}
