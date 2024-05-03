using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using Xunit;

namespace ClearDashboard.WPF.Tests
{
    public class TokenSplittingTokenMapTests
    {

        [Fact]
        public void Test()
        {
            var tokens = new List<Token>
            {
                new(new TokenId(1, 1, 1, 1, 1), "m", "m"),
                new(new TokenId(1, 1, 1, 1, 2), "p", "p"),
                new(new TokenId(1, 1, 1, 1, 3), "u", "u"),
                new(new TokenId(1, 1, 1, 1, 4), "t", "t"),
                new(new TokenId(1, 1, 1, 1, 5), "u", "u"),
                new(new TokenId(1, 1, 1, 1, 6), "g", "g"),
                new(new TokenId(1, 1, 1, 1, 7), "h", "h"),
                new(new TokenId(1, 1, 1, 1, 8), "u", "u"),
                new(new TokenId(1, 1, 1, 1, 9), "p", "p")

            };
            var tokenCollection = new TokenCollection(tokens);

           var tokenMap = new TokenMap(tokens, new TokenizedTextCorpus())
        }

    }
}
