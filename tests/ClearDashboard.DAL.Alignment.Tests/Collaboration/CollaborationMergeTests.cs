using Models = ClearDashboard.DataAccessLayer.Models;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.Collaboration.Factory;
using SIL.Scripture;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using ClearDashboard.DAL.Alignment.Features.Notes;
using ClearDashboard.ParatextPlugin.CQRS.Features.Notes;
using System.IO;
using ClearDashboard.DAL.Alignment.Features.Common;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DataAccessLayer.Models;
using Microsoft.SqlServer.Server;
using ClearDashboard.Collaboration;

namespace ClearDashboard.DAL.Alignment.Tests.Collaboration
{
    [Collection("Sequential")]
    [TestCaseOrderer("ClearDashboard.DAL.Alignment.Tests.Collaboration.AlphabeticalOrderer", "ClearDashboard.DAL.Alignment.Tests")]
    public class CollaborationMergeTests : IClassFixture<CollaborationProjectFixture>
    {
        CollaborationProjectFixture _fixture;
        ITestOutputHelper _output;

        public CollaborationMergeTests(CollaborationProjectFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }


        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test00()
        {
            //try
            //{
            //    var backupsPath =
            //        FilePathTemplates.CollabBaseDirectory + Path.DirectorySeparatorChar + "Backups";

            //    var factoryPrevious = new ProjectSnapshotFromFilesFactory(Path.Combine(backupsPath, "Roman7"), _fixture.Logger);
            //    var previousSnapshotFromFile = factoryPrevious.LoadSnapshot();

            //    var factoryNew = new ProjectSnapshotFromFilesFactory(Path.Combine(backupsPath, "Roman7_Split"), _fixture.Logger);
            //    var newSnapshotFromFile = factoryNew.LoadSnapshot();

            //    await DoMerge(false, newSnapshotFromFile, previousSnapshotFromFile);
            //}
            //catch (Exception ex)
            //{
            //}

            //return;

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();
            Assert.Equal(_fixture.Users.Count + 1, _fixture.ProjectDbContext.Users.Count());  // TestBase added 1 when setting up the project, and we added 2 in BuildInitialEntities
            Assert.Equal(_fixture.Corpora.Count, _fixture.ProjectDbContext.Corpa.Count());
            Assert.Equal(_fixture.TokenizedCorpora.Count, _fixture.ProjectDbContext.TokenizedCorpora.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.TokenComposites.Count());
            Assert.Equal(5, _fixture.ProjectDbContext.TokenCompositeTokenAssociations.Count());
            Assert.Equal(25, _fixture.ProjectDbContext.TokenComponents.Count());  // 23 Tokens and 2 TokenComposites

            Assert.Equal((int)ScrVers.English.Type, _fixture.ProjectDbContext.TokenizedCorpora.First().ScrVersType);
            Assert.Null(_fixture.ProjectDbContext.TokenizedCorpora.First().CustomVersData);

            var tokenComposite = _fixture.ProjectDbContext.TokenCompositeTokenAssociations.Include(e => e.TokenComposite).Select(e => e.TokenComposite!).First();
            var tokens = _fixture.ProjectDbContext.TokenCompositeTokenAssociations.Include(e => e.Token).Where(e => e.TokenCompositeId == tokenComposite!.Id).Select(e => e.Token!).ToList();

            //var compositeToken = ModelHelper.BuildCompositeToken(tokenComposite, tokens);

            //var note = new Note { Text = "a composite note", AbbreviatedText = "not sure", NoteStatus = "Resolved" };
            //await note.CreateOrUpdate(_fixture.Mediator);
            //await note.AssociateDomainEntity(_fixture.Mediator, compositeToken.TokenId);

            //var externalLexicon = await Lexicon.Lexicon.GetExternalLexicon(_fixture.Mediator, "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f");
            //await externalLexicon.SaveAsync(_fixture.Mediator);

            //var lexemeCount = _fixture.ProjectDbContext.Lexicon_Lexemes.Count();
            //var meaningCount = _fixture.ProjectDbContext.Lexicon_Meanings.Count();
            //var formCount = _fixture.ProjectDbContext.Lexicon_Forms.Count();
            //var translationCount = _fixture.ProjectDbContext.Lexicon_Translations.Count();

            //var defaultCreatedDate = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();

            //var extraLexeme = new Models.Lexicon_Lexeme
            //{
            //    Id = Guid.NewGuid(),
            //    Lemma = "booboo",
            //    Language = "sur",
            //    Type = "Stem",
            //    Created = defaultCreatedDate,
            //    UserId = _fixture.Users.First().Id
            //};

            //var extraForm1 = new Models.Lexicon_Form
            //{
            //    Id = Guid.NewGuid(),
            //    Text = "forrrrrm1",
            //    Lexeme = extraLexeme,
            //    LexemeId = extraLexeme.Id,
            //    Created = defaultCreatedDate,
            //    UserId = _fixture.Users.First().Id
            //};

            //var extraForm2 = new Models.Lexicon_Form
            //{
            //    Id = Guid.NewGuid(),
            //    Text = "forrrrrm2",
            //    Lexeme = extraLexeme,
            //    LexemeId = extraLexeme.Id,
            //    Created = defaultCreatedDate,
            //    UserId = _fixture.Users.First().Id
            //};

            //var extraMeaning = new Models.Lexicon_Meaning
            //{
            //    Id = Guid.NewGuid(),
            //    Text = "mean1",
            //    Language = "en",
            //    Lexeme = extraLexeme,
            //    LexemeId = extraLexeme.Id,
            //    Created = defaultCreatedDate,
            //    UserId = _fixture.Users.First().Id
            //};

            //var extraTranslation1 = new Models.Lexicon_Translation
            //{
            //    Id = Guid.NewGuid(),
            //    Text = "trrrrr1",
            //    Meaning = extraMeaning,
            //    MeaningId = extraMeaning.Id,
            //    Created = defaultCreatedDate,
            //    UserId = _fixture.Users.First().Id
            //};

            //var extraTranslation2 = new Models.Lexicon_Translation
            //{
            //    Id = Guid.NewGuid(),
            //    Text = "trrrrr2",
            //    Meaning = extraMeaning,
            //    MeaningId = extraMeaning.Id,
            //    Created = defaultCreatedDate,
            //    UserId = _fixture.Users.First().Id
            //};

            //extraMeaning.Translations.Add(extraTranslation1);
            //extraMeaning.Translations.Add(extraTranslation1);
            //extraLexeme.Meanings.Add(extraMeaning);
            //extraLexeme.Forms.Add(extraForm1);
            //extraLexeme.Forms.Add(extraForm2);

            //_fixture.LexiconLexemes.Add(extraLexeme);

            //await DoMerge();

            //var lexemeCount2 = _fixture.ProjectDbContext.Lexicon_Lexemes.Count();
            //var meaningCount2 = _fixture.ProjectDbContext.Lexicon_Meanings.Count();
            //var formCount2 = _fixture.ProjectDbContext.Lexicon_Forms.Count();
            //var translationCount2 = _fixture.ProjectDbContext.Lexicon_Translations.Count();

            //var lexemeForTest = _fixture.ProjectDbContext.Lexicon_Lexemes.Where(e => e.Lemma == "booboo").FirstOrDefault();

            //var exernalLexemesForDb = LexiconToModel(externalLexicon, _fixture.Users.First().Id);
            //_fixture.LexiconLexemes.AddRange(exernalLexemesForDb);

            //await DoMerge();

            //_fixture.ProjectDbContext.ChangeTracker.Clear();

            //var lexemeCount3 = _fixture.ProjectDbContext.Lexicon_Lexemes.Count();
            //var meaningCount3 = _fixture.ProjectDbContext.Lexicon_Meanings.Count();
            //var formCount3 = _fixture.ProjectDbContext.Lexicon_Forms.Count();
            //var translationCount3 = _fixture.ProjectDbContext.Lexicon_Translations.Count();

            //var lexemeForTest2 = _fixture.ProjectDbContext.Lexicon_Lexemes.Where(e => e.Lemma == "booboo").FirstOrDefault();
        }

        private IEnumerable<Models.Lexicon_Lexeme> LexiconToModel(Lexicon.Lexicon lexicon, Guid defaultUserId)
        {
            var defaultCreatedDate = Models.TimestampedEntity.GetUtcNowRoundedToMillisecond();
            var lexemesDb = new List<Models.Lexicon_Lexeme>();

            foreach (var lexeme in lexicon.Lexemes)
            {
                var lexemeDb = new Models.Lexicon_Lexeme
                {
                    Id = lexeme.LexemeId.Id,
                    Lemma = lexeme.Lemma,
                    Type = lexeme.Type,
                    Language = lexeme.Language,
                    Created = lexeme.LexemeId.Created ?? defaultCreatedDate,
                    UserId = lexeme.LexemeId.UserId?.Id ?? defaultUserId
                };

                // ---------------------------------------------------------------------------------
                // Create and Update Lexeme Forms:
                // ---------------------------------------------------------------------------------
                foreach (var form in lexeme.Forms)
                {
                    var formDb = new Models.Lexicon_Form
                    {
                        Id = form.FormId.Id,
                        Text = form.Text,
                        Lexeme = lexemeDb,
                        LexemeId = lexemeDb.Id,
                        Created = form.FormId.Created ?? defaultCreatedDate,
                        UserId = form.FormId.UserId?.Id ?? defaultUserId
                    };

                    lexemeDb.Forms.Add(formDb);
                }

                // ---------------------------------------------------------------------------------
                // Create / Update Lexeme Meanings and children:
                // ---------------------------------------------------------------------------------
                foreach (var meaning in lexeme.Meanings)
                {
                    var meaningDb = new Models.Lexicon_Meaning
                    {
                        Id = meaning.MeaningId.Id,
                        Text = meaning.Text,
                        Language = meaning.Language,
                        Lexeme = lexemeDb,
                        LexemeId = lexemeDb.Id,
                        Created = meaning.MeaningId.Created ?? defaultCreatedDate,
                        UserId = meaning.MeaningId.UserId?.Id ?? defaultUserId
                    };

                    lexemeDb.Meanings.Add(meaningDb);

                    // ---------------------------------------------------------------------------------
                    // Create / Update Lexeme Meaning Translations:
                    // ---------------------------------------------------------------------------------
                    foreach (var translation in meaning.Translations.Where(e => !e.ExcludeFromSave))
                    {
                        var translationDb = new Models.Lexicon_Translation
                        {
                            Id = translation.TranslationId.Id,
                            Text = translation.Text,
                            Meaning = meaningDb,
                            MeaningId = meaningDb.Id,
                            Created = translation.TranslationId.Created ?? defaultCreatedDate,
                            UserId = translation.TranslationId.UserId?.Id ?? defaultUserId
                        };

                        meaningDb.Translations.Add(translationDb);
                    }
                }

                lexemesDb.Add(lexemeDb);
            }

            return lexemesDb;
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test01()
        {
            // Add custom versification of tokenized corpus

            var testTokenizedCorpus = _fixture.TokenizedCorpora.FirstOrDefault();
            Assert.NotNull(testTokenizedCorpus);

            var customVersification = CollaborationProjectFixture.BuildCustomVersification(ScrVers.RussianOrthodox);
            CollaborationProjectFixture.SetVersification(testTokenizedCorpus, customVersification);

            var testUsers = _fixture.Users.ToArray();
            Assert.Equal(2, testUsers.Length);

            // Add a manuscript Hebrew tokenized corpus

            var (hebrewCorpus, hebrewTokenizedCorpus) = CollaborationProjectFixture.BuildTestManuscriptHebrew(testUsers[1].Id);
            _fixture.Corpora.Add(hebrewCorpus);
            _fixture.TokenizedCorpora.Add(hebrewTokenizedCorpus);

            // Add a parallel corpus

            var testParallelCorpus = CollaborationProjectFixture.BuildTestParallelCorpus(
                testTokenizedCorpus,
                hebrewTokenizedCorpus,
                testUsers[0].Id);
            _fixture.ParallelCorpora.Add(testParallelCorpus);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();
            Assert.Equal(2, _fixture.ProjectDbContext.TokenizedCorpora.Count());
            Assert.Equal(1, _fixture.ProjectDbContext.ParallelCorpa.Count());
            Assert.True(_fixture.ProjectDbContext.TokenComponents.Count() > 400000);        // 23 Tokens + 2 TokenComposites + 468613 (Hebrew)
            Assert.True(_fixture.ProjectDbContext.VerseMappings.Count() > 30000);           // 31163
            Assert.True(_fixture.ProjectDbContext.Verses.Count() > 60000);                  // 62335

            Assert.Equal((int)customVersification.BaseVersification.Type, _fixture.ProjectDbContext.TokenizedCorpora.Where(e => e.Id == testTokenizedCorpus.Id).First().ScrVersType);
            Assert.NotNull(_fixture.ProjectDbContext.TokenizedCorpora.Where(e => e.Id == testTokenizedCorpus.Id).First().CustomVersData);
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test02()
        {
            // Change custom versification of first tokenized corpus (should trigger rebuilding of 
            // the verse mappings of the parallel corpus)

            var latestOldVerseMappingCreated = _fixture.ProjectDbContext.VerseMappings.OrderByDescending(e => e.Created).Select(e => e.Created).First();
            var oldVerseMappingCount = _fixture.ProjectDbContext.VerseMappings.Count();
            var oldVerseCount = _fixture.ProjectDbContext.Verses.Count();

            var testTokenizedCorpus = _fixture.TokenizedCorpora.FirstOrDefault();
            Assert.NotNull(testTokenizedCorpus);

            var beforeScrVersType = testTokenizedCorpus.ScrVersType;
            var beforeCustomVersData = testTokenizedCorpus.CustomVersData;
            Assert.False(string.IsNullOrEmpty(beforeCustomVersData));

            var customVersification = CollaborationProjectFixture.BuildCustomVersification(ScrVers.RussianProtestant, 4);
            CollaborationProjectFixture.SetVersification(testTokenizedCorpus, customVersification);

            var testParallelCorpus = _fixture.ParallelCorpora.First();
            var testUser = _fixture.Users.Skip(1).First();

            // Add Alignment set and some alignments

            var alignmentSet = CollaborationProjectFixture.BuildTestAlignmentSet(testParallelCorpus, testUser.Id);
            _fixture.AlignmentSets.Add(alignmentSet);

            _fixture.Alignments.AddRange(CollaborationProjectFixture.BuildTestAlignments(
                alignmentSet,
                Models.AlignmentVerification.Verified,
                Models.AlignmentOriginatedFrom.Assigned,
                44,
                testUser.Id,
                new List<(Models.TokenComponent, Models.TokenComponent)>
                {
                    (
                        CollaborationProjectFixture.BuildTestToken(testParallelCorpus.SourceTokenizedCorpus!, "001001001001001"),
                        CollaborationProjectFixture.BuildTestToken(testParallelCorpus.TargetTokenizedCorpus!, "001001001001001")
                    ),
                    (
                        _fixture.TokenComposites.Where(e => e.EngineTokenId == "001001001002001-001001001003001").First(),
                        CollaborationProjectFixture.BuildTestToken(testParallelCorpus.TargetTokenizedCorpus!, "001001001002001")
                    )
                }
            ));

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            // Verify versification change:
            var testTokenizedCorpusFromDb = _fixture.ProjectDbContext.TokenizedCorpora
                .Include(e => e.TokenComponents)
                .Where(e => e.Id == testTokenizedCorpus.Id).FirstOrDefault();
            Assert.NotNull(testTokenizedCorpusFromDb);
            Assert.False(string.IsNullOrEmpty(testTokenizedCorpusFromDb.CustomVersData));
            Assert.NotEqual(beforeScrVersType, testTokenizedCorpusFromDb.ScrVersType);
            Assert.NotEqual(beforeCustomVersData, testTokenizedCorpusFromDb.CustomVersData);

            // Should not have merged any of the tokens above into the database/
            // because none of them were soft deleted (i.e. split tokens)
            Assert.Equal(23, testTokenizedCorpusFromDb.TokenComponents.Where(e => e.GetType() == typeof(Models.Token)).Count());
            Assert.Equal(2, testTokenizedCorpusFromDb.TokenComponents.Where(e => e.GetType() == typeof(Models.TokenComposite)).Count());

            Assert.True(_fixture.ProjectDbContext.VerseMappings.Count() > 30000);           // 31163
            Assert.True(_fixture.ProjectDbContext.Verses.Count() > 60000);                  // 62335

            // Verify that new verse mappings were created (because of versification change)
            var earliestNewVerseMappingCreated = _fixture.ProjectDbContext.VerseMappings.OrderBy(e => e.Created).Select(e => e.Created).First();
            var newVerseMappingCount = _fixture.ProjectDbContext.VerseMappings.Count();
            var newVerseCount = _fixture.ProjectDbContext.Verses.Count();
            Assert.True(earliestNewVerseMappingCreated > latestOldVerseMappingCreated);

            Assert.Equal(1, _fixture.ProjectDbContext.AlignmentSets.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.Alignments.Count());
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test03()
        {
            var testUser = _fixture.Users.First();

            // Add two SemanticDomains
            var semanticDomain1 = CollaborationProjectFixture.BuildTestLexiconSemanticDomain("sd1", testUser.Id);
            var semanticDomain2 = CollaborationProjectFixture.BuildTestLexiconSemanticDomain("sd2", testUser.Id);

            // Add a Lexicon
            var lexiconLexeme1 = CollaborationProjectFixture.BuildTestLexiconLexeme("en", "lemma1", null, testUser.Id);
            var lexiconLexeme1Meaning1 = CollaborationProjectFixture.BuildTestLexiconMeaning("fr", "lemma1Meaning1", lexiconLexeme1, testUser.Id);
            _ = CollaborationProjectFixture.BuildTestLexiconTranslation("lemma1Meaning1Tr1", lexiconLexeme1Meaning1, testUser.Id);
            var lexiconLexeme1Meaning2 = CollaborationProjectFixture.BuildTestLexiconMeaning("fr", "lemma1Meaning2", lexiconLexeme1, testUser.Id);
            var lexiconLexeme1Meaning2Tr1 = CollaborationProjectFixture.BuildTestLexiconTranslation("lemma1Meaning2Tr1", lexiconLexeme1Meaning2, testUser.Id);
            _fixture.LexiconLexemes.Add(lexiconLexeme1);

            _ = CollaborationProjectFixture.BuildTestLexiconSemanticDomainMeaningAssociation(semanticDomain1, lexiconLexeme1Meaning2, testUser.Id);

            var lexiconLexeme2 = CollaborationProjectFixture.BuildTestLexiconLexeme("en", "lemma2", null, testUser.Id);
            var lexiconLexeme2Meaning1 = CollaborationProjectFixture.BuildTestLexiconMeaning("fr", "lemma2Meaning1", lexiconLexeme2, testUser.Id);
            _ = CollaborationProjectFixture.BuildTestLexiconTranslation("lemma2Meaning1Tr1", lexiconLexeme2Meaning1, testUser.Id);
            _ = CollaborationProjectFixture.BuildTestLexiconForm("lemma2Form1", lexiconLexeme2, testUser.Id);
            _fixture.LexiconLexemes.Add(lexiconLexeme2);

            _ = CollaborationProjectFixture.BuildTestLexiconSemanticDomainMeaningAssociation(semanticDomain1, lexiconLexeme2Meaning1, testUser.Id);
            _ = CollaborationProjectFixture.BuildTestLexiconSemanticDomainMeaningAssociation(semanticDomain2, lexiconLexeme2Meaning1, testUser.Id);

            _fixture.LexiconSemanticDomains.Add(semanticDomain1);
            _fixture.LexiconSemanticDomains.Add(semanticDomain2);

            // Add translation set and some translations

            var testAlignmentSet = _fixture.AlignmentSets.First();

            var translationSet = CollaborationProjectFixture.BuildTestTranslationSet(testAlignmentSet, testUser.Id);
            _fixture.TranslationSets.Add(translationSet);

            _fixture.Translations.AddRange(CollaborationProjectFixture.BuildTestTranslations(
                translationSet,
                Models.TranslationOriginatedFrom.Assigned,
                testUser.Id,
                new List<(Models.TokenComponent, string, Models.Lexicon_Translation?)>
                {
                    (
                        CollaborationProjectFixture.BuildTestToken(testAlignmentSet.ParallelCorpus!.SourceTokenizedCorpus!, "001001001001001"),
                        "target text one",
                        null
                    ),
                    (
                        _fixture.TokenComposites.Where(e => e.EngineTokenId == "001001001002001-001001001003001").First(),
                        "target text two",
                        lexiconLexeme1Meaning2Tr1
                    )
                }));

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, _fixture.ProjectDbContext.TranslationSets.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.Translations.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.Lexicon_Lexemes.Count());
            Assert.Equal(3, _fixture.ProjectDbContext.Lexicon_Meanings.Count());
            Assert.Equal(3, _fixture.ProjectDbContext.Lexicon_Translations.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.Lexicon_SemanticDomains.Count());
            Assert.Equal(3, _fixture.ProjectDbContext.Lexicon_SemanticDomainMeaningAssociations.Count());

            Assert.Single(_fixture.ProjectDbContext.Translations.Where(t => t.LexiconTranslationId != null));

            var lexiconLexemeMeaningsDb = _fixture.ProjectDbContext.Lexicon_Meanings
                .Include(e => e.SemanticDomains)
                .ToDictionary(e => e.Id, e => e);

            Assert.Empty(lexiconLexemeMeaningsDb.Where(e => e.Value.Text == lexiconLexeme1Meaning1.Text).First().Value.SemanticDomains);
            Assert.Single(lexiconLexemeMeaningsDb.Where(e => e.Value.Text == lexiconLexeme1Meaning2.Text).First().Value.SemanticDomains);
            Assert.Equal(2, lexiconLexemeMeaningsDb.Where(e => e.Value.Text == lexiconLexeme2Meaning1.Text).First().Value.SemanticDomains.Count);
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test04()
        {
            // Add some notes and associate with tokens

            var builderContext = new BuilderContext(_fixture.ProjectDbContext!);

            var testTokenizedCorpus = _fixture.TokenizedCorpora.First();
            var testUser = _fixture.Users.Skip(1).First();

            var testNoteId1 = Guid.NewGuid();
            var testNoteId2 = Guid.NewGuid();
            var testNoteId3 = Guid.NewGuid();
            var testNoteId4 = Guid.NewGuid();
            var testNoteId5 = Guid.NewGuid();
            var testNoteId6 = Guid.NewGuid();

            var testNoteId4Reply1 = Guid.NewGuid();

            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId1, "boo boo", Models.NoteStatus.Open, testUser.Id));
            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId2, "boo boo two", Models.NoteStatus.Resolved, testUser.Id));
            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId3, "boo boo three", Models.NoteStatus.Open, testUser.Id));

            // test for archived note
            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId6, "boo boo six", Models.NoteStatus.Archived, testUser.Id));

            var testNote4 = CollaborationProjectFixture.BuildTestNote(testNoteId4, "boo boo four", Models.NoteStatus.Open, testUser.Id);
            var testNote4Reply1 = CollaborationProjectFixture.BuildTestNote(testNoteId4Reply1, "boo boo four reply 1", Models.NoteStatus.Open, testUser.Id);
            testNote4.ThreadId = testNoteId4;
            testNote4Reply1.ThreadId = testNoteId4;
            _fixture.Notes.Add(testNote4);
            _fixture.Notes.Add(testNote4Reply1);

            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId5, "boo boo five", Models.NoteStatus.Open, testUser.Id));

            
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpus.Id, "001001001001001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpus.Id, "001001001002001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId2, testTokenizedCorpus.Id, "001001001005001-001001001006001-001001001008001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId3, testTokenizedCorpus.Id, "001001001003001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId3, testTokenizedCorpus.Id, "001001001004001", builderContext));

            _fixture.NoteUserSeenAssociations.Add(CollaborationProjectFixture.BuildTestNoteUserSeenAssociation(testNoteId4, _fixture.Users.First().Id, builderContext));
            _fixture.NoteUserSeenAssociations.Add(CollaborationProjectFixture.BuildTestNoteUserSeenAssociation(testNoteId4Reply1, _fixture.Users.First().Id, builderContext));
            foreach (var user in _fixture.Users)
            {
                _fixture.NoteUserSeenAssociations.Add(CollaborationProjectFixture.BuildTestNoteUserSeenAssociation(testNoteId5, user.Id, builderContext));
            }

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(7, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(5, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Equal(1 + 1 + _fixture.Users.Count, _fixture.ProjectDbContext.NoteUserSeenAssociations.Count());

            var testNote4Thread = _fixture.ProjectDbContext.Notes.Include(e => e.NoteUserSeenAssociations).Where(e => e.ThreadId == testNoteId4).ToList();
            Assert.Equal(2, testNote4Thread.Count);
            Assert.True(testNote4Thread.All(e => e.NoteUserSeenAssociations.Count() == 1));
            Assert.True(testNote4Thread.All(e => e.NoteUserSeenAssociations.First().UserId == _fixture.Users.First().Id));

            var testNote1TokenIds = _fixture.ProjectDbContext.NoteDomainEntityAssociations
                .Where(e => e.NoteId == testNoteId1)
                .Where(e => e.DomainEntityIdName!.Contains("TokenId"))
                .Select(e => e.DomainEntityIdGuid!)
                .ToArray();

            Assert.Equal(2, testNote1TokenIds.Length);

            var associatedTokens1 = _fixture.ProjectDbContext.Tokens
                .Where(e => testNote1TokenIds.Contains(e.Id))
                .ToArray();

            Assert.Equal(2, associatedTokens1.Length);

            var testNote2TokenIds = _fixture.ProjectDbContext.NoteDomainEntityAssociations
                .Where(e => e.NoteId == testNoteId2)
                .Where(e => e.DomainEntityIdName!.Contains("TokenId"))
                .Select(e => e.DomainEntityIdGuid!)
                .ToArray();

            Assert.Single(testNote2TokenIds);

            var associatedTokens2 = _fixture.ProjectDbContext.TokenComposites
                .Where(e => testNote2TokenIds.Contains(e.Id))
                .ToArray();

            Assert.Single(associatedTokens2);

            var associatedToCompositeTokens2 = _fixture.ProjectDbContext.TokenCompositeTokenAssociations
                .Where(e => e.TokenCompositeId == testNote2TokenIds[0]!.Value)
                .Select(e => e.Token)
                .ToArray();

            Assert.Equal(3, associatedToCompositeTokens2.Length);

            var testNote3TokenIds = _fixture.ProjectDbContext.NoteDomainEntityAssociations
                .Where(e => e.NoteId == testNoteId3)
                .Where(e => e.DomainEntityIdName!.Contains("TokenId"))
                .Select(e => e.DomainEntityIdGuid!)
                .ToArray();

            Assert.Equal(2, testNote3TokenIds.Length);

            var associatedTokens3 = _fixture.ProjectDbContext.Tokens
                .Where(e => testNote3TokenIds.Contains(e.Id))
                .ToArray();

            Assert.Equal(2, associatedTokens3.Length);

            // verify archived note
            var archivedNote = _fixture.ProjectDbContext.Notes.FirstOrDefault(e => e.Id == testNoteId6);
            Assert.Equal(NoteStatus.Archived, archivedNote.NoteStatus);
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test05()
        {
            // Add a label 

            var testNotes = _fixture.Notes.Take(3).ToArray();
            Assert.Equal(3, testNotes.Length);

            var testLabelId1 = Guid.NewGuid();
            var testLabel1 = new Models.Label { Id = testLabelId1, Text = "test label 1" };

            var testLabelId2 = Guid.NewGuid();
            var testLabel2 = new Models.Label { Id = testLabelId2, Text = "test label 2" };

            var testLabelId3 = Guid.NewGuid();
            var testLabel3 = new Models.Label { Id = testLabelId3, Text = "test label 3" };

            _fixture.Labels.Add(testLabel1);
            _fixture.Labels.Add(testLabel2);
            _fixture.Labels.Add(testLabel3);

            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), Label = testLabel1, LabelId = testLabelId1, NoteId = testNotes[0].Id });
            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), Label = testLabel1, LabelId = testLabelId1, NoteId = testNotes[1].Id });
            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), Label = testLabel2, LabelId = testLabelId2, NoteId = testNotes[1].Id });
            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), Label = testLabel3, LabelId = testLabelId3, NoteId = testNotes[1].Id });
            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), Label = testLabel3, LabelId = testLabelId3, NoteId = testNotes[2].Id });

            var testLabelGroupId1 = Guid.NewGuid();
            var testLabelGroup1 = new Models.LabelGroup { Id = testLabelGroupId1, Name = "test label group 1" };

            var testLabelGroupId2 = Guid.NewGuid();
            var testLabelGroup2 = new Models.LabelGroup { Id = testLabelGroupId2, Name = "test label group 2" };

            _fixture.LabelGroups.Add(testLabelGroup1);
            _fixture.LabelGroups.Add(testLabelGroup2);

            _fixture.LabelGroupAssociations.Add(new Models.LabelGroupAssociation { Id = Guid.NewGuid(), Label = testLabel1, LabelId = testLabelId1, LabelGroup = testLabelGroup1, LabelGroupId = testLabelGroupId1 });
            _fixture.LabelGroupAssociations.Add(new Models.LabelGroupAssociation { Id = Guid.NewGuid(), Label = testLabel1, LabelId = testLabelId1, LabelGroup = testLabelGroup2, LabelGroupId = testLabelGroupId2 });
            _fixture.LabelGroupAssociations.Add(new Models.LabelGroupAssociation { Id = Guid.NewGuid(), Label = testLabel2, LabelId = testLabelId2, LabelGroup = testLabelGroup2, LabelGroupId = testLabelGroupId2 });

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(3, _fixture.ProjectDbContext.Labels.Count());
            Assert.Equal(5, _fixture.ProjectDbContext.LabelNoteAssociations.Count());

            Assert.Single(_fixture.ProjectDbContext.LabelNoteAssociations
                .Where(e => e.NoteId == testNotes[0].Id));

            Assert.Equal(3, _fixture.ProjectDbContext.LabelNoteAssociations
                .Where(e => e.NoteId == testNotes[1].Id).Count());

            Assert.Equal(2, _fixture.ProjectDbContext.LabelGroups.Count());
            Assert.Equal(3, _fixture.ProjectDbContext.LabelGroupAssociations.Count());

            Assert.Equal(2, _fixture.ProjectDbContext.LabelGroupAssociations.Include(e => e.Label)
                .Where(e => e.Label!.Text == testLabel1.Text).Count());

            Assert.Single(_fixture.ProjectDbContext.LabelGroupAssociations.Include(e => e.Label)
                .Where(e => e.Label!.Text == testLabel2.Text));
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test06()
        {
            // Remove a note association
            var testNote3 = _fixture.Notes.Where(e => e.Text!.Contains("three")).FirstOrDefault();
            Assert.NotNull(testNote3);

            var testNote3AssociationToRemove = _fixture.NoteAssociations.Where(e => e.Item1.NoteId == testNote3.Id).First();
            _fixture.NoteAssociations.RemoveAll(e => e.Item1.Id == testNote3AssociationToRemove.Item1.Id);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(6, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(4, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Null(_fixture.ProjectDbContext.NoteDomainEntityAssociations.Where(e => e.Id == testNote3AssociationToRemove.Item1.Id).FirstOrDefault());

            // Remove a note
            _fixture.Notes.RemoveAll(e => e.Id == testNote3.Id);
            _fixture.LabelNoteAssociations.RemoveAll(e => e.NoteId == testNote3.Id);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(5, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(3, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Null(_fixture.ProjectDbContext.Notes.Where(e => e.Id == testNote3.Id).FirstOrDefault());

            _fixture.NoteAssociations.RemoveAll(e => e.Item1.NoteId == testNote3.Id);
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test07()
        {
            var testNote2 = _fixture.Notes.Where(e => e.Text!.Contains("two")).FirstOrDefault();
            Assert.NotNull(testNote2);
            _fixture.Notes.RemoveAll(e => e.Id == testNote2.Id);
            _fixture.LabelNoteAssociations.RemoveAll(e => e.NoteId == testNote2.Id);

            var testNote2AssociationToRemove = _fixture.NoteAssociations.Where(e => e.Item1.NoteId == testNote2.Id).First();
            _fixture.NoteAssociations.RemoveAll(e => e.Item1.Id == testNote2AssociationToRemove.Item1.Id);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(4, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Null(_fixture.ProjectDbContext.Notes.Where(e => e.Id == testNote2.Id).FirstOrDefault());

        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test08()
        {
            var newNoteText = "new text in old note";

            var testNote1 = _fixture.Notes.FirstOrDefault();
            Assert.NotNull(testNote1);

            testNote1.Text = newNoteText;

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(4, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(newNoteText, _fixture.ProjectDbContext.Notes.First().Text);
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test09()
        {
            var newTranslationText = "translation change in lexicon";

            var meaning = _fixture.LexiconLexemes.First().Meanings.Where(e => e.Text == "lemma1Meaning1").First();
            var testTranslation1 = meaning.Translations.First();
            Assert.NotNull(testTranslation1);

            testTranslation1.Text = newTranslationText;

            Assert.Empty(_fixture.ProjectDbContext.Lexicon_Meanings
                .Include(e => e.SemanticDomains)
                .First(e => e.Text == "lemma1Meaning1").SemanticDomains);

            var semanticDomain = _fixture.LexiconSemanticDomains.Where(e => e.Text == "sd2").First();
            _ = CollaborationProjectFixture.BuildTestLexiconSemanticDomainMeaningAssociation(semanticDomain, meaning, meaning.UserId);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            var trDb = _fixture.ProjectDbContext.Lexicon_Translations.Where(e => e.Text == testTranslation1.Text).FirstOrDefault();
            Assert.NotNull(trDb);
            Assert.Equal(newTranslationText, trDb.Text);
            Assert.Single(_fixture.ProjectDbContext.Lexicon_Meanings
                .Include(e => e.SemanticDomains)
                .First(e => e.Text == "lemma1Meaning1").SemanticDomains);
            Assert.Equal(4, _fixture.ProjectDbContext.Lexicon_SemanticDomainMeaningAssociations.Count());
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test10()
        {
            var testTokenizedCorpus = _fixture.ProjectDbContext.TokenizedCorpora
                .Where(e => e.DisplayName == "test tokenized corpus one")
                .FirstOrDefault();
            Assert.NotNull(testTokenizedCorpus);

            await _fixture.ChangeProjectData(async (ProjectDbContext dbContext, CancellationToken cancellationToken) => {

                // These represent changes to the 'target' database that were done outside of collaboration

                var tokenizedCorpus = dbContext.TokenizedCorpora
                    .Where(e => e.Id == testTokenizedCorpus.Id)
                    .FirstOrDefault();
                Assert.NotNull(tokenizedCorpus);

                var verseRow = CollaborationProjectFixture.BuildTestVerseRow(Guid.NewGuid(), tokenizedCorpus.Id, "001001020", "splitSource other0 other1 other2 other3 other4 other5 other6", tokenizedCorpus.UserId);

                // Token that was split (target system):
                var splitSource1 = CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020001001", "splitSource", null, verseRow, DateTimeOffset.Now);

                // Tokens/composite that result from split (target system):

                var splitComposite1 = CollaborationProjectFixture.BuildTestTokenComposite(
                    tokenizedCorpus,
                    null,
                    "001001020001001-001001020001002-001001020001003-001001020001004",
                    "spl_itSou_rce_other0",
                    "001001020001001-001001020001001-001001020001001-001001020001002",
                    verseRow);

                // Additional tokens in same word and/or verse (target system):
                var extraTokens1 = new Models.Token[]
                {
                    CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020001005", "other1", "001001020001003", verseRow),
                    CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020001006", "other2", "001001020001004", verseRow),
                    CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020002001", "other3", null, verseRow),
                    CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020002002", "other4", null, verseRow),
                    CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020003001", "other5", null, verseRow),
                    CollaborationProjectFixture.BuildTestToken(tokenizedCorpus, "001001020004001", "other6", null, verseRow)
                };

                // Need the database to first have some tokens:
                dbContext.Add(verseRow);
                dbContext.Add(splitSource1);
                dbContext.Add(splitComposite1);
                dbContext.AddRange(extraTokens1);

                await dbContext.SaveChangesAsync(cancellationToken);

            }, CancellationToken.None);

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            var verseRow = _fixture.ProjectDbContext.VerseRows
                .Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id)
                .Where(e => e.BookChapterVerse == "001001020")
                .FirstOrDefault();

            Assert.Equal(3, _fixture.ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id).Count());
            Assert.Equal(34, _fixture.ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id).Count());

            // Token that was split (source system - to be merged into target):
            var splitSource2 = CollaborationProjectFixture.BuildTestToken(testTokenizedCorpus, "001001020001001", "splitSource", null, verseRow, DateTimeOffset.Now);

            // Tokens/composite that result from split (source system - to be merged into target):
            var splitComposite2 = CollaborationProjectFixture.BuildTestTokenComposite(
                testTokenizedCorpus,
                null,
                "001001020001001-001001020001002",
                "split_Source",
                "001001020001001-001001020001001",
                verseRow);

            // Additional tokens/composite in same word and/or verse (source system - to be merged into target):
            var extraComposite2 = CollaborationProjectFixture.BuildTestTokenComposite(
                testTokenizedCorpus,
                null,
                "001001020001004-001001020001005-001001020004001",
                "other1_other2_other6",
                "001001020001003-001001020001004-",
                verseRow);

            // Additional tokens in same word and/or verse (source system - to be merged into target):
            var extraTokens2 = new Models.Token[]
            {
                CollaborationProjectFixture.BuildTestToken(testTokenizedCorpus, "001001020001003", "other0", "001001020001002", verseRow),
                CollaborationProjectFixture.BuildTestToken(testTokenizedCorpus, "001001020002001", "other3", null, verseRow),
                CollaborationProjectFixture.BuildTestToken(testTokenizedCorpus, "001001020002002", "other4", null, verseRow),
                CollaborationProjectFixture.BuildTestToken(testTokenizedCorpus, "001001020003001", "other5", null, verseRow)
            };

            _fixture.Tokens.Add(splitSource2);
            _fixture.Tokens.AddRange(splitComposite2.Tokens);
            _fixture.Tokens.AddRange(extraComposite2.Tokens);
            _fixture.Tokens.AddRange(extraTokens2);
            _fixture.TokenComposites.Add(splitComposite2);
            _fixture.TokenComposites.Add(extraComposite2);

            await DoMerge(true);

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(4, _fixture.ProjectDbContext.TokenComposites.Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id).Count());
            Assert.Equal(33, _fixture.ProjectDbContext.Tokens.Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id).Count());

            var splitCompositeDb = _fixture.ProjectDbContext.TokenComposites
                .Include(e => e.Tokens.OrderBy(t => t.EngineTokenId))
                .Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id)
                .Where(e => e.EngineTokenId == "001001020001001-001001020001002")
                .FirstOrDefault();

            var extraCompositeDb = _fixture.ProjectDbContext.TokenComposites
                .Include(e => e.Tokens.OrderBy(t => t.EngineTokenId))
                .Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id)
                .Where(e => e.EngineTokenId == "001001020001004-001001020001005-001001020004001")
                .FirstOrDefault();

            Assert.NotNull(splitCompositeDb);
            Assert.Equal("split_Source", splitCompositeDb.SurfaceText);

            Assert.NotNull(extraCompositeDb);
            Assert.Equal("other1_other2_other6", extraCompositeDb.SurfaceText);

            var tokensWord1Db = _fixture.ProjectDbContext.Tokens
                .Where(e => e.TokenizedCorpusId == testTokenizedCorpus.Id)
                .Where(e => e.Deleted == null)
                .Where(e => e.BookNumber == 1)
                .Where(e => e.ChapterNumber == 1)
                .Where(e => e.VerseNumber == 20)
                .Where(e => e.WordNumber == 1)
                .OrderBy(e => e.SubwordNumber)
                .ToArray();

            Assert.Equal(5, tokensWord1Db.Length);
            Assert.Equal(1, tokensWord1Db[0].SubwordNumber);
            Assert.Equal(2, tokensWord1Db[1].SubwordNumber);
            Assert.Equal(3, tokensWord1Db[2].SubwordNumber);
            Assert.Equal(4, tokensWord1Db[3].SubwordNumber);
            Assert.Equal(5, tokensWord1Db[4].SubwordNumber);

            Assert.Equal("001001020001001", splitCompositeDb.Tokens.First().OriginTokenLocation);
            Assert.Equal("001001020001001", splitCompositeDb.Tokens.Last().OriginTokenLocation);

            var extraCompositeTokensDb = extraCompositeDb.Tokens.ToArray();
            Assert.Equal(3, extraCompositeTokensDb.Length);
            Assert.Equal("001001020001004", extraCompositeTokensDb[0].EngineTokenId);
            Assert.Equal("001001020001005", extraCompositeTokensDb[1].EngineTokenId);
            Assert.Equal("001001020004001", extraCompositeTokensDb[2].EngineTokenId);
        }

        protected async Task DoMerge(bool isIt = false, ProjectSnapshot? sourceSnapshot = null, ProjectSnapshot? previousSnapshot = null)
        {
            var testProject = _fixture.ProjectDbContext.Projects.First();

            var shaIndex = 1;
            if (!string.IsNullOrEmpty(testProject.LastMergedCommitSha))
            {
                var result = Regex.Match(testProject.LastMergedCommitSha, @"\d+$", RegexOptions.RightToLeft);
                if (result.Success)
                {
                    shaIndex = int.Parse(result.Value) + 1;
                }
            }
                
            var commitShaToMerge = $"{CollaborationProjectFixture.ShaBase}_{shaIndex}";

            var snapshotLastMerged = previousSnapshot ?? _fixture.ProjectSnapshotLastMerged ?? ProjectSnapshotFactoryCommon.BuildEmptySnapshot(testProject.Id);
            var snapshotToMerge = sourceSnapshot ?? _fixture.ToProjectSnapshot();
            var progress = new Progress<ProgressStatus>(Report);

            if (isIt)
            {
                var backupsPath =
                    FilePathTemplates.CollabBaseDirectory + Path.DirectorySeparatorChar + "Backups";

                Directory.CreateDirectory(backupsPath);

                var factory = new ProjectSnapshotFilesFactory(Path.Combine(backupsPath, "merge_test_final_snapshot"), _fixture.Logger);
                factory.SaveSnapshot(snapshotToMerge);
            }

            await _fixture.MergeIntoDatabase(commitShaToMerge, snapshotLastMerged, snapshotToMerge, progress);
        }

        protected void Report(ProgressStatus status)
        {
            var message = Regex.Replace(status.Message ?? string.Empty, "{PercentCompleted(:.*)?}", "{0$1}");
            var description = Regex.IsMatch(message, "{0(:.*)?}") ?
                string.Format(message, status.PercentCompleted) :
                message;

            _output.WriteLine(description);
        }

    }
}
