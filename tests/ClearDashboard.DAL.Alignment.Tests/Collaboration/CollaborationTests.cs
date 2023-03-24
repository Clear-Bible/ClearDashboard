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