using MediatR;
using Microsoft.Extensions.Logging;
using MySqlConnector;

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
            //string connectionString = DataAccessLayer.Models.Encryption.Decrypt(
            //    "IhxlhV+rjvducjKx0q2TlRD4opTViPRm5w/h7CvsGcLXmSAgrZLX1pWFLLYpWqS3");

            connectionString = "Server=cleardashboard.org;User ID=cleardas;Password=AmG$MsJSRb7@!R?6;Database=dashboard";
            await using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            await using MySqlCommand command = new MySqlCommand("SELECT * FROM gitlabusers;", connection);
            await using MySqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                object value = reader.GetValue(0);
                // do something with 'value'
            }


            //var connection = new SqliteConnection($"Data Source={ResourcePath};Cache=Shared");
            //connection.CreateCollation("NOCASE", (s1, s2) => string.Compare(s1, s2, StringComparison.InvariantCultureIgnoreCase));
            //try
            //{
            //    connection.Open();
            //    var cmd = connection.CreateCommand();
            //    cmd.CommandText = commandText;
            //    DataReader = cmd.ExecuteReader();

            //    return ProcessData();
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogError(ex, $"An unexpected error occurred while executing the following command: '{commandText}'");
            //    throw;
            //}
            //finally
            //{
            //    connection.Close();
            //    SqliteConnection.ClearPool(connection);
            //}

            return ProcessData();
        }
    }
}
