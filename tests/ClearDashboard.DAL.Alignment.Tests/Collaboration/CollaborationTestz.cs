using Models = ClearDashboard.DataAccessLayer.Models;
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
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.Collaboration.Factory;

namespace ClearDashboard.DAL.Alignment.Tests.Collaboration
{
    [TestCaseOrderer("ClearDashboard.DAL.Alignment.Tests.Collaboration.AlphabeticalOrderer", "ClearDashboard.DAL.Alignment.Tests")]
    public class CollaborationTestz : IClassFixture<CollaborationProjectFixture>
    {
        CollaborationProjectFixture _fixture;
        ITestOutputHelper _output;

        public CollaborationTestz(CollaborationProjectFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task Test1()
        {
            var testTokenizedCorpora = _fixture.TokenizedCorpora.Take(2).ToArray();
            Assert.Equal(2, testTokenizedCorpora.Length);

            var testUser = _fixture.Users.First();

            var testParallelCorpus = CollaborationProjectFixture.BuildTestParallelCorpus(
                testTokenizedCorpora[0], 
                testTokenizedCorpora[1], 
                testUser.Id);
            _fixture.ParallelCorpora.Add(testParallelCorpus);

            await DoMerge();
        }

        [Fact]
        public async Task Test2()
        {
            var testParallelCorpus = _fixture.ParallelCorpora.First();
            var testUser = _fixture.Users.Skip(1).First();

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
        }

        [Fact]
        public async Task Test3()
        {
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
        }

        [Fact]
        public async Task Test4()
        {
            var builderContext = new BuilderContext(_fixture.ProjectDbContext!);

            var testTokenizedCorpus = _fixture.TokenizedCorpora.First();
            var testUser = _fixture.Users.Skip(1).First();

            var testNoteId1 = Guid.NewGuid();
            var testNoteId2 = Guid.NewGuid();

            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId1, "boo boo", Models.NoteStatus.Open, testUser.Id));
            _fixture.Notes.Add(CollaborationProjectFixture.BuildTestNote(testNoteId2, "boo boo two", Models.NoteStatus.Resolved, testUser.Id));

            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpus.Id, "001001001001001", builderContext));
            _fixture.NoteAssociations.Add(CollaborationProjectFixture.BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpus.Id, "001001001002001", builderContext));

            await DoMerge();
        }

        [Fact]
        public async Task Test5()
        {
            var testNotes = _fixture.Notes.Take(2).ToArray();
            Assert.Equal(2, testNotes.Length);

            var testLabelId1 = Guid.NewGuid();

            _fixture.Labels.Add(new Models.Label { Id = testLabelId1, Text = "test label 1" });

            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), LabelId = testLabelId1, NoteId = testNotes[0].Id });
            _fixture.LabelNoteAssociations.Add(new Models.LabelNoteAssociation { Id = Guid.NewGuid(), LabelId = testLabelId1, NoteId = testNotes[1].Id });

            await DoMerge();
        }

        protected async Task DoMerge()
        {
            var testProject = _fixture.ProjectDbContext!.Projects.First();

            var lastSha = testProject.LastMergedCommitSha ?? "shasha";
            var snapshotLastMerged = _fixture.ProjectSnapshotLastMerged ?? ProjectSnapshotFactoryCommon.BuildEmptySnapshot(testProject.Id);
            var snapshotToMerge = _fixture.ToProjectSnapshot();
            var progress = new Progress<ProgressStatus>(Report);

            await _fixture.MergeIntoDatabase(lastSha + "1", snapshotLastMerged, snapshotToMerge, progress);
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
