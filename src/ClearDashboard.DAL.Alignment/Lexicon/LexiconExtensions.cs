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
        /// Merges incoming lexemes into 'second' by finding lexeme matches (@see RelateByLexemeMatchValue)
        /// and then:
        /// - Adding lexemes only in first
        /// - Adding lexemes only in second
        /// - For any lexeme found in both: forms, meanings and/or translations from first that are not in 
        /// second are merged into second and then second is added.  Note that value matching instead of
        /// id matching is used since the ids in first were likely generated randomly.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static IEnumerable<Lexeme> MergeIntoSecond(this IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var (mapping, onlyInFirst, onlyInSecond) = RelateByLexemeMatchValue(first, second);

            var firstByGuid = first.ToDictionary(e => e.LexemeId.Id, e => e);
            var secondByGuid = second.ToDictionary(e => e.LexemeId.Id, e => e);

            // Start off with lexemes from first having no 'lexeme match' in second and vice versa:
            var lexemeImportCandidates = firstByGuid
                .IntersectBy(onlyInFirst, e => e.Key)
                .Select(e => e.Value)
                .ToList();

            lexemeImportCandidates.AddRange(secondByGuid
                .IntersectBy(onlyInSecond, e => e.Key)
                .Select(e => e.Value)
            );

            foreach (var (FirstGuid, SecondGuid) in mapping)
            {
                // Add entities missing from the first to the second and put the results into lexemesToImport:
                var firstLexeme = firstByGuid[FirstGuid];
                var secondLexeme = secondByGuid[SecondGuid];

                // Forms:
                var firstFormsNotInSecondLexeme = firstLexeme.Forms
                    .Where(e => !secondLexeme.Forms.Select(f => f.Text).Contains(e.Text));

                if (firstFormsNotInSecondLexeme.Any())
                {
                    foreach (var form in firstFormsNotInSecondLexeme)
                    {
                        secondLexeme.Forms.Add(form);
                    }
                }

                // Meanings:
                var secondLexemeMeaningsByMatch = secondLexeme.Meanings
                    .ToDictionary(e => (e.Language, e.Text), e => e);

                foreach (var firstMeaning in firstLexeme.Meanings)
                {
                    if (secondLexemeMeaningsByMatch.TryGetValue((firstMeaning.Language, firstMeaning.Text), out var secondMeaning))
                    {
                        // Translations:
                        var firstTranslationsNotInSecondMeaning = firstMeaning.Translations
                            .Where(e => !secondMeaning.Translations.Select(t => t.Text).Contains(e.Text));

                        if (firstTranslationsNotInSecondMeaning.Any())
                        {
                            foreach (var translation in firstTranslationsNotInSecondMeaning)
                            {
                                secondMeaning.Translations.Add(translation);
                            }
                        }
                    }
                    else if (firstMeaning.Translations.Any())
                    {
                        secondLexeme.Meanings.Add(firstMeaning);
                    }
                }

                lexemeImportCandidates.Add(secondLexeme);
            }

            return lexemeImportCandidates;
        }

        public static IEnumerable<Guid> GetTranslationMatchTranslationIds(this IEnumerable<Lexeme> lexemeImportCandidates)
        {
            var incomingTranslationIds = new List<Guid>();

            // This is to allow the UI to designate 'import lexicon candidate' rows where the row's translation matches
            // any translation from the internal lexicon.  The thinking here is that maybe the user would want to add
            // the incoming translation's lexeme as a form to another lexeme that contains a matching translation:

            // UI needs to know which rows (translations) to put the "Add as form for ..." button (incoming translation ids).

            lexemeImportCandidates
                .SelectMany(l => l.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Select(t => (LexemeId: l.LexemeId.Id, TranslationId: t.TranslationId.Id, t.IsInDatabase, TranslationText: t.Text!, m.Language))))
                .GroupBy(e => (e.TranslationText, e.Language))
                .Select(g => g.Select(e => (e.LexemeId, e.TranslationId, e.IsInDatabase)))
                .ToList()
                .ForEach(g =>
                {
                    if (g.Any(e => e.IsInDatabase))
                    {
                        incomingTranslationIds.AddRange(g
                            .Where(t => !t.IsInDatabase)
                            .Select(t => t.TranslationId));
                    }
                });

            return incomingTranslationIds.Distinct();
        }

        public static IEnumerable<Guid> GetLemmaOrFormMatchTranslationIds(this IEnumerable<Lexeme> lexemeImportCandidates)
        {
            var incomingTranslationIds = new List<Guid>();

            // This is to allow the UI to designate 'import lexicon candidate' rows where the row's lexeme-or-forms match
            // any lexeme-or-forms from the incoming lexicon.  The thinking here is that maybe the user would want to add
            // the incoming lexeme's translation to the existing lexeme that matches the incoming one by lemma or form text:

            // UI needs to know which rows (translations) to put the "Add as translation for ..." button (incoming translation ids).

            lexemeImportCandidates
                .SelectMany(l => l.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .SelectMany(t => l.LemmaPlusFormTexts
                            .Select(text => (TranslationId: t.TranslationId.Id, t.IsInDatabase, l.Language, l.Type, LemmaOrFormText: text)))))
                .GroupBy(e => (e.Language, e.Type, e.LemmaOrFormText))
                .Select(g => g
                    .Select(e => (e.LemmaOrFormText, e.TranslationId, e.IsInDatabase)))
                .ToList()
                .ForEach(g =>
                {
                    // Gather all the source words (lemmas and forms) where IsInDatabase == true (internal lexeme target candidates)
                    // Gather all the source words (lemmas and forms) where IsInDatabase == false (incoming lexemes)
                    // Look for any 'partial matches' (not sure what that means yet...)

                    var incoming = g
                        .Where(e => !e.IsInDatabase)
                        .Select(e => e.LemmaOrFormText);

                    var established = g
                        .Where(e => e.IsInDatabase)
                        .Select(e => e.LemmaOrFormText);

                    if (incoming.AnyPartialMatch(established))
                    {
                        incomingTranslationIds.AddRange(g
                            .Where(e => !e.IsInDatabase)
                            .Select(e => e.TranslationId));
                    }
                });

            return incomingTranslationIds.Distinct();
        }

        public static bool AnyPartialMatch(this IEnumerable<string> incoming, IEnumerable<string> established)
        {
            return incoming.Any(i => established.Any(e => e.Contains(i)));
        }

        public static IEnumerable<(Guid IncomingTranslationId, Guid CandidateLexemeId)> GetAddAsFormMappings(
            this IEnumerable<Lexeme> first, 
            IEnumerable<Lexeme> second,
            Func<Translation, bool>? translationPredicate = null,
            Func<Translation, bool>? secondTranslationPredicate = null)
        {
            var combos = new List<(Guid IncomingTranslationId, Guid CandidateLexemeId)>();

            // This is to allow the UI to designate 'import lexicon candidate' rows where the row's translation matches
            // any translation from the internal lexicon.  The thinking here is that maybe the user would want to add
            // the incoming translation's lexeme as a form to another lexeme that contains a matching translation:

            // UI needs to know which rows (translations) to put the "Add as form for ..." button (incoming translation ids).
            // UI needs to know which lexemes have one or more translation that matches the target (candidate lexeme ids).

            translationPredicate ??= t => true;
            secondTranslationPredicate ??= t => true;

            var firstTranslationIdsInSecond = FindFirstTranslationIdsInSecond(first, second);

            var firstOnes = first
                .SelectMany(l => l.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Where(t => !firstTranslationIdsInSecond.Contains(t.TranslationId.Id))
                        .Where(translationPredicate)
                        .Select(t => (LexemeId: l.LexemeId.Id, TranslationId: t.TranslationId.Id, TranslationText: t.Text!, m.Language, From: nameof(first)))));

            var secondOnes = second
                .SelectMany(l => l.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Where(secondTranslationPredicate)
                        .Select(t => (LexemeId: l.LexemeId.Id, TranslationId: t.TranslationId.Id, TranslationText: t.Text!, m.Language, From: nameof(second)))));

            firstOnes.Union(secondOnes)
                .GroupBy(e => (e.TranslationText, e.Language))
                .Select(g => g.Select(e => (e.LexemeId, e.TranslationId, e.From)))
                .ToList()
                // ForEach Group:
                .ForEach(e =>
                {
                    foreach (var second in e.Where(t => t.From == nameof(second)))
                    {
                        combos.AddRange(e
                            .Where(t => t.From == nameof(first))
                            .Select(t => (t.TranslationId, second.LexemeId)));
                    }
                });

            return combos;
        }

        public static IEnumerable<(Guid IncomingTranslationId, Guid CandidateLexemeId)> GetAddAsTranslationMappings(
            this IEnumerable<Lexeme> first,
            IEnumerable<Lexeme> second,
            Func<Translation, bool>? translationPredicate = null,
            Func<Translation, bool>? secondTranslationPredicate = null)
        {
            var combos = new List<(Guid IncomingTranslationId, Guid CandidateLexemeId)>();

            // This is to allow the UI to designate 'import lexicon candidate' rows where the row's lexeme-or-forms match
            // any lexeme-or-forms from the incoming lexicon.  The thinking here is that maybe the user would want to add
            // the incoming lexeme's translation to the existing lexeme that matches the incoming one by lemma or form text:

            // UI needs to know which rows (translations) to put the "Add as translation for ..." button (incoming translation ids).
            // UI needs to know which lexemes have one or more translation that matches the source (candidate lexeme ids).

            translationPredicate ??= t => true;
            secondTranslationPredicate ??= t => true;

            var firstTranslationIdsInSecond = FindFirstTranslationIdsInSecond(first, second);

            var firstOnes = first
                .SelectMany(l => l.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Where(t => !firstTranslationIdsInSecond.Contains(t.TranslationId.Id))
                        .Where(translationPredicate)
                        .SelectMany(t => l.LemmaPlusFormTexts
                            .Select(text => (LexemeId: l.LexemeId.Id, TranslationId: t.TranslationId.Id, l.Language, l.Type, LemmaOrFormText: text, From: nameof(first))))));

            var secondOnes = second
                .SelectMany(l => l.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Where(secondTranslationPredicate)
                        .SelectMany(t => l.LemmaPlusFormTexts
                            .Select(text => (LexemeId: l.LexemeId.Id, TranslationId: t.TranslationId.Id, l.Language, l.Type, LemmaOrFormText: text, From: nameof(second))))));

            firstOnes.Union(secondOnes)
                .GroupBy(e => (e.Language, e.Type, e.LemmaOrFormText))
                .Select(g => g.Select(e => (e.LexemeId, e.TranslationId, e.From)))
                .ToList()
                // ForEach Group:
                .ForEach(e =>
                {
                    foreach (var second in e.Where(t => t.From == nameof(second)))
                    {
                        combos.AddRange(e
                            .Where(t => t.From == nameof(first))
                            .Select(t => (t.TranslationId, second.LexemeId)));
                    }
                });

            return combos;
        }

        private static IEnumerable<Guid> FindFirstTranslationIdsInSecond(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var (mapping, onlyInFirst, onlyInSecond) = RelateByLexemeMatchValue(first, second);

            var firstByGuid = first.ToDictionary(e => e.LexemeId.Id, e => e);
            var secondByGuid = second.ToDictionary(e => e.LexemeId.Id, e => e);

            var aaFirst = first.Where(e => e.Lemma == "a" && e.Type == "Word" && e.Language == "sur").FirstOrDefault();
            var aaSecond = second.Where(e => e.Lemma == "a" && e.Type == "Word" && e.Language == "sur").FirstOrDefault();

            var firstTranslationIdsInSecond = new List<Guid>();
            foreach (var (FirstGuid, SecondGuid) in mapping)
            {
                var secondLexemeMeaningsByMatch = secondByGuid[SecondGuid].Meanings
                    .ToDictionary(e => (e.Language, e.Text), e => e);

                foreach (var firstMeaning in firstByGuid[FirstGuid].Meanings)
                {
                    if (secondLexemeMeaningsByMatch.TryGetValue((firstMeaning.Language, firstMeaning.Text), out var secondMeaning))
                    {
                        // Translations:
                        firstTranslationIdsInSecond.AddRange(firstMeaning.Translations
                            .Where(e => secondMeaning.Translations.Select(t => t.Text).Contains(e.Text))
                            .Select(e => e.TranslationId.Id));
                    }
                }
            }

            return firstTranslationIdsInSecond;
        }

        /// <summary>
        /// Returns Lexemes in first that do not appear as full matches in second, based on the
        /// following full-match criteria:
        /// - a matching lemma and/or a form of a lexeme (lemma-or-form-text + lexeme language + lexeme type) AND 
        /// - a matching translation for the same lexeme (translation text + translation language)
        /// Lexemes having a partial match 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>
        /// Lexemes:  Lexemes in first that do not appear as full matches in second <br/>
        /// LemmaOrFormMatchesOnly:  Lexeme Ids from Lexemes that only match on LemmaOrForm <br/>
        /// TranslationMatchesOnly:  Lexeme Ids from Lexemes that only match on Translation
        /// </returns>
        public static (IEnumerable<Lexeme> Lexemes, IEnumerable<Guid> LemmaOrFormMatchesOnly, IEnumerable<Guid> TranslationMatchesOnly) ExceptByLexemeTranslationMatch(this IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
			var matchIds = FindLexemeIdsInFirstHavingMatchInSecond(first, second);
            var translationMatchIds = FindLexemeIdsInFirstHavingTranslationMatchInSecond(first, second);

            var lexemesWithoutFullMatch = first.ExceptBy(matchIds
                .Where(e => e.IsFullMatch)
                .Select(e => e.First), e => e.LexemeId.Id);

            return (
                lexemesWithoutFullMatch,
                matchIds.Where(e => !e.IsFullMatch).Select(e => e.First),
                translationMatchIds
                    .SelectMany(e => e.FirstLexemeIds)
                    .Except(matchIds
                        .Where(e => e.IsFullMatch)
                        .Select(e => e.First))
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
                .IntersectBy(matchIds
                    .Where(e => e.IsFullMatch)
                    .Select(e => e.First), e => e.LexemeId.Id)
                .Select(e => e.LexemeId);
        }

        private static IEnumerable<(Guid First, Guid Second, bool IsFullMatch)> FindLexemeIdsInFirstHavingMatchInSecond(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var firstLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(first);
            var secondLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(second);

            var firstTranslationMatchValuesByLexemeId = FindTranslationMatchValuesByLexemeId(first);
            var secondTranslationMatchValuesByLexemeId = FindTranslationMatchValuesByLexemeId(second);

			var lemmaOrFormMatchIds = new List<(Guid first, Guid second, bool isFullMatch)>();

			// Intersection results in 'lexeme match' values found in both first and
			// second.  For each of these, loop through all combinations of 'first'
			// and 'second' lexeme ids and for any 'translation match' values in both,
			// add to 'firstLexemeIdsHavingMatchInSecond' list: 
			foreach (var lexemeMatchValueInBoth in firstLexemeIdsByLexemeMatchValue.Keys
                .Intersect(secondLexemeIdsByLexemeMatchValue.Keys))
            {
                var firstLexemeId = firstLexemeIdsByLexemeMatchValue[lexemeMatchValueInBoth];
                var secondLexemeId = secondLexemeIdsByLexemeMatchValue[lexemeMatchValueInBoth];

                if (firstTranslationMatchValuesByLexemeId.TryGetValue(firstLexemeId, out var firstTranslationMatchValues) &&
                    secondTranslationMatchValuesByLexemeId.TryGetValue(secondLexemeId, out var secondTranslationMatchValues) &&
					firstTranslationMatchValues.SequenceEqual(secondTranslationMatchValues))
                {
					lemmaOrFormMatchIds.Add((firstLexemeId, secondLexemeId, true));
				}
				else
                {
					lemmaOrFormMatchIds.Add((firstLexemeId, secondLexemeId, false));
				}
			}

            return lemmaOrFormMatchIds;
        }

        private static List<((string TranslationText, string? Language) TranslationTextLanguage, List<Guid> FirstLexemeIds, List<Guid> SecondLexemeIds)> FindLexemeIdsInFirstHavingTranslationMatchInSecond(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var firstLexemeIdsByTranslationMatchValue = FindLexemeIdsByTranslationMatchValue(first);
            var secondLexemeIdsByTranslationMatchValue = FindLexemeIdsByTranslationMatchValue(second);

            return firstLexemeIdsByTranslationMatchValue.Keys
                .Intersect(secondLexemeIdsByTranslationMatchValue.Keys)
                .Select(e => (
                    TranslationTextLanguage: e,
                    FirstLexemeIds: firstLexemeIdsByTranslationMatchValue[e],
                    SecondLexemeIds: secondLexemeIdsByTranslationMatchValue[e]))
                .ToList();
        }

        private static (IEnumerable<(Guid FirstGuid, Guid SecondGuid)> Mapping, IEnumerable<Guid> OnlyInFirst, IEnumerable<Guid> OnlyInSecond) RelateByLexemeMatchValue(IEnumerable<Lexeme> first, IEnumerable<Lexeme> second)
        {
            var firstLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(first);
            var secondLexemeIdsByLexemeMatchValue = FindLexemeIdsByLexemeMatchValue(second);

            var mapping = firstLexemeIdsByLexemeMatchValue.Keys
                .Intersect(secondLexemeIdsByLexemeMatchValue.Keys)
                .Select(e => (
                    FirstGuid: firstLexemeIdsByLexemeMatchValue[e],
                    SecondGuid: secondLexemeIdsByLexemeMatchValue[e]))
                .ToList();

            var onlyInFirst = firstLexemeIdsByLexemeMatchValue
                .ExceptBy(secondLexemeIdsByLexemeMatchValue.Keys, e => e.Key)
                .Select(e => e.Value)
                .ToList();

            var onlyInSecond = secondLexemeIdsByLexemeMatchValue
                .ExceptBy(firstLexemeIdsByLexemeMatchValue.Keys, e => e.Key)
                .Select(e => e.Value)
                .ToList();

            return (mapping, onlyInFirst, onlyInSecond);
        }

        private static Dictionary<(string? Lemma, string? Language, string? Type), Guid> FindLexemeIdsByLexemeMatchValue(IEnumerable<Lexeme> lexemes)
        {
            var groups = lexemes
                .Select(e => (e.Lemma, e.Language, e.Type, LexemeId: e.LexemeId.Id))
                .GroupBy(e => (e.Lemma, e.Language, e.Type));

            if (groups.Any(g => g.Count() > 1))
            {
                var duplicateLexemes = groups
                    .Where(g => g.Count() > 1)
					.Select(g => $"Lemma: '{g.Key.Lemma}' Type: '{g.Key.Type}' Language: '{g.Key.Language}'")
					.ToList();

				throw new Exception($"Duplicate lexemes encountered:  {string.Join(", ", duplicateLexemes)}");
            }

			return groups
				.ToDictionary(
                g => g.Key,
                g => g.Select(e => e.LexemeId).First());
        }

        private static Dictionary<(string LemmaOrFormText, string? Language, string? Type), Guid> FindLexemeIdsByLemmaPlusFormsMatchValue(IEnumerable<Lexeme> lexemes)
        {
            var groups = lexemes
                .SelectMany(e => e.LemmaPlusFormTexts
                    .Select(l => (LemmaOrFormText: l, e.Language, e.Type, LexemeId: e.LexemeId.Id)))
                .GroupBy(e => (e.LemmaOrFormText, e.Language, e.Type));

            if (groups.Any(g => g.Count() > 1))
            {
                var duplicateLexemes = groups
                    .Where(g => g.Count() > 1)
                    .Select(g => $"Lemma: '{g.Key.LemmaOrFormText}' Type: '{g.Key.Type}' Language: '{g.Key.Language}'")
                    .ToList();

                throw new Exception($"Duplicate lexemes encountered:  {string.Join(", ", duplicateLexemes)}");
            }

            return groups
                .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.LexemeId).First());
        }

        private static Dictionary<(string TranslationText, string? Language), List<Guid>> FindLexemeIdsByTranslationMatchValue(IEnumerable<Lexeme> lexemes)
        {
            return lexemes
                .SelectMany(e => e.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Select(t => (TranslationText: t.Text!, m.Language, LexemeId: e.LexemeId.Id))))
                .GroupBy(e => (e.TranslationText, e.Language))
                .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.LexemeId).ToList());
        }

        private static Dictionary<Guid, List<(string TranslationText, string? Language)>> FindTranslationMatchValuesByLexemeId(IEnumerable<Lexeme> lexemes)
        {
            return lexemes
                .SelectMany(e => e.Meanings
                    .SelectMany(m => m.Translations
                        .Where(t => !string.IsNullOrEmpty(t.Text))
                        .Select(t => (TranslationText: t.Text!, m.Language, LexemeId: e.LexemeId.Id))))
                .GroupBy(e => e.LexemeId)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .OrderByDescending(e => e.Language)
                        .OrderBy(e => e.TranslationText)
                        .Select(e => (e.TranslationText, e.Language))
                        .ToList());
        }
    }
}
