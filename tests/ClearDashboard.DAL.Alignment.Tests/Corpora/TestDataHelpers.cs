using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.IO;
using System.Text;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    internal static class TestDataHelpers
    {
        public static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "Corpora", "data");

        public static readonly string UsfmTestProjectPath = Path.Combine(TestDataPath, "usfm", "Tes");

        public static readonly string GreekNTUsfmTestProjectPath =
            Path.Combine(TestDataPath, "usfm", "nestle1904");

        public static ITextCorpus GetSampleTextCorpus()
        {
            return new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, UsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();
        }
    }
}