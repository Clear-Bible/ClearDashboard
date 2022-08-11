using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.ParatextPlugin.CQRS.Features.BiblicalTerms;
using ClearDashboard.ParatextPlugin.CQRS.Features.BookUsfm;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.WebApiParatextPlugin.Tests
{
    public class GetUsfmBookByParatextIdBookIdQueryHandlerTest : TestBase
    {
        public GetUsfmBookByParatextIdBookIdQueryHandlerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GetUsfmBookByParatextIdBookIdTest()
        {
            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetRowsByParatextProjectIdAndBookIdQuery>(
                    "bookusfmbyparatextidbookid",
                    new GetRowsByParatextProjectIdAndBookIdQuery("3f0f2b0426e1457e8e496834aaa30fce00000002abcdefff", "GEN"));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<UsfmVerse>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

            }
            finally
            {
                await StopParatextAsync();
            }
        }


        [Fact]
        public async Task GetUsfmBookByParatextIdBookId_zzSUR_Test()
        {
            List<UsfmVerse>? verses;

            try
            {
                await StartParatextAsync();
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/api/");

                var response = await client.PostAsJsonAsync<GetRowsByParatextProjectIdAndBookIdQuery>(
                    "bookusfmbyparatextidbookid",
                    new GetRowsByParatextProjectIdAndBookIdQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", "GEN"));

                Assert.True(response.IsSuccessStatusCode);
                var result = await response.Content.ReadAsAsync<RequestResult<List<UsfmVerse>>>();

                Assert.NotNull(result);
                Assert.True(result.Success);
                Assert.NotNull(result.Data);

                verses = result.Data;
            }
            finally
            {
                await StopParatextAsync();
            }

            // test for the same here
            var exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData");
            var verseTextFile = Path.Combine(exportPath, "VerseText", "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f", "01GEN01.sfm");

            var verseLines = GetFileData(verseTextFile);

            // trim down the verses only to chapter 1
            var incomming = verses.Where(x => x.Chapter == "1").ToList();
            incomming.OrderBy(x => x.Verse);

            foreach (var verseLine in verseLines)
            {
                if (verseLine.StartsWith(@"\v"))
                {
                    //get the verse number
                    int index = verseLine.IndexOf(' ', verseLine.IndexOf(' ') + 1);
                    string verseNum = verseLine.Substring(2, index - 2).Trim();

                    // get the corresponding verse from what we read in
                    var verse = incomming.Where(v => v.Verse == verseNum).ToList();

                    string correctVerse = verseLine.Substring(index).Trim();

                    //Debug.WriteLine($"{verseNum} {correctVerse}");
                    //Debug.WriteLine($"{verseNum} {verse.First().Text.Trim()}");
                    Assert.Equal(correctVerse, verse.First().Text.Trim());
                }
            }

        }

        private List<string> GetFileData(string verseTextFile)
        {
            List<string> lines = new List<string>();
            using (StreamReader r = new StreamReader(verseTextFile))
            {
                string? line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}
