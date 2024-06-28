using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WPF.Tests
{

    public class GrammarType
    {
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }
    public  class GenerateInsertGrammarTypesSQL
    {
        public ITestOutputHelper Output { get; private set; }

        public GenerateInsertGrammarTypesSQL(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void GenerateSQL()
        {
            var path = @"Data\GrammarTypes.json";
            string json = File.ReadAllText(path);
            var grammarList = JsonSerializer.Deserialize<List<GrammarType>>(json);

            foreach (var grammarType in grammarList)
            {
                var sql = $"migrationBuilder.Sql(\"INSERT INTO Grammar (Id, ShortName, Description) VALUES ('{Guid.NewGuid()}','{grammarType.ShortName}', '{grammarType.Description}');\");";
                Output.WriteLine(sql);

            }
        }
    }
}
