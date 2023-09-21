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
        /// <returns>
        /// Lexemes:  Lexemes in first that do not appear in second <br/>
        /// LemmaOrFormMatchesOnly:  Lexeme Ids from Lexemes that only match on LemmaOrForm <br/>
        /// TranslationMatchesOnly:  Lexeme Ids from Lexemes that only match on Translation
        /// </returns>
        public static (IEnumerable<Lexeme> Lexemes, IEnumerable<Guid> LemmaOrFormMatchesOnly, IEnumerable<Guid> TranslationMatchesOnly) ExceptByLexemeTranslationMatch(this IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var matchIds = FindLexemeIdsInFirstHavingMatchInSecond(first, second);
            var translationMatchIds = FindLexemeIdsInFirstHavingTranslationMatchInSecond(first, second);

            var lexemesWithoutFullMatch = first.ExceptBy(matchIds.FullMatchIds, e => e.LexemeId.Id);
            return (
                lexemesWithoutFullMatch,
                matchIds.LemmaOrFormMatchIds.Except(matchIds.FullMatchIds),
                translationMatchIds.Except(matchIds.FullMatchIds)
            );
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
            var matchIds = FindLexemeIdsInFirstHavingMatchInSecond(first, second);
            return first
                .IntersectBy(matchIds.FullMatchIds, e => e.LexemeId.Id)
                .Select(e => e.LexemeId);
        }

        private static (IEnumerable<Guid> FullMatchIds, IEnumerable<Guid> LemmaOrFormMatchIds) FindLexemeIdsInFirstHavingMatchInSecond(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var firstLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(first);
            var secondLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(second);

            var firstTranslationMatchValuesByLexemeId = FindTranslationMatchValuesByLexemeId(first);
            var secondTranslationMatchValuesByLexemeId = FindTranslationMatchValuesByLexemeId(second);

            var fullMatchIds = new List<Guid>();
            var lemmaOrFormMatchIds = new List<Guid>();

            // Intersection results in 'lexeme match' values found in both first and
            // second.  For each of these, loop through all combinations of 'first'
            // and 'second' lexeme ids and for any 'translation match' values in both,
            // add to 'firstLexemeIdsHavingMatchInSecond' list: 
            foreach (var lexemeMatchValueInBoth in firstLexemeIdsByLexemeMatchValue.Keys
                .Intersect(secondLexemeIdsByLexemeMatchValue.Keys))
            {
                foreach (var firstLexemeId in firstLexemeIdsByLexemeMatchValue[lexemeMatchValueInBoth])
                {
                    lemmaOrFormMatchIds.Add(firstLexemeId);

                    if (firstTranslationMatchValuesByLexemeId.ContainsKey(firstLexemeId))
                    {
                        foreach (var secondLexemeId in secondLexemeIdsByLexemeMatchValue[lexemeMatchValueInBoth])
                        {
                            if (secondTranslationMatchValuesByLexemeId.ContainsKey(secondLexemeId))
                            {
                                if (firstTranslationMatchValuesByLexemeId[firstLexemeId]
                                    .Intersect(secondTranslationMatchValuesByLexemeId[secondLexemeId]).Any())
                                {
                                    fullMatchIds.Add(firstLexemeId);
                                }
                            }
                        }
                    }
                }
            }

            return (fullMatchIds, lemmaOrFormMatchIds);
        }

        private static IEnumerable<Guid> FindLexemeIdsInFirstHavingTranslationMatchInSecond(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var firstLexemeIdsByTranslationMatchValue = FindLexemeIdsByTranslationMatchValue(first);
            var secondLexemeIdsByTranslationMatchValue = FindLexemeIdsByTranslationMatchValue(second);

            return firstLexemeIdsByTranslationMatchValue.Keys
                .Intersect(secondLexemeIdsByTranslationMatchValue.Keys)
                .SelectMany(e => firstLexemeIdsByTranslationMatchValue[e])
                .ToList();
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

        private static Dictionary<(string TranslationText, string? Language), IEnumerable<Guid>> FindLexemeIdsByTranslationMatchValue(IEnumerable<Lexeme> lexemes)
        {
            return lexemes
                .SelectMany(e => e.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Select(t => (TranslationText: t.Text!, m.Language, LexemeId: e.LexemeId.Id))))
                .GroupBy(e => (e.TranslationText, e.Language))
                .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.LexemeId));
        }

        private static Dictionary<Guid, IEnumerable<(string TranslationText, string? Language)>> FindTranslationMatchValuesByLexemeId(IEnumerable<Lexeme> lexemes)
        {
            return lexemes
                .SelectMany(e => e.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Select(t => (TranslationText: t.Text!, m.Language, LexemeId: e.LexemeId.Id))))
                .GroupBy(e => e.LexemeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => (e.TranslationText, e.Language)));
        }
    }
}
