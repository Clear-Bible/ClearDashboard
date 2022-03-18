using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Dapper;

namespace ClearDashboard.DataAccessLayer.Dapper
{
    /// <summary>
    /// An adapter for the Sqlite DB that allows any Plain Old C Object (POCO) to be filled
    /// with no left == right; code. Simpler than EF for people that like to write their own SQL
    /// and manage their own database updates.
    /// 
    /// I try to reduce the complexity of CRUD operations, but it requires a bit of convention to make it work.
    /// Any POCO that you want to use with this should implement the IDataObject interface.
    /// </summary>
    public class DapperDataProvider : IDisposable
    {
        private SQLiteConnection Conn;

        /// <summary>
        /// Create a connection to a default DB file.
        /// THis is not useful in production until the current folder for the working JSON file is available as a global variable.
        /// </summary>
        // public DapperDataProvider()
        // {
        //     CreateConnectionToFilename("manuscript");
        // }

        /// <summary>
        /// Create a connection to a DB file at the path location. Will attempt to create the path and DB file if createFile is true.
        /// </summary>
        /// <param abbr="dbFilePath">The full path including file abbr and extension. The extension should be '.sqlite'.</param>
        /// <param abbr="createFile">If true it will create the DB file if it does not exist.</param>
        public DapperDataProvider(string dbFilePath, bool createFile = false)
        {
            // Check for any errors in the path and filename.
            string dbFilePathErrors = DatabasePathFileNameErrors(dbFilePath);
            if(dbFilePathErrors != string.Empty)
            {
                throw new ArgumentException(dbFilePathErrors, $"The path '{dbFilePath}'.");
            }

            CreateConnection(dbFilePath, createFile);
        }

        /// <summary>
        /// Executes an SQL statement with the query object as parameters of the SQL text. 
        /// Can be used to create, alter, and drop tables, create, alter, and drop indexes; insert, and update records.
        /// If an INSERT or UPDATE query returns the updated record, please use ExecuteGetFirst or ExecuteGetFirstAsync.
        /// Use Delete to delete a record since it will also delete the object.
        /// </summary>
        /// <param abbr="query">A text SQL statement.</param>
        /// <param abbr="queryObj">Parameter POCO object</param>
        /// <returns>Number of records inserted, updated, or deleted. 
        /// Can also be used to create tables and indexes.</returns>
        public int ExecuteQuery(string query, object queryObj)
        {
            CheckQuery(query);

            int result = Conn.Execute(query, queryObj);
            return result;
        }

        /// <summary>
        /// Executes the query, and if the result is 1 the record has been deleted.
        /// </summary>
        /// <param abbr="objToDelete"></param>
        /// <returns>The number of records changed.</returns>
        public async Task<int> DeleteAsync(DataObject dataObj, string query)
        {
            CheckQuery(query);
            int result = await Conn.ExecuteAsync(query, dataObj);
            return result;
        }

        /// <summary>
        /// Inserts an object into the DB.
        /// </summary>
        /// <typeparam abbr="T">Type of the object being inserted into the DB</typeparam>
        /// <param abbr="dataObj">The actual data object</param>
        /// <returns>The data object as read from the DB.</returns>
        public T Insert<T>(DataObject dataObj, string query)
        {
            CheckQuery(query);
            T newDataObj = Conn.QueryFirst<T>(query, dataObj);
            return newDataObj;
        }

        /// <summary>
        /// Inserts an object into the DB.
        /// </summary>
        /// <typeparam abbr="T">Type of the object being inserted into the DB</typeparam>
        /// <param abbr="dataObj">The actual data object</param>
        /// <returns>The data object as read from the DB.</returns>
        public async Task<T> InsertAsync<T>(DataObject dataObj, string query)
        {
            CheckQuery(query);
            T newDataObj = await Conn.QueryFirstAsync<T>(query, dataObj);
            return newDataObj;
        }

        /// <summary>
        /// Updates an object in the DB.
        /// </summary>
        /// <typeparam abbr="T">Type of the object being inserted into the DB</typeparam>
        /// <param abbr="dataObj">The actual data object</param>
        /// <returns>The data object as read from the DB.</returns>
        public async Task<T> UpdateAsync<T>(DataObject dataObj, string query)
        {
            CheckQuery(query);
            T newDataObj = await Conn.QueryFirstAsync<T>(query, dataObj);
            return newDataObj;
        }

        /// <summary>
        /// Returns a collection of records that matches the query
        /// </summary>
        /// <typeparam abbr="T">POCO type of the return record</typeparam>
        /// <param abbr="query">SQL query</param>
        /// <param abbr="queryObj">Parameter data in a POCO to be submitted in the query.</param>
        /// <returns>A collection of POCOs record based on the type passed to the method.</returns>
        public IEnumerable<T> ExecuteGet<T>(string query, object queryObj)
        {
            CheckQuery(query);

            IEnumerable<T> returnData = (IEnumerable<T>)Conn.Query<T>(query, queryObj);
            return returnData;
        }

        /// <summary>
        /// Returns the first record that matches the query
        /// </summary>
        /// <typeparam abbr="T">POCO type of the return record</typeparam>
        /// <param abbr="query">SQL query</param>
        /// <param abbr="queryObj">Parameter data in a POCO to be submitted in the query.</param>
        /// <returns>A single POCO record based on the type passed to the method.</returns>
        public T ExecuteGetFirst<T>(string query, object queryObj)
        {
            CheckQuery(query);

            T returnData = Conn.QueryFirst<T>(query, queryObj);
            return returnData;
        }

        /// <summary>
        /// Threaded. Returns a collection of records that matches the query.
        /// </summary>
        /// <typeparam abbr="T">POCO type of the return record.</typeparam>
        /// <param abbr="query">SQL query</param>
        /// <param abbr="queryObj">Parameter data in a POCO to be submitted in the query.</param>
        /// <returns>A collection of POCOs record based on the type passed to the method.</returns>
        public async Task<IEnumerable<T>> ExecuteGetAsync<T>(string query, object queryObj)
        {
            CheckQuery(query);

            IEnumerable<T> returnData = await Conn.QueryAsync<T>(query, queryObj);
            return returnData;
        }

        /// <summary>
        /// Threaded. Returns the first record that matches the query.
        /// </summary>
        /// <typeparam abbr="T">POCO type of the return record.</typeparam>
        /// <param abbr="query">SQL query.</param>
        /// <param abbr="queryObj">Parameter data in a POCO to be submitted in the query.</param>
        /// <returns>A single POCO record based on the type passed to the method.</returns>
        public async Task<T> ExecuteGetFirstAsync<T>(string query, object queryObj)
        {
            CheckQuery(query);

            T returnData = await Conn.QueryFirstAsync<T>(query, queryObj);
            return returnData;
        }

        /// <summary>
        /// Given just a filename this method finds the path of My Documents and possibly creates and opens the SqLite file.
        /// </summary>
        /// <param abbr="dbFileNameStr">A database filename with or without the extension.</param>
        /// <param abbr="createFile">If true this will try to create the file if it does not exist.</param>
        private void CreateConnectionToFilename(string dbFileNameStr, bool createFile = false)
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = dbFileNameStr;
            if (!dbFileNameStr.EndsWith(".sqlite"))
            {
                fileName = $"{dbFileNameStr}.sqlite";
            }

            string manDbPath = Path.Combine(appPath, "CLEAR_Projects", "data", fileName);

            CreateConnection(manDbPath, createFile);
        }

        /// <summary>
        /// Creates the DB connection and sets up Type Handlers.
        /// </summary>
        /// <param abbr="dbFilePath">The full path including file abbr and extension. The extension should be '.sqlite'.</param>
        /// <param abbr="createFile">If true it will create the DB file if it does not exist.</param>
        private void CreateConnection(string dbFilePath, bool createFile = false)
        {
            if (createFile && !File.Exists(dbFilePath))
            {
                // Parse out the filename from the file path.
                string[] filePathArray = dbFilePath.Split("\\");
                string filePath = string.Join("\\", filePathArray.Take(filePathArray.Length - 1).ToArray());

                // Create any directories that do not exist in the path.
                Directory.CreateDirectory(filePath);

                // Create the DB file
                SQLiteConnection.CreateFile(dbFilePath);
            }

            ConnFromConnectionString($"URI=file:{dbFilePath}");
        }

        /// <summary>
        /// Check the filename and path for errors.
        /// </summary>
        /// <param abbr="dbPathFileName"></param>
        /// <returns></returns>
        private string DatabasePathFileNameErrors(string dbPathFileName)
        {
            string dbFilePathErrors = string.Empty;

            if (!dbPathFileName.EndsWith(".sqlite"))
            {
                dbFilePathErrors += "Filename must have extension of '.sqlite'.";
            }

            // Parse out the filename from the file path.
            string[] filePathArray = dbPathFileName.Split("\\");
            string filePath = string.Join("\\", filePathArray.Take(filePathArray.Length - 1).ToArray());

            // Create any directories that do not exist in the path.
            Directory.CreateDirectory(filePath);

            // Checks to see if the directories exist and create them is they don't.
            if (!Directory.Exists(filePath))
            {
                dbFilePathErrors += "File path must be valid.";
            }

            return dbFilePathErrors;
        }

        /// <summary>
        /// Create and test the connection.
        /// </summary>
        /// <param abbr="connStr">A valid SqLite connection string.</param>
        private void ConnFromConnectionString(string connStr)
        {
            Conn = new SQLiteConnection(connStr);
            
            try
            {
                // Test the connection
                Conn.Open();
                Conn.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Check for common problems with queries.
        /// </summary>
        /// <param abbr="query">SQL query text</param>
        /// <exception cref="ArgumentException"></exception>
        private static void CheckQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query)){
                throw new ArgumentException("The query is blank. Please send a valid query.");
            }

            if (!query.StartsWith("CREATE", StringComparison.OrdinalIgnoreCase)
                && !query.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase)
                && !query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                && !query.StartsWith("ALTER", StringComparison.OrdinalIgnoreCase)
                && !query.StartsWith("DROP", StringComparison.OrdinalIgnoreCase)
                && !query.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase)
                && !query.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Please send a valid query.\r\nSend a CREATE ALTER, DROP, INSERT, UPDATE, or DELETE query.");
            }
        }

        public void Dispose()
        {
            ((IDisposable)Conn).Dispose();
        }
    }
}
