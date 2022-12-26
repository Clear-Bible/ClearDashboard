using System.Data.SQLite;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
// ReSharper disable InconsistentNaming

namespace GenerateSemantiDomainLookupSqlite
{
    internal class Program
    {
        private static string _databaseFileName = string.Empty;
        private static string _connectionString => "URI=file:" + _databaseFileName;
        private static string _sourceDirectory = string.Empty;
        private static List<WordLookup> _wordLookups = new List<WordLookup>();


        private static void Main(string[] args)
        {
            // Create a new database
            _databaseFileName = Environment.CurrentDirectory + @"\SemanticDomainsLookup.sqlite";
            if (File.Exists(_databaseFileName))
            {
                File.Delete(_databaseFileName);
            }
            
            SQLiteConnection.CreateFile(_databaseFileName);
            
            // Create a new table
            CreateTable();
            
            // Parse SDBH/SDBG files
            ParseXmlDatabases();

            // Insert into database table
            InsertRecords();

            Console.WriteLine("Inserted {0} records into the database", _wordLookups.Count);
            Console.WriteLine("Do you wish to replace the existing SemanticDomainsLookup.sqlite file in the ClearDashboard project? (y/n)");
            string? input = Console.ReadLine();
            if (input != null && input.ToLowerInvariant() == "y")
            {
                string source = _databaseFileName;
                string destination = Path.Combine(_sourceDirectory, @"ClearDashboard.DAL\Resources\marble-concordances\SemanticDomainsLookup.sqlite");
                File.Copy(source, destination, true);
                Console.WriteLine("SemanticDomainsLookup.sqlite file replaced in the ClearDashboard project");
            }

        }

        private static void CreateTable()
        {
            using var con = new SQLiteConnection(_connectionString);
            con.Open();

            using var cmd = new SQLiteCommand(con);

            cmd.CommandText = "DROP TABLE IF EXISTS WordLookup";
            cmd.ExecuteNonQuery();

            cmd.CommandText = @"CREATE TABLE WordLookup(ID LARGEINT PRIMARY KEY,
            Original CHARACTER(50), Consonants CHARACTER(40), Gloss CHARACTER(256), IsHebrew BOOL)";
            cmd.ExecuteNonQuery();
        }

        private static void ParseXmlDatabases()
        {
            // Dictionary of approved Hebrew Consonants (ie. all the vowels and punctuation removed)
            Dictionary<string, string> hebrewConsonants = new();

            hebrewConsonants.Add("05C1", "");
            hebrewConsonants.Add("05C2", "");

            hebrewConsonants.Add("05D0", "");
            hebrewConsonants.Add("05D1", "");
            hebrewConsonants.Add("05D2", "");
            hebrewConsonants.Add("05D3", "");
            hebrewConsonants.Add("05D4", "");
            hebrewConsonants.Add("05D5", "");
            hebrewConsonants.Add("05D6", "");
            hebrewConsonants.Add("05D7", "");
            hebrewConsonants.Add("05D8", "");
            hebrewConsonants.Add("05D9", "");
            hebrewConsonants.Add("05DA", "");
            hebrewConsonants.Add("05DB", "");
            hebrewConsonants.Add("05DC", "");
            hebrewConsonants.Add("05DE", "");
            hebrewConsonants.Add("05DD", "");
            hebrewConsonants.Add("05DF", "");
            hebrewConsonants.Add("05E0", "");
            hebrewConsonants.Add("05E1", "");
            hebrewConsonants.Add("05E2", "");
            hebrewConsonants.Add("05E3", "");
            hebrewConsonants.Add("05E4", "");
            hebrewConsonants.Add("05E5", "");
            hebrewConsonants.Add("05E6", "");
            hebrewConsonants.Add("05E7", "");
            hebrewConsonants.Add("05E8", "");
            hebrewConsonants.Add("05E9", "");
            hebrewConsonants.Add("05EA", "");

            
            long index = 1;

            // loop between Hebrew/Gr databases
            for (int i = 0; i < 2; i++)
            {
                _sourceDirectory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\..\src"));
                var directoryPath = Path.Combine(_sourceDirectory, @"ClearDashboard.DAL\Resources");
                if (Directory.Exists(directoryPath) == false)
                {
                    Console.WriteLine("Cannot find resources at: " + directoryPath);
                    return;
                }

                if (i == 0)
                {
                    directoryPath = Path.Combine(directoryPath, @"SDBH");
                }
                else
                {
                    directoryPath = Path.Combine(directoryPath, @"SDBG");
                }

                var files = Directory.GetFiles(directoryPath, "*.xml");

                // pull out the source and gloss from all the *.xml data files
                foreach (var file in files)
                {
                    var xmlStr = File.ReadAllText(file);
                    var str = XElement.Parse(xmlStr);

                    // get a list of all the source words
                    var sourceWords = str.Elements("Lexicon_Entry")
                        .Select(x => x.Attribute("Lemma").Value)
                        .ToList();

                    // get the gloss list for each word
                    foreach (var word in sourceWords)
                    {
                        var lexMeanings = str.Elements("Lexicon_Entry")
                            .Where(x => x.Attribute("Lemma").Value == word)
                            .Elements("BaseForms")
                            .Elements("BaseForm")
                            .Elements("LEXMeanings")
                            .Elements("LEXMeaning")
                            .ToList();

                        // normalize the word using FormD so we can have a word
                        // that is easy to type
                        var normalizedWord = word.Normalize(NormalizationForm.FormD);
                        string vowelessSource = "";

                        if (i == 0)
                        {
                            // check for Hebrew letters within the range
                            foreach (var letter in normalizedWord)
                            {
                                var unicode = ((int)letter).ToString("X4");
                                if (hebrewConsonants.ContainsKey(unicode))
                                {
                                    vowelessSource += letter;
                                }
                            }
                        }
                        else
                        {
                            // check for Greek letters without diacritics
                            foreach (char letter in normalizedWord)
                            {
                                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                                {
                                    string s = letter.ToString().ToLower(new CultureInfo("el-GR"));
                                    vowelessSource += s;
                                }
                            }
                        }

                        // drill down to get all the glosses from each lexical meaning
                        foreach (var meaning in lexMeanings)
                        {
                            var totalVerseCount = meaning.Elements("LEXReferences")
                                .Elements("LEXReference")
                                .ToList();
                            var senses = meaning.Elements("LEXSenses").ToList();
                            var lexSenseEnglish = senses.Elements("LEXSense")
                                .FirstOrDefault(x => x.Attribute("LanguageCode").Value.Equals("en"));

                            List<string> glosses = new();
                            if (lexSenseEnglish is not null)
                            {
                                glosses = lexSenseEnglish.Elements("Glosses")
                                    .Elements("Gloss")
                                    .Select(x => x.Value)
                                    .Distinct()
                                    .ToList();
                            }

                            foreach (var gloss in glosses)
                            {
                                // remove commas since we are in CSV format
                                var commaLessGloss = gloss.Replace(",", ";");
                                if (commaLessGloss is null)
                                {
                                    commaLessGloss = "";
                                }

                                bool isHebrew = false;
                                if (i == 0)
                                {
                                    isHebrew = true;
                                }

                                //"ROW,ORIGINAL,NORMALIZED,CONSONANTS,GLOSS,ISHEBREW"
                                _wordLookups.Add(new WordLookup
                                {
                                    Id = index,
                                    Original = word,
                                    Consonants = vowelessSource, 
                                    Gloss = commaLessGloss, 
                                    IsHebrew = isHebrew
                                });

                                index++;
                            }
                        }
                    }

                }
            }
        }

        private static void InsertRecords()
        {
            var results = new List<int>();
            var sqlInsert = @"INSERT INTO [WordLookup] ([Id],[Original],[Consonants],[Gloss],[IsHebrew])"
                            + " VALUES (@id,@original, @consonants, @gloss, @isHebrew);";

            using var cn = new SQLiteConnection(_connectionString);
            cn.Open();
            using var transaction = cn.BeginTransaction();
            using (var cmd = cn.CreateCommand())
            {
                cmd.CommandText = sqlInsert;
                foreach (var user in _wordLookups)
                {
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    cmd.Parameters.AddWithValue("@original", user.Original);
                    cmd.Parameters.AddWithValue("@consonants", user.Consonants);
                    cmd.Parameters.AddWithValue("@gloss", user.Gloss);
                    cmd.Parameters.AddWithValue("@isHebrew", user.IsHebrew);
                    results.Add(cmd.ExecuteNonQuery());
                }
            }
            transaction.Commit();
        }
    }

    public class WordLookup
    {
        public long Id { get; set; }
        public string Original { get; set; }
        public string Consonants { get; set; }
        public string Gloss { get; set; }
        public bool IsHebrew { get; set; }
    }
}