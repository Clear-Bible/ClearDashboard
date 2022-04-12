using System.Data.SQLite;
using System.Diagnostics;

namespace ClearDashboard.SQLite
{
    public class Connection
    {
        public SQLiteConnection Conn;

        public Connection(string sDBpath)
        {
            // Create a new database connection:
            Conn = new SQLiteConnection("URI=file:" + sDBpath + ";");
            // Open the connection:
            try
            {
                Conn.Open();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public void CloseConnection()
        {
            Conn.Close();
        }
    }
}