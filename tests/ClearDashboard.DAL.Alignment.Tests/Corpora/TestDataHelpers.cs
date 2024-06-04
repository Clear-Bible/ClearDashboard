using ClearBible.Engine.Corpora;
using ClearBible.Engine.SyntaxTree.Corpora;
using ClearBible.Engine.Tokenization;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static ITextCorpus GetZZSurCorpus(IEnumerable<string>? bookIds = null)
        {
            if (bookIds != null && bookIds.Any())
            {
				return new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
					.FilterTexts(bookIds)
					.Tokenize<LatinWordTokenizer>()
					.Transform<IntoTokensTextRowProcessor>();
			}
			else
            {
                return new ParatextTextCorpus("C:\\My Paratext 9 Projects\\zz_SUR")
                    .Tokenize<LatinWordTokenizer>()
                    .Transform<IntoTokensTextRowProcessor>();
            }
        }

		public static ITextCorpus FilterTexts(this ScriptureTextCorpus corpus, IEnumerable<string> bookIds)
		{
			return new ScriptureTextCorpus(corpus.Versification, corpus.Texts.Where(e => bookIds.Contains(e.Id)));
		}

		public static SourceTextIdToVerseMappings GetSampleTextCorpusSourceTextIdToVerseMappings(ScrVers sourceVersification, ScrVers targetVersification)
        {
            var verseMappings = EngineParallelTextCorpus.VerseMappingsForAllVerses(
                sourceVersification,
                targetVersification);

            List<VerseMapping> verseMappingsFiltered = verseMappings
                .Where(verseMapping => verseMapping.SourceVerses
                    .Where(verse => verse.Book == "MAT")
                    .Where(verse => verse.ChapterNum == 1)
                    .Where(verse => verse.VerseNum <= 5)
                    .Any())
                .ToList();

            verseMappingsFiltered.AddRange(verseMappings
                .Where(verseMapping => verseMapping.SourceVerses
                    .Where(verse => verse.Book == "MAT")
                    .Where(verse => verse.ChapterNum == 2)
                    .Where(verse => verse.VerseNum <= 7)
                    .Any())
                .ToList());

            verseMappingsFiltered.AddRange(verseMappings
                .Where(verseMapping => verseMapping.SourceVerses
                    .Where(verse => verse.Book == "MRK")
                    .Where(verse => verse.ChapterNum == 1)
                    .Where(verse => verse.VerseNum <= 4)
                    .Any())
                .ToList());

            return new SourceTextIdToVerseMappingsFromVerseMappings(verseMappingsFiltered);
        }

		public interface IRowFilter<T> where T : TextRow
		{
			bool ShouldIncludeRow(T textRow);
		}

		public interface ITextFilter<T> where T : IText
		{
			bool ShouldIncludeText(T text);
		}
	}
}