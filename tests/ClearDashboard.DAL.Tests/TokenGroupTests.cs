using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class TokenGroupTests
    {
        private readonly ITestOutputHelper _output;

        public TokenGroupTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GroupingTest()
        {
            var list = new List<Token>
            {
                new Token { ChapterNumber = 1, VerseNumber = 1 },
                new Token { ChapterNumber = 1, VerseNumber = 1 },
                new Token { ChapterNumber = 1, VerseNumber = 1 },
                new Token { ChapterNumber = 1, VerseNumber = 2 },
                new Token { ChapterNumber = 1, VerseNumber = 2 },
                new Token { ChapterNumber = 2, VerseNumber = 10 },
                new Token { ChapterNumber = 2, VerseNumber = 10 },
                new Token { ChapterNumber = 2, VerseNumber = 10 },
                new Token { ChapterNumber = 2, VerseNumber = 10 },
                new Token { ChapterNumber = 2, VerseNumber = 11 }
            };

            var groupedTokens = list.GroupBy(token => new { token.ChapterNumber, token.VerseNumber }, token => token);

            foreach (var groupToken in groupedTokens)
            {
                _output.WriteLine($"{groupToken.Key.ChapterNumber},{groupToken.Key.VerseNumber}");
                var tokens = groupToken.ToList();

                _output.WriteLine($"Number of Tokens - {tokens.Count}");
            }
        }
    }
}
