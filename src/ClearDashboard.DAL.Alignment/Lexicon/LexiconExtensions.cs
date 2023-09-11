using ClearDashboard.DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public static class LexiconExtensions
    {
        /// <summary>
        /// Returns Lexemes in first that do not appear in second, based on the
        /// following criteria:
        /// - a matching lemma and/or a form of a lexeme (lemma-or-form-text + lexeme language + lexeme type) AND 
        /// - a matching translation for the same lexeme (translation text + translation language)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<Lexeme> ExceptByLexemeTranslationMatch(this IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var lexemeIdsInFirstHavingMatchInSecond = FindLexemeIdsInFirstHavingMatchInSecond(first, second);
            return first.ExceptBy(lexemeIdsInFirstHavingMatchInSecond, e => e.LexemeId.Id);
        }

        /// <summary>
        /// Returns LexemeIds for Lexemes in first that match at least one Lexeme in second, based on the
        /// following criteria:
        /// - a matching lemma and/or a form of a lexeme (lemma-or-form-text + lexeme language + lexeme type) AND 
        /// - a matching translation for the same lexeme (translation text + translation language)
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<LexemeId> IntersectIdsByLexemeTranslationMatch(this IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var lexemeIdsInFirstHavingMatchInSecond = FindLexemeIdsInFirstHavingMatchInSecond(first, second);
            return first
                .IntersectBy(lexemeIdsInFirstHavingMatchInSecond, e => e.LexemeId.Id)
                .Select(e => e.LexemeId);
        }

        private static IEnumerable<Guid> FindLexemeIdsInFirstHavingMatchInSecond(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var firstLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(first);
            var secondLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(second);

            var firstTranslationMatchValuesByLexemeId = FindTranslationMatchValuesByLexemeId(first);
            var secondTranslationMatchValuesByLexemeId = FindTranslationMatchValuesByLexemeId(second);

            var lexemeIdsInFirstHavingMatchInSecond = new List<Guid>();

            // Intersection results in 'lexeme match' values found in both first and
            // second.  For each of these, loop through all combinations of 'first'
            // and 'second' lexeme ids and for any 'translation match' values in both,
            // add to 'firstLexemeIdsHavingMatchInSecond' list: 
            foreach (var lexemeMatchValueInBoth in firstLexemeIdsByLexemeMatchValue.Keys
                .Intersect(secondLexemeIdsByLexemeMatchValue.Keys))
            {
                foreach (var firstLexemeId in firstLexemeIdsByLexemeMatchValue[lexemeMatchValueInBoth]
                    .Where(firstTranslationMatchValuesByLexemeId.ContainsKey))
                {
                    foreach (var secondLexemeId in secondLexemeIdsByLexemeMatchValue[lexemeMatchValueInBoth]
                        .Where(secondTranslationMatchValuesByLexemeId.ContainsKey))
                    {
                        if (firstTranslationMatchValuesByLexemeId[firstLexemeId]
                            .Intersect(secondTranslationMatchValuesByLexemeId[secondLexemeId]).Any())
                        {
                            lexemeIdsInFirstHavingMatchInSecond.Add(firstLexemeId);
                        }
                    }
                }
            }

            return lexemeIdsInFirstHavingMatchInSecond;
        }

        private static Dictionary<(string LemmaOrFormText, string? Language, string? Type), IEnumerable<Guid>> FindLexemeIdsByLexemeMatchValue(IEnumerable<Lexeme> lexemes)
        {
            return lexemes
                .SelectMany(e => e.LemmaPlusFormTexts
                    .Select(l => (LemmaOrFormText: l, e.Language, e.Type, LexemeId: e.LexemeId.Id)))
                .GroupBy(e => (e.LemmaOrFormText, e.Language, e.Type))
                .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.LexemeId));
        }

        private static Dictionary<Guid, IEnumerable<(string? TranslationText, string? Language)>> FindTranslationMatchValuesByLexemeId(IEnumerable<Lexeme> lexemes)
        {
            return lexemes
                .SelectMany(e => e.Meanings.SelectMany(m => m.Translations.Select(t => (TranslationText: t.Text, m.Language, LexemeId: e.LexemeId.Id))))
                .GroupBy(e => e.LexemeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => (e.TranslationText, e.Language)));
        }
    }
}
