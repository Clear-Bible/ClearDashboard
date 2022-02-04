using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ClearDashboard.Common;
using ClearDashboard.DAL.Models;

namespace ClearDashboard.DAL.Repositories
{
    public class NoteTagListRepository : DapperRepository
    {
        internal Models.NoteTagListModel NoteTagContainer { get; set; }

        public NoteTagListRepository(ref Models.NoteTagListModel noteTagListModel, string databaseFilePathName, string tableName = "NotesAndTags")
        {
            DatabaseFilePathName = databaseFilePathName;
            NoteTagContainer = noteTagListModel;
            TableName = tableName;

            Db = new(DatabaseFilePathName, true);
            CreateTableIfNotExists();
        }

        /// <summary>
        /// Loads the NotesAndTags list from the database.
        /// </summary>
        /// <returns>A count of records</returns>
        public int LoadFromDb()
        {
            NoteTagContainer.NotesAndTags = new ObservableCollection<Models.NoteTagModel>(Db.ExecuteGetAsync<Models.NoteTagModel>(SelectQuery, null).Result);
            return NoteTagContainer.NotesAndTags.Count;
        }

        public int LoadFromDbByVerse(string verseFullId, Models.ENums.NoteTagType type)
        {
            NoteTagContainer.NotesAndTags = new ObservableCollection<Models.NoteTagModel>(Db.ExecuteGetAsync<Models.NoteTagModel>(SelectQueryByVerse(verseFullId, type), null).Result);

            return NoteTagContainer.NotesAndTags.Count;
        }

        /// <summary>
        /// Loads the NotesAndTags list from the database.
        /// </summary>
        /// <returns>A count of records</returns>
        public int LoadNotesListFromDb()
        {
            NoteTagContainer.NotesAndTags = new ObservableCollection<Models.NoteTagModel>(Db.ExecuteGetAsync<Models.NoteTagModel>(SelectQueryByType(Models.ENums.NoteTagType.Note), null).Result);
            return NoteTagContainer.NotesAndTags.Count;
        }

        public int LoadTagsListFromDb()
        {
            NoteTagContainer.NotesAndTags = new ObservableCollection<Models.NoteTagModel>(Db.ExecuteGetAsync<Models.NoteTagModel>(SelectQueryByType(Models.ENums.NoteTagType.Tag), null).Result);
            return NoteTagContainer.NotesAndTags.Count;
        }

        public void CreateTableIfNotExists()
        {
            Db.ExecuteQuery(CreateTableCmdQuery, null);
        }

        public void InsertAllNotesAndTags()
        {
            // Insert each object into the db.
            NoteTagContainer.NotesAndTags.ToList().ForEach(note =>
            {
                note = InsertNoteOrTag(note);
            });
        }

        /// <summary>
        /// Inserts the note or tag into the database and loads the object from the database which captures any changes.
        /// </summary>
        /// <param abbr="noteTag">The object from the DB with any DB updates like the Id.</param>
        /// <returns></returns>
        public Models.NoteTagModel InsertNoteOrTag(Models.NoteTagModel noteTag, bool AddToCollection = false)
        {
            if (AddToCollection)
            {
                NoteTagContainer.NotesAndTags.Add(noteTag);
            }

            return Db.Insert<Models.NoteTagModel>(noteTag, InsertQuery());
        }

        public void InsertOrUpdateAllNotesAndTags()
        {
            // Insert each object into the db.
            NoteTagContainer.NotesAndTags.ToList().ForEach(note =>
            {
                note = InsertOrUpdateNoteOrTag(note);
            });
        }

        /// <summary>
        /// Based on the default value of Id, this either inserts the object or updates it in the db.
        /// </summary>
        /// <param abbr="noteTag"></param>
        /// <returns>The object from the db after the insert or update.</returns>
        public Models.NoteTagModel InsertOrUpdateNoteOrTag(Models.NoteTagModel noteTag)
        {
            // The default value for the Id field is -1. If that is the current value, the object needs to be inserted into the db.
            if (noteTag.Id == -1)
            {
                return Db.Insert<Models.NoteTagModel>(noteTag, InsertQuery());
            }

            return Db.UpdateAsync<Models.NoteTagModel>(noteTag, UpdateQuery()).Result;
        }

        public int DeleteNoteOrTag(Models.NoteTagModel noteTag)
        {
            return Db.DeleteAsync(noteTag, DeleteByIdQuery(noteTag.Id)).Result;
        }

        /// <summary>
        /// Get all the objects.
        /// Used by other methods, so please do not have a semicolon ; at the end of the statement.
        /// </summary>
        /// <returns>All the objects in the table.</returns>
        public override string SelectQuery => $@"SELECT Id, NoteOrTag, Title, Text, English, AltText, VerseReferences, LastChanged FROM {TableName}";
        // SQL select query is used by other methods, so please do not have a semicolon ; at the end of the statement.

        public string SelectQueryByType(Models.ENums.NoteTagType type)
        {
            int typeInt = (int)type;
            return string.Concat(SelectQuery, " ", $@"WHERE NoteOrTag = {typeInt};");
        }

        public override string SelectQueryById => string.Concat(SelectQuery, " ", @"WHERE Id = @Id LIMIT 1;");

        public string SelectQueryByVerse(string VerseRef, Models.ENums.NoteTagType type)
        {
            int typeInt = (int)type;
            return string.Concat(SelectQuery, " ", $@"WHERE NoteOrTag = {typeInt} AND instr(VerseReferences, '{VerseRef}') > 0;");
        }

        public override string InsertQuery()
        {
            return string.Concat($@"INSERT INTO {TableName}(NoteOrTag, Title, Text, English, AltText, VerseReferences, LastChanged) VALUES(@NoteOrTag, @Title, @Text, @English, @AltText, @VerseReferences, @LastChanged);"
                , " ", SelectQuery, " ", "ORDER BY Id DESC LIMIT 1;");
        }

        public override string UpdateQuery()
        {
            return string.Concat($@"UPDATE {TableName} SET NoteOrTag = @NoteOrTag, Title = @Title, Text = @Text, English = @English, AltText = @AltText, VerseReferences = @VerseReferences, LastChanged = @LastChanged WHERE Id = @Id;"
                , " ", SelectQueryById);
        }

        public bool DeleteObject(DataObject obj)
        {
            string deleteQuery = base.DeleteByIdQuery(obj.Id);
            int recordsChanged = Db.DeleteAsync(obj, deleteQuery).Result;
            NoteTagContainer.NotesAndTags.Remove((Models.NoteTagModel)obj);
            
            // Only one record should be changed.
            return recordsChanged.Equals(1);
        }

        public override string DropCmdQuery => $@"DROP TABLE IF EXISTS {TableName}; ";

        public override string CreateTableCmdQuery => $@"CREATE TABLE IF NOT EXISTS ""{TableName}"" 
                    (""Id"" INTEGER NOT NULL UNIQUE,
                        ""NoteOrTag"" TEXT,
	                    ""Title"" TEXT,
                        ""Text"" TEXT,
	                    ""English""   TEXT,
	                    ""AltText""   TEXT,
	                    ""VerseReferences""   TEXT,
                        ""LastChanged""	TEXT,
	                    PRIMARY KEY(""ID"" AUTOINCREMENT)
                    )";
    }
}