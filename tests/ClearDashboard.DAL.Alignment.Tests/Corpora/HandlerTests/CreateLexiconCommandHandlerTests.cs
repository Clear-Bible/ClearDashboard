using System;
using System.Collections.Generic;
using System.Linq;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using System.Threading.Tasks;
using System.Text;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Threading;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using Lexeme = ClearDashboard.DAL.Alignment.Lexicon.Lexeme;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using SIL.Scripture;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Org.BouncyCastle.Utilities.Encoders;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;

public class CreateLexiconCommandHandlerTests : TestBase
{
    public CreateLexiconCommandHandlerTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    [Trait("Category", "WordAnalyses")]
    public async Task WordAnalyses_ExternalGet()
    {
        try
        {
			var externalLexiconCommand = new GetExternalLexiconQuery(null /* currently loaded project */);
			var externalLexiconResult = await Mediator.Send(externalLexiconCommand);
			await externalLexiconResult.Data!.SaveAsync(Mediator!);

			var internalLexiconCommand = new GetInternalLexiconQuery();
			var internalLexiconResult = await Mediator.Send(internalLexiconCommand);
			var internalLexemesByTypeLemma = internalLexiconResult.Data!.Lexemes
                .GroupBy(e => (e.Type?.ToLower(), e.Lemma?.ToLower()))
                .ToDictionary(
                    g => g.Key, 
                    g => g.Select(e => e).First());

			var externalWordAnalysesCommand = new GetExternalWordAnalysesQuery(null /* currently loaded project */);
            var externalWordAnalysesResult = await Mediator.Send(externalWordAnalysesCommand);
            var wordsToImport = externalWordAnalysesResult.Data!.ToList();

            var wordsToImportLexemeCount = wordsToImport.SelectMany(e => e.Lexemes).Count();
            Output.WriteLine("");
            Output.WriteLine($"Lexemes in wordsToImport: {wordsToImportLexemeCount}");

			foreach (var w in wordsToImport)
			{
                for (int i = 0; i < w.Lexemes.Count; i++)
                {
                    var lex = w.Lexemes[i];
                    if (internalLexemesByTypeLemma.TryGetValue((lex.Type?.ToLower(), lex.Lemma?.ToLower()), out var replacementLexeme))
                    {
                        w.Lexemes[i] = replacementLexeme;
                    }
                }
			}

			var lexemesInWordsToImport = wordsToImport
				.SelectMany(e => e.Lexemes)
				.GroupBy(e => e.LexemeId.Id)
				.SelectMany(g => g.Select(l => l))
				.ToList();
			Output.WriteLine($"WordAnalyses lexemes to import: {lexemesInWordsToImport.Where(e => !e.IsInDatabase).Count()}");
			Output.WriteLine($"WordAnalyses lexemes already imported: {lexemesInWordsToImport.Where(e => e.IsInDatabase).Count()}");

            Output.WriteLine("");
            var ct = 0;
            foreach (var wordSample in wordsToImport.Where(e => e.Lexemes.Any(l => l.IsInDatabase)))
            {
				Output.WriteLine($"Word:  {wordSample.Word}");
				foreach (var lex in wordSample.Lexemes)
				{
					Output.WriteLine($"\tLexeme:  {lex.Type}:{lex.Lemma} [{lex.Language}] is in database: {lex.IsInDatabase}");
				}

				if (ct++ > 5) break;
			}

			ct = 0;
			foreach (var wordSample in wordsToImport.Where(e => e.Lexemes.Any(l => !l.IsInDatabase)))
			{
				Output.WriteLine($"Word:  {wordSample.Word}");
				foreach (var lex in wordSample.Lexemes)
				{
					Output.WriteLine($"\tLexeme:  {lex.Type}:{lex.Lemma} [{lex.Language}] is in database: {lex.IsInDatabase}");
				}
				
                if (ct++ > 5) break;
			}
		}
		catch (Exception ex)
		{
			Output.WriteLine($"\tException:  {ex.Message}");
		}
	}
	[Fact]
	[Trait("Category", "Handlers")]
	public async Task Lexicon_ExternalSave()
	{
		try
		{
			var lemma1 = "Lemma_1";
			var lemma2 = "Lemma_2";
			var lemma3 = "Lemma_3";
			var lemma1Form1 = lemma1 + "_form1";
            var lemma1Form2 = lemma1 + "_form2";
            var lemma1Meaning1 = lemma1 + "_meaning1";
            var lemma1Meaning2 = lemma1 + "_meaning2";
            var lemma2Meaning1 = lemma2 + "_meaning1";
            var lemma2Meaning2 = lemma2 + "_meaning2";
            var lemma2Meaning3 = lemma2 + "_meaning3";
            var lemma3Meaning1 = lemma3 + "_meaning1";
            var lemma1Meaning2Tr1 = lemma1Meaning2 + "_tr1";
            var lemma1Meaning2Tr2 = lemma1Meaning2 + "_tr2";
            var languageEnglish = "en";
            var languageHebrew = "he";

            // LEXICON
            // lexeme1:
            var lexeme1 = await new Lexeme
            {
                Lemma = lemma1,
                Language = languageEnglish,
                Type = "some type"
            }.Create(Mediator!);

            await lexeme1.PutForm(Mediator!, new Form { Text = lemma1Form1 });
            await lexeme1.PutForm(Mediator!, new Form { Text = lemma1Form2 });

            var lexeme1Meaning1 = new Meaning
            {
                Text = lemma1Meaning1,
                Language = languageHebrew
            };
            var lexeme1Meaning2 = new Meaning { Text = lemma1Meaning2 /* no language */ };
            await lexeme1.PutMeaning(Mediator!, lexeme1Meaning1);
            await lexeme1.PutMeaning(Mediator!, lexeme1Meaning2);

            await lexeme1Meaning2.PutTranslation(Mediator!, new Lexicon.Translation { Text = lemma1Meaning2Tr1 });
            await lexeme1Meaning2.PutTranslation(Mediator!, new Lexicon.Translation { Text = lemma1Meaning2Tr2 });

            // lexeme2:
            var lexeme2 = await new Lexeme { Lemma = lemma2 /* no language */  }.Create(Mediator!);
            await lexeme2.PutMeaning(Mediator!, new Meaning { Text = lemma2Meaning1 /* no language */ });
            var lexeme2Meaning2 = new Meaning { Text = lemma2Meaning2, Language = "bogus" };
            var lexeme2Meaning3 = new Meaning { Text = lemma2Meaning3, Language = languageHebrew };
            await lexeme2.PutMeaning(Mediator!, lexeme2Meaning2);
            await lexeme2.PutMeaning(Mediator!, lexeme2Meaning3);
            var s1 = await lexeme2Meaning2.CreateAssociateSenanticDomain(Mediator!, "sem1");
            var s2 = await lexeme2Meaning3.CreateAssociateSenanticDomain(Mediator!, "sem2");

            // lexeme3:
            var lexeme3 = await new Lexeme { Lemma = lemma3, Language = "bogus" }.Create(Mediator!);
            await lexeme3.PutMeaning(Mediator!, new Meaning { Text = lemma3Meaning1 /* no language */ });
            await lexeme3.Meanings.First().AssociateSemanticDomain(Mediator!, s2);

            ProjectDbContext!.ChangeTracker.Clear();

            var internalLexiconCommand = new GetInternalLexiconQuery();
            var internalLexiconResult = await Mediator.Send(internalLexiconCommand);

            var internalLexicon = internalLexiconResult.Data!;

            Assert.Equal(3, internalLexicon.Lexemes.Count);

            var internalLexiconLexeme1 = internalLexicon.Lexemes.Where(e => e.Lemma == lemma1).First();
            var internalLexiconLexeme2 = internalLexicon.Lexemes.Where(e => e.Lemma == lemma2).First();
            var internalLexiconLexeme3 = internalLexicon.Lexemes.Where(e => e.Lemma == lemma3).First();

            Assert.Equal(2, internalLexiconLexeme1.Forms.Count);
            Assert.Equal(2, internalLexiconLexeme1.Meanings.Count);

            var internalLexiconLexeme1Meaning2 = internalLexiconLexeme1.Meanings.Where(e => e.Text == lemma1Meaning2).First();

            Assert.Equal(2, internalLexiconLexeme1Meaning2.Translations.Count);
            AssertIsInDatabaseIsDirty(internalLexicon, true, false);

            var externalLexiconCommand = new GetExternalLexiconQuery(null /* currently loaded project */);
            var externalLexiconResult = await Mediator.Send(externalLexiconCommand);

            var externalLexicon = externalLexiconResult.Data!;
            AssertIsInDatabaseIsDirty(externalLexicon, false, false);

            var dups = externalLexicon.Lexemes
                .Where(e => e.Lemma == "naan" && e.Language == "sur")
                .ToList();

            var lexemesExternalExceptInternal = externalLexicon.Lexemes
                .Where(el =>
                    !internalLexicon.Lexemes.Any(il =>
                        il.LemmaPlusFormTexts.Intersect(el.LemmaPlusFormTexts).Any() &&
                        il.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))
                            .Intersect(
                        el.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))
                            ).Any()
                    ));

            var lemmaLanguagePairs = externalLexicon.Lexemes
                .Select(e => (e.Lemma, e.Language))
                .GroupBy(e => e)
                .ToDictionary(g => g.Key, g => g.Count());

            //Output.WriteLine($"Duplicate language/lemma record count: [{lemmaLanguagePairs.Count()}]");
            //foreach (var lexeme in lemmaLanguagePairs.Where(e => e.Value > 1))
            //{
            //    Output.WriteLine($"Lemma: '{lexeme.Key.Lemma}', language: '{lexeme.Key.Language}', count: [{lexeme.Value}]");
            //}

            var partialExternalLexicon = new Alignment.Lexicon.Lexicon()
            {
                Lexemes = new ObservableCollection<Lexeme>(externalLexicon.Lexemes.Take(5000))
            };
            await partialExternalLexicon.SaveAsync(Mediator!);
            AssertIsInDatabaseIsDirty(partialExternalLexicon, true, false);

            var externalLexiconCommand2 = new GetExternalLexiconQuery("2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f");
            var externalLexiconResult2 = await Mediator.Send(externalLexiconCommand2);

            var internalLexiconCommand2 = new GetInternalLexiconQuery();
            var internalLexiconResult2 = await Mediator.Send(internalLexiconCommand2);

            Stopwatch sw = new();
            sw.Start();

            var lexemeIdsInFirstHavingMatchInSecond = externalLexiconResult2.Data!.Lexemes
                .IntersectIdsByLexemeTranslationMatch(internalLexiconResult2.Data!.Lexemes);

            Assert.True(lexemeIdsInFirstHavingMatchInSecond.Count() > 2000);

            //var leftovers = externalLexiconResult2.Data!.Lexemes
            //    .Where(el =>
            //        !internalLexiconResult2.Data!.Lexemes.Any(il =>
            //            il.LemmaPlusFormTexts.Intersect(el.LemmaPlusFormTexts).Any() &&
            //            il.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))
            //                .Intersect(
            //            el.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))
            //                ).Any()
            //        ))
            //    .ToList();

            sw.Stop();
            Output.WriteLine($"IntersectIdsByLexemeTranslationMatch:  {sw.Elapsed}");

            sw.Restart();

            var externalNotInInternal = externalLexiconResult2.Data!.Lexemes.ExceptByLexemeTranslationMatch(internalLexiconResult2!.Data!.Lexemes);
            var externalLexemesIdsNotInInternal = externalNotInInternal.Lexemes.Select(e => e.LexemeId.Id);
            Assert.False(externalNotInInternal.LemmaOrFormMatchesOnly.Except(externalLexemesIdsNotInInternal).Any());
            Assert.False(externalNotInInternal.TranslationMatchesOnly.Except(externalLexemesIdsNotInInternal).Any());

            sw.Stop();
            Output.WriteLine($"ExceptByLexemeTranslationMatch:  {sw.Elapsed}");
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    private void AssertIsInDatabaseIsDirty(Alignment.Lexicon.Lexicon lexicon, bool isInDatabase, bool isDirty)
    {
        foreach (var lexeme in lexicon.Lexemes)
        {
            Assert.Equal(isInDatabase, lexeme.IsInDatabase);
            Assert.Equal(isDirty, lexeme.IsDirty);

            foreach (var form in lexeme.Forms)
            {
                Assert.Equal(isInDatabase, form.IsInDatabase);
                Assert.Equal(isDirty, form.IsDirty);
            }

            foreach (var meaning in lexeme.Meanings)
            {
                Assert.Equal(isInDatabase, meaning.IsInDatabase);
                Assert.Equal(isDirty, meaning.IsDirty);

                foreach (var translation in meaning.Translations)
                {
                    Assert.Equal(isInDatabase, translation.IsInDatabase);
                    Assert.Equal(isDirty, translation.IsDirty);
                }
            }
        }
    }
}