using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClearDashboard.DataAccessLayer.Dapper;

namespace ClearDashboard.DataAccessLayer.Repositories
{
    /// <summary>
    /// This parent class should be inherited by any class that you use to hold a list of data from the DB.
    /// 
    /// To write the SQL and create tables I use the freeware https://sqlitebrowser.org/
    /// 
    /// This parent class implements INotifyPropertyChanged, so that classes inheriting this class don't have to. When you create properties
    /// in your class that need to trigger a binding to a UI control, make sure to use the full property and on the setter call OnPropertyChanged();
    /// </summary>
    public abstract class DapperRepository : INotifyPropertyChanged
    {
        public DapperRepository()
        {

        }

        internal string? DatabaseFilePathName { get; set; }

     //   internal DapperDataProvider Db { get; set; }

        /// <summary>
        /// Allows for easy change of the table abbr. We want developers to be able to save objects in different tables.
        /// </summary>
        public string? TableName { get; set; }

        /// <summary>
        /// Get all the objects.
        /// Used by other methods, so please do not have a semicolon ; at the end of the statement.
        /// </summary>
        /// <returns></returns>
        public virtual string SelectQuery => $@"SELECT * FROM {TableName}";

        /// <summary>
        /// The insert query should CREATE the record if the record is not found in the DB.
        /// </summary>
        /// <returns>The SQL select query text.</returns>
        public abstract string InsertQuery();

        /// <summary>
        /// The standard update query goes here. The data object will be used as the data sent to the DB.
        /// The query text should also select the last record created.
        /// </summary>
        /// <returns>The SQL update query text.</returns>
        public abstract string UpdateQuery();

        /// <summary>
        /// The standard delete query goes here. The data object will be used as the data sent to the DB. 
        /// </summary>
        /// <returns>The SQL delete by Id query text.</returns>
        public virtual string DeleteByIdQuery(int id)
        {
            return $@"DELETE FROM {TableName} WHERE Id = @Id";
        }

        /// <summary>
        /// Default select by Id.
        /// </summary>
        public virtual string SelectQueryById => string.Concat(SelectQuery, " ", @"WHERE Id = @Id LIMIT 1;");

        /// <summary>
        /// This SQL command will drop the table named in TableName if it exists.
        /// </summary>
        public virtual string DropCmdQuery => $@"DROP TABLE IF EXISTS {TableName}; ";

        /// <summary>
        /// Basic create table command. It needs to be overridden with a complete table creation command. 
        /// </summary>
        public virtual string CreateTableCmdQuery => $@"CREATE TABLE IF NOT EXISTS ""{TableName}"" 
                    (""Id"" INTEGER NOT NULL UNIQUE,
	                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                    )";

        /// <summary>
        /// Triggers an update of the UI when the data changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
