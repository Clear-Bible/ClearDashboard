using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
namespace ClearDashboard.DataAccessLayer.Data.Interceptors
{




    public class SqliteDatabaseConnectionInterceptor : DbConnectionInterceptor
    {
        public SqliteDatabaseConnectionInterceptor(string databaseName)
        {
            database = databaseName;
        }

        readonly string database;

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            if (database != null)
            {
                connection.ChangeDatabase(database); // The 'magic' code
            }
            base.ConnectionOpened(connection, eventData);
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            if (database != null)
            {
                await connection.ChangeDatabaseAsync(database); // The 'magic' code
            }
            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

    }
}
