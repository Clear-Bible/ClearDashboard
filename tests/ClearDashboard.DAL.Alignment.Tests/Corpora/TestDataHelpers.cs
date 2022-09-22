using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora
{
    internal static class TestDataHelpers
    {
        public static readonly string TestDataPath = Path.Combine(AppContext.BaseDirectory, "Corpora", "data");

        public static readonly string UsfmTestProjectPath = Path.Combine(TestDataPath, "usfm", "Tes");

        public static readonly string GreekNTUsfmTestProjectPath =
            Path.Combine(TestDataPath, "usfm", "nestle1904");

        public static readonly string GreekNTUsfmFullTestProjectPath =
            Path.Combine(TestDataPath, "usfm", "nestle1904Full");


        public static ITextCorpus GetSampleTextCorpus()
        {
            return new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, UsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();
        }

        public static ITextCorpus GetSampleGreekCorpus()
        {
            return new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, GreekNTUsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();
        }

        public static ITextCorpus GetFullGreekNTCorpus()
        {
            return new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, GreekNTUsfmFullTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();
        }

        public static ITextCorpus GetManuscript()
        {
            var syntaxTree = new SyntaxTrees();
            return new SyntaxTreeFileTextCorpus(syntaxTree);
        }

        public static ITextCorpus GetZZSurCorpus()
        {
            return new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
                 .Tokenize<LatinWordTokenizer>()
                 .Transform<IntoTokensTextRowProcessor>();
        }
    }
}