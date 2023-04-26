using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClearBible.Engine.Corpora;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.DAL.Alignment.Features;
using ClearBible.Engine.Utils;

//USE TO ACCESS Models
using Models = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DAL.Alignment.Translation;
using System.Diagnostics.Metrics;
using System.CodeDom;
using Microsoft.EntityFrameworkCore;
using ClearDashboard.DAL.Alignment.Exceptions;
using SIL.Machine.Translation;
using ClearBible.Engine.SyntaxTree.Aligner.Legacy;
using ClearDashboard.DAL.Alignment.BackgroundServices;
using Autofac;
using System.Threading;
using static ClearBible.Engine.Persistence.FileGetBookIds;
using ClearDashboard.DAL.CQRS;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Configuration;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Collaboration.Factory;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using SIL.Machine.Utils;
using ClearDashboard.Collaboration;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Model;
using System.ComponentModel.DataAnnotations.Schema;
using ClearDashboard.DataAccessLayer.Models;
using SIL.Scripture;
using System.IO;
using Versification = SIL.Scripture.Versification;
using static ClearDashboard.DAL.Alignment.Notes.EntityContextKeys;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Merge;

namespace ClearDashboard.DAL.Alignment.Tests.Corpora.HandlerTests;


public class CollaborationTests : TestBase
{
    public CollaborationTests(ITestOutputHelper output) : base(output)
    {
    }


    [Fact]
    public async void ConfigurationTest()
    {
        try
        {
            var collaborationManager = Container!.Resolve<CollaborationManager>();
            
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    [Fact]
    public async Task TestInitialLoad()
    {
        try
        {
            await LoadInitialTestProjectData();

            // VerseRow original text change
            // Add VerseRows from new book
            // Corpus add/delete/modify
            // TokenizedCorpus add/delete/modify
            // Convert tokens to cpomosite
            // Convert composite to tokens
            // 
        }
        finally
        {
            await DeleteDatabaseContext();
        }
    }

    protected async Task LoadInitialTestProjectData()
    {
        var builderContext = new BuilderContext(ProjectDbContext!);

        var testUserId1 = Guid.NewGuid();
        var testUserId2 = Guid.NewGuid();

        //            var testProject = BuildTestProject(Guid.NewGuid(), "projectA", null, testUserId1);
        var testProject = ProjectDbContext!.Projects.First();

        var userBuilder = new UserBuilder
        {
            GetUsers = (projectDbContext) =>
            {
                return new List<Models.User>
                    {
                        BuildTestUser(testUserId1, "tester", "one"),
                        BuildTestUser(testUserId2, "tester", "two")
                    };
            }
        };

        var testCorpus1 = BuildTestCorpus(Guid.NewGuid(), "test corpus 1", "test corpus one", CorpusType.Standard, testUserId1);
        var testCorpus2 = BuildTestCorpus(Guid.NewGuid(), "test corpus 2", "test corpus two", CorpusType.ManuscriptGreek, testUserId1);
        var corpusBuilder = new CorpusBuilder
        {
            GetCorpora = (projectDbContext) =>
            {
                return new List<Models.Corpus>
                    {
                        testCorpus1,
                        testCorpus2
                    };
            }
        };

        var testTokenizedCorpusId1 = Guid.NewGuid();
        var testTokenizedCorpusId2 = Guid.NewGuid();
        var testTokenizedCorpora = new List<Models.TokenizedCorpus>
        {
            BuildTestTokenizedCorpus(testTokenizedCorpusId1, testCorpus1, "test tokenized corpus one", testUserId1),
            BuildTestTokenizedCorpus(testTokenizedCorpusId2, testCorpus2, "test tokenized corpus two", testUserId1)
        };
        var testBookNumbersByTc = new Dictionary<Guid, IEnumerable<string>>
        {
            {testTokenizedCorpusId1, new GeneralListModel<string> { "001", "002" } },
            {testTokenizedCorpusId2, new GeneralListModel<string> { } }
        };
        var testVerseRowsByTc = new Dictionary<Guid, IEnumerable<Models.VerseRow>>
        {
            {testTokenizedCorpusId1, new List<Models.VerseRow> {
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpusId1, "001001001", "yes, the worms did eat my food", testUserId1),
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpusId1, "001001002", "my hovercraft is full of eels", testUserId1),
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpusId1, "001001003", "boo!", testUserId1)
            }},
            {testTokenizedCorpusId2, new List<Models.VerseRow> { } }
        };
        var testTokenCompositesByTc = new Dictionary<Guid, IEnumerable<(Models.TokenComposite composite, IEnumerable<Models.Token>)>>
        {
            {testTokenizedCorpusId1, new List<(Models.TokenComposite composite, IEnumerable<Models.Token>)> { }},
            {testTokenizedCorpusId2, new List<(Models.TokenComposite composite, IEnumerable<Models.Token>)> { }}
        };

        var tokenizedCorpusBuilder = BuildTokenizedCorpusBuilder(testTokenizedCorpora, testBookNumbersByTc, testVerseRowsByTc, testTokenCompositesByTc);

        var testNoteId1 = Guid.NewGuid();
        var testNoteId2 = Guid.NewGuid();
        var testNotes = new List<Models.Note>
        {
            BuildTestNote(testNoteId1, "boo boo", NoteStatus.Open, testUserId2),
            BuildTestNote(testNoteId2, "boo boo two", NoteStatus.Resolved, testUserId1)
        };

        var noteAssociations = new List<(Models.NoteDomainEntityAssociation nd, NoteModelRef noteModelRef)>
        {
            BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpusId1, "001001001001001", builderContext),
            BuildTestNoteTokenAssociation(testNoteId1, testTokenizedCorpusId1, "001001001002001", builderContext)
        };

        var noteBuilder = BuildNoteBuilder(
            testNotes,
            new Dictionary<Guid, IEnumerable<Models.Note>>(),
            noteAssociations);

        var projectSnapshot = new ProjectSnapshot(ProjectBuilder.BuildModelSnapshot(testProject));
        projectSnapshot.AddGeneralModelList(userBuilder.BuildModelSnapshots(builderContext));
        projectSnapshot.AddGeneralModelList(corpusBuilder.BuildModelSnapshots(builderContext));
        projectSnapshot.AddGeneralModelList(tokenizedCorpusBuilder.BuildModelSnapshots(builderContext));
        projectSnapshot.AddGeneralModelList(noteBuilder.BuildModelSnapshots(builderContext));

        var emptySnapshot = ProjectSnapshotFactoryCommon.BuildEmptySnapshot(testProject.Id);

        var commitShaToMerge = "shasha";
        var command = new MergeProjectSnapshotCommand(
            commitShaToMerge,
            emptySnapshot,
            projectSnapshot,
            true,
            false,
            new Progress<ProgressStatus>());
        var result = await Mediator!.Send(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Success);

        ProjectDbContext!.ChangeTracker.Clear();
        Assert.Equal(commitShaToMerge, ProjectDbContext!.Projects.First().LastMergedCommitSha);
    }

    protected static Models.User BuildTestUser(Guid id, string firstName, string lastName)
    {
        var user = new Models.User()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            LicenseKey = "12345"
        };

        return user;
    }

    protected static Models.Project BuildTestProject(Guid id, string projectName, string? lastMergedCommitSha, Guid userId)
    {
        var project = new Models.Project()
        {
            Id = id,
            ProjectName = projectName,
            IsRtl = false,
            DesignSurfaceLayout = null,
            WindowTabLayout = null,
            AppVersion = "1.0.0.0",
            LastMergedCommitSha = lastMergedCommitSha,
            Created = DateTimeOffset.UtcNow,
            UserId = userId
        };

        return project;
    }

    protected static Models.Corpus BuildTestCorpus(Guid id, string name, string? displayName, CorpusType corpusType, Guid userId)
    {
        var corpus = new Models.Corpus()
        {
            Id = id,
            IsRtl = true,
            FontFamily = "test font family",
            Name = name,
            DisplayName = displayName,
            Language = "test language",
            ParatextGuid = "123456789",
            CorpusType = corpusType,
            Created = DateTimeOffset.UtcNow,
            UserId = userId
        };

        return corpus;
    }

    protected static Models.TokenizedCorpus BuildTestTokenizedCorpus(Guid id, Models.Corpus corpus, string? displayName, Guid userId)
    {
        var tokenizedCorpus = new Models.TokenizedCorpus()
        {
            Id = id,
            CorpusId = corpus.Id,
            Corpus = corpus,
            DisplayName = displayName,
            TokenizationFunction = "LatinWordTokenizer",
            Metadata = new(),
            LastTokenized = DateTimeOffset.UtcNow,
            Created = DateTimeOffset.UtcNow,
            UserId = userId
        };

        string customVersificationAddition = "&MAT 1:2 = MAT 1:1\nMAT 1:3 = MAT 1:2\nMAT 1:1 = MAT 1:3\n";
        ScrVers versification = ScrVers.RussianOrthodox;
        using (var reader = new StringReader(customVersificationAddition))
        {
            Versification.Table.Implementation.RemoveAllUnknownVersifications();
            versification = Versification.Table.Implementation.Load(reader, "not a file", versification, "custom");
        }

        tokenizedCorpus.ScrVersType = (int)versification.BaseVersification.Type;
        using (var writer = new StringWriter())
        {
            versification.Save(writer);
            tokenizedCorpus.CustomVersData = writer.ToString();
        }

        return tokenizedCorpus;
    }

    protected static TokenizedCorpusBuilder BuildTokenizedCorpusBuilder(
        IEnumerable<Models.TokenizedCorpus> tokenizedCorpora, 
        Dictionary<Guid, IEnumerable<string>> bookNumbersByTc,
        Dictionary<Guid, IEnumerable<Models.VerseRow>> verseRowsByTc,
        Dictionary<Guid, IEnumerable<(Models.TokenComposite composite, IEnumerable<Models.Token>)>> tokenCompositesByTc)
    {
        var tokenizedCorpusBuilder = new TokenizedCorpusBuilder
        {
            GetTokenizedCorpora = (projectDbContext) =>
            {
                return tokenizedCorpora;
            },
            GetBookNumbers = (projectDbContext, tokenizedCorpusId) =>
            {
                if (bookNumbersByTc.TryGetValue(tokenizedCorpusId, out var bookNumbers))
                {
                    return bookNumbers.ToGeneralListModel();
                }
                else
                {
                    return new GeneralListModel<string>();
                }
            },
            VerseRowBuilder = new VerseRowBuilder
            {
                GetVerseRows = (projectDbContext, tokenizedCorpusId) =>
                {
                    if (verseRowsByTc.TryGetValue(tokenizedCorpusId, out var verseRows))
                    {
                        return verseRows;
                    }
                    else
                    {
                        return Enumerable.Empty<Models.VerseRow>();
                    }
                }
            },
            TokenBuilder = new TokenBuilder()
            {
                GetTokenizedCorpusCompositeTokens = (projectDbContext, tokenizedCorpusId) =>
                {
                    if (tokenCompositesByTc.TryGetValue(tokenizedCorpusId, out var tokenComposites))
                    {
                        return tokenComposites;
                    }
                    else
                    {
                        return Enumerable.Empty<(Models.TokenComposite, IEnumerable<Models.Token>)>();
                    }
                },
                GetParallelCorpusCompositeTokens = (projectDbContext, parallelCorpusId) =>
                {
                    return Enumerable.Empty<(Models.TokenComposite, IEnumerable<Models.Token>)>();
                }
            }
        };

        return tokenizedCorpusBuilder;
    }

    protected static Models.VerseRow BuildTestVerseRow(Guid id, Guid tokenizedCorpusId, string bookChapterVerse, string originalText, Guid userId)
    {
        return new Models.VerseRow
        {
            Id = id,
            BookChapterVerse = bookChapterVerse,
            OriginalText = originalText,
            IsSentenceStart = true,
            IsInRange = false,
            IsRangeStart = false,
            IsEmpty = false,
            TokenizedCorpusId = tokenizedCorpusId,
            Created = DateTimeOffset.UtcNow,
            Modified = null,
            UserId = userId
        };
    }

    protected static Models.Note BuildTestNote(Guid id, string text, NoteStatus noteStatus, Guid userId)
    {
        return new Models.Note
        {
            Id = id,
            Text = text,
            AbbreviatedText = null,
            ThreadId = null,
            NoteStatus = noteStatus,
            UserId = userId
        };
    }

    protected static (Models.NoteDomainEntityAssociation, NoteModelRef) BuildTestNoteTokenAssociation(Guid noteId, Guid tokenizedCorpusId, string tokenLocation, BuilderContext builderContext)
    {
        var nd = new Models.NoteDomainEntityAssociation
        {
            Id = Guid.NewGuid(),
            NoteId = noteId,
            DomainEntityIdGuid = Guid.NewGuid(),
            DomainEntityIdName = "ClearBible.Engine.Utils.EntityId`1[[ClearBible.Engine.Corpora.TokenId, ClearBible.Engine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], ClearBible.Engine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
        };

        var noteModelRef = new NoteModelRef<TokenRef>
        (
            nd.Id,
            nd.NoteId,
            TokenBuilder.BuildTokenRef(new Models.Token { TokenizedCorpusId = tokenizedCorpusId, EngineTokenId = tokenLocation }, builderContext)
        );

        return (nd, noteModelRef);
    }

    protected static NoteBuilder BuildNoteBuilder(
        IEnumerable<Models.Note> notes, 
        Dictionary<Guid, IEnumerable<Models.Note>> repliesByThread, 
        IEnumerable<(Models.NoteDomainEntityAssociation nd, NoteModelRef noteModelRef)> noteAssociations)
    {
        var noteBuilder = new NoteBuilder
        {
            GetNotes = (projectDbContext) =>
            {
                return notes;
            },
            GetRepliesByThreadId = (projectDbContext) =>
            {
                return repliesByThread;
            },
            GetNoteDomainEntityAssociationsByNoteId = (projectDbContext) =>
            {
                return noteAssociations
                    .Select(e => e.nd)
                    .GroupBy(e => e.NoteId)
                    .Select(g => new
                    {
                        NoteId = g.Key,
                        DomainEntityTypes = g.ToList()
                            .GroupBy(gg => gg.DomainEntityIdName!)
                            .ToDictionary(gg => gg.Key, gg => gg.Select(e => e))
                    })
                    .ToDictionary(g => g.NoteId, g => g.DomainEntityTypes);
            },
            ExtractNoteModelRefs = (nda, builderContext) =>
            {
                return new GeneralListModel<NoteModelRef>(noteAssociations.Select(e => e.noteModelRef));
            }
        };

        return noteBuilder;
    }

    //[Fact]
    protected async Task HardResetLocalChanges()
    {
        var collaborationManager = Container!.Resolve<CollaborationManager>();
        collaborationManager.HardResetChanges();

        await Task.CompletedTask;
    }

    //[Fact]
    protected async Task ListProjectFileStatuses()
    {
        var collaborationManager = Container!.Resolve<CollaborationManager>();

        var projects = collaborationManager.GetAllProjects();

        if (projects.Any())
        {
            foreach (var status in collaborationManager.RetrieveFileStatuses(projects.First().projectId))
            {
                Output.WriteLine(status);
            }
        }

        await Task.CompletedTask;
    }

    //[Fact]
    protected async Task GetProjectsFromServer()
    {
        var collaborationManager = Container!.Resolve<CollaborationManager>();
        collaborationManager.InitializeRepository();
        collaborationManager.FetchMergeRemote();

        // Just to get project Ids and names:
        var projectIdsNames = ProjectSnapshotFromFilesFactory.FindProjectIdsNames(collaborationManager.RepositoryPath);

        foreach (var (projectId, projectName) in projectIdsNames)
        {
            // Run initialize to create each project database (with project and
            // user entities)
            await collaborationManager.InitializeProjectDatabaseAsync(projectId, true, default, new Progress<ProgressStatus>());
        }
    }

    //[Fact]
    protected async Task InitializeServerWithCurrentProject()
    {
        var collaborationManager = Container!.Resolve<CollaborationManager>();
        collaborationManager.InitializeRepository();
        collaborationManager.FetchMergeRemote();

        var progress = new Progress<ProgressStatus>();
        await collaborationManager.StageProjectChangesAsync(default, progress);

        collaborationManager.CommitChanges("[some commit message]", progress);
        collaborationManager.PushChangesToRemote();
    }

    //[Fact]
    protected async Task GetCurrentProjectChangesFromServer()
    {
        bool remoteOverridesLocal = false;  // Configuration?  User choice?

        var projectProvider = Container!.Resolve<IProjectProvider>();
        var collaborationManager = Container!.Resolve<CollaborationManager>();

        // Pull down and merge HEAD into local git repository:
        collaborationManager.FetchMergeRemote();

        await collaborationManager.MergeProjectLatestChangesAsync(remoteOverridesLocal, false, default, new Progress<ProgressStatus>());
    }

    //[Fact]
    protected async Task CommitCurrentProjectChangesToServer()
    {
        var collaborationManager = Container!.Resolve<CollaborationManager>();

        var progress = new Progress<ProgressStatus>();
        await collaborationManager.StageProjectChangesAsync(default, progress);

        collaborationManager.CommitChanges("[some commit message]", progress);
        collaborationManager.PushChangesToRemote();
    }

    //[Fact]
    protected async Task GetProjectsChangesFromServer()
    {
        bool remoteOverridesLocal = false;  // Configuration?  User choice?

        var projectProvider = Container!.Resolve<IProjectProvider>();
        var collaborationManager = Container!.Resolve<CollaborationManager>();

        // Pull down and merge HEAD into local git repository:
        collaborationManager.FetchMergeRemote();

        // Just to get project Ids and names:
        var projectIdsNames = ProjectSnapshotFromFilesFactory.FindProjectIdsNames(collaborationManager.RepositoryPath);

        var previousProject = projectProvider.CurrentProject;
        var dbContextFactory = Container!.Resolve<ProjectDbContextFactory>();

        foreach (var (projectId, projectName) in projectIdsNames)
        {
            await using (var requestScope = dbContextFactory.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                // Is there a general utility for doing this sort of stuff outside of this text fixture?
                // Or do we need a command/handler for this?

                ProjectDbContext = await dbContextFactory!.GetDatabaseContext(
                    projectName,
                    true).ConfigureAwait(false);
                var project = ProjectDbContext.Projects.FirstOrDefault();
                projectProvider!.CurrentProject = project;

                await collaborationManager.MergeProjectLatestChangesAsync(remoteOverridesLocal, false, default, new Progress<ProgressStatus>());
            }
        }

        projectProvider.CurrentProject = previousProject;
    }
}