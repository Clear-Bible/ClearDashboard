using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using ClearDashboard.DAL.Interfaces;

namespace ClearDashboard.DataAccessLayer.Data.Interceptors
{

    public class SqliteDatabaseConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly ProjectDbContextFactory _projectDbContextFactory;

        public SqliteDatabaseConnectionInterceptor(ProjectDbContextFactory projectDbContextFactory)
        {
            _projectDbContextFactory = projectDbContextFactory;
        }
        
        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            // NB:  Adding "DataSource=" to the connection string is required, otherwise we get an unhelpful error:
            //       "Format of the initialization string does not conform to specification starting at index 0."
            connection.ConnectionString = $"DataSource={_projectDbContextFactory.ProjectAssets.DataContextPath}";
            return base.ConnectionOpening(connection, eventData, result);
        }

    }
}
