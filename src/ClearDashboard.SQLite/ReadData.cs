using ClearDashboard.Common.Models;
using System.Data;
using System.Data.SQLite;

namespace ClearDashboard.SQLite
{
    public class ReadData
    {
        public SQLiteConnection Conn;
        public ReadData(SQLiteConnection conn)
        {
            Conn = conn;
        }

        /// <summary>
        /// Gets all the verses for a chapter
        /// </summary>
        /// <param name="verseID"></param>
        /// <returns></returns>
        public List<CoupleOfStrings> GetSourceChapterText(string verseID)
        {
            SQLiteDataReader sqliteDatareader;
            SQLiteCommand sqliteCmd;
            sqliteCmd = Conn.CreateCommand();
            sqliteCmd.CommandType = CommandType.Text;

            string sql = "SELECT verseID, verseText FROM verses WHERE verseID LIKE '"
                         + verseID.Substring(0, 5) + "%' ORDER BY verseID";

            sqliteCmd.CommandText = sql;

            sqliteDatareader = sqliteCmd.ExecuteReader();

            List<CoupleOfStrings> list = new List<CoupleOfStrings>();
            string verseText = "";
            while (sqliteDatareader.Read())
            {
                list.Add(new CoupleOfStrings
                {
                    stringA = sqliteDatareader.GetString(0),
                    stringB = sqliteDatareader.GetString(1)
                });
            }

            return list;
        }

    }
}
