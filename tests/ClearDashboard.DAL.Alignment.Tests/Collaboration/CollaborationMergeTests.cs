﻿using Models = ClearDashboard.DataAccessLayer.Models;
using SIL.Machine.FiniteState;
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
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Collaboration.Factory;
using SIL.Scripture;
using MediatR;
using ClearDashboard.DAL.Alignment.Features;
using System.Data.Entity;

namespace ClearDashboard.DAL.Alignment.Tests.Collaboration
{
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
        public async Task Test0()
        {
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

        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test1()
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
        public async Task Test2()
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
            var testTokenizedCorpusFromDb = _fixture.ProjectDbContext.TokenizedCorpora.Where(e => e.Id == testTokenizedCorpus.Id).FirstOrDefault();
            Assert.NotNull(testTokenizedCorpusFromDb);
            Assert.False(string.IsNullOrEmpty(testTokenizedCorpusFromDb.CustomVersData));
            Assert.NotEqual(beforeScrVersType, testTokenizedCorpusFromDb.ScrVersType);
            Assert.NotEqual(beforeCustomVersData, testTokenizedCorpusFromDb.CustomVersData);

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
        public async Task Test3()
        {
            // Add translation set and some translations

            var testAlignmentSet = _fixture.AlignmentSets.First();
            var testUser = _fixture.Users.First();

            var translationSet = CollaborationProjectFixture.BuildTestTranslationSet(testAlignmentSet, testUser.Id);
            _fixture.TranslationSets.Add(translationSet);

            _fixture.Translations.AddRange(CollaborationProjectFixture.BuildTestTranslations(
                translationSet,
                Models.TranslationOriginatedFrom.Assigned,
                testUser.Id,
                new List<(Models.TokenComponent, string)>
                {
                    (
                        CollaborationProjectFixture.BuildTestToken(testAlignmentSet.ParallelCorpus!.SourceTokenizedCorpus!, "001001001001001"),
                        "target text one"
                    ),
                    (
                        _fixture.TokenComposites.Where(e => e.EngineTokenId == "001001001002001-001001001003001").First(),
                        "target text two"
                    )
                }));

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, _fixture.ProjectDbContext.TranslationSets.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.Translations.Count());
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test4()
        {
            // Add some notes and associate with tokens

            var builderContext = new BuilderContext(_fixture.ProjectDbContext!);

            var testTokenizedCorpus = _fixture.TokenizedCorpora.First();
            var testUser = _fixture.Users.Skip(1).First();

            var testNoteId1 = Guid.NewGuid();
            var testNoteId2 = Guid.NewGuid();
            var testNoteId3 = Guid.NewGuid();

            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId1, "boo boo", Models.NoteStatus.Open, testUser.Id));
            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId2, "boo boo two", Models.NoteStatus.Resolved, testUser.Id));
            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId3, "boo boo three", Models.NoteStatus.Open, testUser.Id));

            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpus.Id, "001001001001001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpus.Id, "001001001002001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId2, testTokenizedCorpus.Id, "001001001005001-001001001006001-001001001008001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId3, testTokenizedCorpus.Id, "001001001003001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId3, testTokenizedCorpus.Id, "001001001004001", builderContext));

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(3, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(5, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());

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
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test5()
        {
            // Add a label 

            var testNotes = _fixture.Notes.Take(2).ToArray();
            Assert.Equal(2, testNotes.Length);

            var testLabelId1 = Guid.NewGuid();

            _fixture.Labels.Add(new Models.Label { Id = testLabelId1, Text = "test label 1" });

            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), LabelId = testLabelId1, NoteId = testNotes[0].Id });
            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), LabelId = testLabelId1, NoteId = testNotes[1].Id });

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, _fixture.ProjectDbContext.Labels.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.LabelNoteAssociations.Count());

            Assert.Single(_fixture.ProjectDbContext.LabelNoteAssociations
                .Where(e => e.NoteId == testNotes[0].Id));

            Assert.Single(_fixture.ProjectDbContext.LabelNoteAssociations
                .Where(e => e.NoteId == testNotes[1].Id));

        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test6()
        {
            // Remove a note association
            var testNote3 = _fixture.Notes.Where(e => e.Text!.Contains("three")).FirstOrDefault();
            Assert.NotNull(testNote3);

            var testNote3AssociationToRemove = _fixture.NoteAssociations.Where(e => e.Item1.NoteId == testNote3.Id).First();
            _fixture.NoteAssociations.RemoveAll(e => e.Item1.Id == testNote3AssociationToRemove.Item1.Id);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(3, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(4, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Null(_fixture.ProjectDbContext.NoteDomainEntityAssociations.Where(e => e.Id == testNote3AssociationToRemove.Item1.Id).FirstOrDefault());

            // Remove a note
            _fixture.Notes.RemoveAll(e => e.Id == testNote3.Id);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(2, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(3, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Null(_fixture.ProjectDbContext.Notes.Where(e => e.Id == testNote3.Id).FirstOrDefault());

            _fixture.NoteAssociations.RemoveAll(e => e.Item1.NoteId == testNote3.Id);
        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test7()
        {
            var testNote2 = _fixture.Notes.Where(e => e.Text!.Contains("two")).FirstOrDefault();
            Assert.NotNull(testNote2);
            _fixture.Notes.RemoveAll(e => e.Id == testNote2.Id);

            var testNote2AssociationToRemove = _fixture.NoteAssociations.Where(e => e.Item1.NoteId == testNote2.Id).First();
            _fixture.NoteAssociations.RemoveAll(e => e.Item1.Id == testNote2AssociationToRemove.Item1.Id);

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Equal(1, _fixture.ProjectDbContext.Notes.Count());
            Assert.Equal(2, _fixture.ProjectDbContext.NoteDomainEntityAssociations.Count());
            Assert.Null(_fixture.ProjectDbContext.Notes.Where(e => e.Id == testNote2.Id).FirstOrDefault());

        }

        [Fact]
        [Trait("Category", "Collaboration")]
        public async Task Test8()
        {
            var newNoteText = "new text in old note";

            var testNote1 = _fixture.Notes.FirstOrDefault();
            Assert.NotNull(testNote1);

            testNote1.Text = newNoteText;

            await DoMerge();

            _fixture.ProjectDbContext.ChangeTracker.Clear();

            Assert.Single(_fixture.ProjectDbContext.Notes);
            Assert.Equal(newNoteText, _fixture.ProjectDbContext.Notes.First().Text);
        }

        protected async Task DoMerge()
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

            var snapshotLastMerged = _fixture.ProjectSnapshotLastMerged ?? ProjectSnapshotFactoryCommon.BuildEmptySnapshot(testProject.Id);
            var snapshotToMerge = _fixture.ToProjectSnapshot();
            var progress = new Progress<ProgressStatus>(Report);

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