using System;
using System.Data;
using System.Data.SQLite;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.DataAccessLayer.Sqlite
{
    public class SqliteConnectionManager
    {
        public SQLiteConnection Connection { get; private set; }

        private readonly ILogger _logger;
        private readonly string _databasePath;
        public SqliteConnectionManager(string databasePath, ILogger logger)
        {
            _logger = logger;
            _databasePath = databasePath;
            Connection = new SQLiteConnection($"URI=file:{databasePath};");
            try
            {
                Connection.Open();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"An unexpected error occurred while creating and opening a connection to '{databasePath}'");
                throw;
            }
        }

        public SQLiteCommand CreateCommand(string commandText, CommandType commandType = CommandType.Text)
        {
            var command = Connection.CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }

        public void CloseConnection()
        {
            try
            {
                Connection.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"An unexpected error occurred while closing the connection to '{_databasePath}'.");
                throw;
            }
           
        }
    }
}