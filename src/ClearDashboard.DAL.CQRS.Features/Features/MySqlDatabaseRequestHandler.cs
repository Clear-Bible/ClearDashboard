using MediatR;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System.Diagnostics;

namespace ClearDashboard.DAL.CQRS.Features.Features
{
    /// <summary>
    /// A base class used to query data from Sqlite databases - typically found in the "Resources" folder found in the directory of the executable.
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="TData"></typeparam>
    public abstract class
        MySqlDatabaseRequestHandler<TRequest, TResponse, TData> : ResourceRequestHandler<TRequest, TResponse, TData>
        where TRequest : IRequest<TResponse>
    {
        protected MySqlDataReader? DataReader { get; private set; }

        protected MySqlDatabaseRequestHandler(ILogger logger) : base(logger)
        {
            //no-op
        }

        protected async Task<TData> ExecuteMySqlCommandAndProcessData(string connectionString, string commandText)
        {
            await using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            await using MySqlCommand command = new MySqlCommand("SELECT * FROM gitlabusers;", connection);
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                object value = reader.GetValue(0);
                object value1 = reader.GetValue(1);
                object value2 = reader.GetValue(2);
            }

            return ProcessData();
        }


        protected async Task<TData> ExecuteMySqlCommand(string connectionString,
            int userId,
            string remoteUserName,
            string remoteEmail,
            string remotePersonalAccessToken,
            string remotePersonalPassword,
            string group,
            int namespaceId)
        {
            try
            {
                await using MySqlConnection connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                await using MySqlCommand command = new MySqlCommand("INSERT INTO gitlabusers"
                    + " (UserId, RemoteUserName, RemoteEmail, RemotePersonalAccessToken, RemotePersonalPassword, GroupName, NamespaceId)"
                    + $" VALUES ({userId}, \"{remoteUserName}\", \"{remoteEmail}\", \"{remotePersonalAccessToken}\",\"{remotePersonalPassword}\",\"{group}\",{namespaceId});", connection);
                
                var ret = await command.ExecuteNonQueryAsync();

                //if (ret != 0)
                //{
                //    return true;
                //}
                //else
                //{
                    
                //}
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            return ProcessData();
        }
    }
}
