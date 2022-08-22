using Microsoft.EntityFrameworkCore;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.DataAccessLayer.ViewModels;
using ClearDashboard.ParatextPlugin.CQRS.Features.User;
using MediatR;
using Microsoft.Extensions.Logging;
using MvvmHelpers;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
//using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ClearDashboard.DataAccessLayer
{


    public abstract class ProjectManager : IUserProvider, IProjectProvider, IProjectManager, IDisposable
    {
#nullable disable
        #region Properties

        protected ILogger Logger { get; private set; }
        protected ParatextProxy ParatextProxy { get; private set; }
        public ProjectDbContextFactory ProjectNameDbContextFactory { get; set; }
        public IMediator Mediator { get; private set; }

        public ProjectAssets ProjectAssets { get; set; }



        public User CurrentUser { get; set; }

        public Project CurrentProject { get; set; }
        public ParatextProject CurrentParatextProject { get; set; }
        public bool HasCurrentProject => CurrentProject != null;
        public bool HasCurrentParatextProject => CurrentParatextProject != null;


        public ObservableRangeCollection<ParatextProjectViewModel> ParatextProjects { get; set; } = new();

        public ObservableRangeCollection<ParatextProjectViewModel> ParatextResources { get; set; } = new();



        public bool ParatextVisible = false;
        public string ParatextUserName { get; set; } = "";
        private string _currentVerse;

        public string CurrentVerse
        {
            get => _currentVerse;
            set
            {
                // ensure that we are getting a fully delimited BBB as things like
                // 01 through 09 often get truncated to "1" through "9" without the 
                // leading zero
                var s = value;
                if (s.Length < "BBBCCCVVV".Length)
                {
                    s = value.PadLeft("BBBCCCVVV".Length, '0');
                }
                _currentVerse = s;
            }
        }

        #endregion

        #region Events

        #endregion

        #region Startup

        protected ProjectManager(IMediator mediator, ParatextProxy paratextProxy, ILogger<ProjectManager> logger, ProjectDbContextFactory projectNameDbContextFactory)
        {
            Logger = logger;
            ProjectNameDbContextFactory = projectNameDbContextFactory;
            ParatextProxy = paratextProxy;
            Logger.LogInformation("'ProjectManager' ctor called.");
            Mediator = mediator;

        }


        #endregion

        #region Methods

        protected abstract Task PublishParatextUser(User user);

        public virtual async Task Initialize()
        {
            CurrentUser = await GetUser();
            EnsureDashboardProjectDirectory();
        }

        private void EnsureDashboardProjectDirectory()
        {
            if (!Directory.Exists(FilePathTemplates.ProjectBaseDirectory))
            {
                // need to create that directory
                try
                {
                    Directory.CreateDirectory(FilePathTemplates.ProjectBaseDirectory);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex,
                        $"An unexpected error occurred while creating '{FilePathTemplates.ProjectBaseDirectory}");
                }
            }
        }


        /// <summary>
        /// Taken from LandingViewModel - most likely will be deprecated
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string GetJsonProjectName(string filePath)
        {
            string line;
            string project = "";

            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                if (line.ToLower().Contains("projectname\":"))
                {
                    project = line.Substring(line.IndexOf(':') + 1);
                    //remove the trailing comma
                    project = project.Substring(0, project.IndexOf(','));
                    // remove the double quotes
                    project = project.Replace("\"", "").Trim();
                    break;
                }
            }
            file.Close();

            return project;
        }

        public async Task SetupParatext()
        {
            // detect if Paratext is installed

            ParatextVisible = ParatextProxy.IsParatextInstalled();

            if (ParatextVisible)
            {
                // get all the Paratext Projects (Projects/Backtranslations)
                ParatextProjects.Clear();
                var projects = await ParatextProxy.GetParatextProjectsOrResources(ParatextProxy.FolderType.Projects);
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectViewModel>();
                    foreach (var project in projects)
                    {
                        ParatextProjects.Add(TinyMapper.Map<ParatextProjectViewModel>(project));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error while initializing");
                }

                // get all the Paratext Resources (LWC)
                ParatextResources.Clear();
                var resources = ParatextProxy.GetParatextResources();
                try
                {
                    TinyMapper.Bind<ParatextProject, ParatextProjectViewModel>();
                    foreach (var resource in resources)
                    {
                        ParatextResources.Add(TinyMapper.Map<ParatextProjectViewModel>(resource));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Unexpected error while initializing");
                }
            }
        }

        public async Task<User> GetUser()
        {
            // HACK:  Create a place holder User until we figure out
            //        how to manage Users holistically
            //        NOTE: every user will have the same GUID for now.

            var user = new User
            {
                Id = Guid.Parse("5649B1D2-2766-4C10-9274-F7E7BF75E2B7"),

            };

            var result = await ExecuteRequest(new GetCurrentParatextUserQuery(), CancellationToken.None);

            Logger.LogInformation(result.Success ? $"Found Paratext user - {result.Data.Name}" : $"GetParatextUserName - {result.Message}");


            if (result.Success && result.HasData)
            {
                var paratextUserName = result.Data.Name;
                user.ParatextUserName = paratextUserName;
                var userNameParts = paratextUserName.Split(" ");

                if (userNameParts.Length == 2)
                {
                    user.FirstName = userNameParts[0];
                    user.LastName = userNameParts[1];

                }
                await PublishParatextUser(user);
            }
            else
            {
                user.FirstName = string.Empty;
                user.LastName = string.Empty;
            }

            return user;
        }

        public DashboardProject CurrentDashboardProject { get; set; }

        public bool HasDashboardProject => CurrentDashboardProject != null;


        public DashboardProject CreateDashboardProject()
        {
            CurrentDashboardProject = new DashboardProject
            {
                ParatextUser = ParatextUserName,
                Created = DateTime.Now
            };

            return CurrentDashboardProject;
        }

        public async Task CreateNewProject(string projectName)
        {
            CreateDashboardProject();

            var projectAssets = await ProjectNameDbContextFactory.Get(projectName);

            CurrentProject = await CreateProject(projectName);

            CurrentDashboardProject.ProjectName = projectAssets.ProjectName;
            CurrentDashboardProject.DirectoryPath = projectAssets.ProjectDirectory;
            //CurrentDashboardProject.ParatextProjectPath = 
        }


        public async Task CreateNewProject(DashboardProject dashboardProject)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(dashboardProject.ProjectName);
                projectAssets.ProjectDbContext.Users.Add(CurrentUser);
                projectAssets.ProjectDbContext.Projects.Add(
                    new Project() { ProjectName = dashboardProject.ProjectName });
                await projectAssets.ProjectDbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
            // Populate Project table
            // Identify relationships
            //   1. Create ParallelCorpus per green line, which includes Corpus, getting back ParallelCorpusId and CorpaIds
            //   2.Manuscript to target (use ToDb.ManuscriptParatextParallelCorporaToDb)
            //      a. Get manuscript from Clear.Engine (SourceCorpus)
            //      b. Get Target from Paratext (TargetCorpus)
            //      c. Parallelize 
            //
            //      d.  Insert results into 

            //dashboardProject.TargetProject.
        }

        public void Dispose()
        {

        }


        #endregion

        #region Commands

        public Task<TResponse> ExecuteRequest<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            return Mediator.Send(request, cancellationToken);
        }
        #endregion


        public Task<IEnumerable<Project>> GetAllProjects()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Corpus>> LoadProject(string projectName)
        {
            var projectAssets = await ProjectNameDbContextFactory.Get(projectName);

            return EntityFrameworkQueryableExtensions.Include(projectAssets.ProjectDbContext.Corpa, corpus => corpus.TokenizedCorpora).ThenInclude(tokenizedCorpus=> tokenizedCorpus.Tokens);
           // return null;
        }

        public async Task<Project> DeleteProject(string projectName)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(projectName);

                if (projectAssets.ProjectDbContext != null)
                {
                    var project = projectAssets.ProjectDbContext.Projects.FirstOrDefault();

                    //projectAssets.ProjectDbContext!.Database.EnsureDeleted();
                    await projectAssets.ProjectDbContext!.Database.EnsureDeletedAsync();


                    if (Directory.Exists(projectAssets.ProjectDirectory))
                    {
                        Directory.Delete(projectAssets.ProjectDirectory, true);
                    }

                    return project;
                }

                throw new NullReferenceException($"The 'ProjectDbContext' for the project {projectName} could not be created.");

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{projectName}'");
                throw;
            }

        }

        public async Task<Project> CreateProject(string projectName)
        {
            try
            {
                var projectAssets = await ProjectNameDbContextFactory.Get(projectName);
               

                if (projectAssets.ProjectDbContext != null)
                {
                    var project = new Project()
                    {
                        ProjectName = projectName
                    };

                    try
                    {
                        await projectAssets.ProjectDbContext.Projects.AddAsync(project);
                        await projectAssets.ProjectDbContext.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                     
                        //var projects = projectAssets.ProjectDbContext.Projects.ToList() ?? throw new ArgumentNullException("projectAssets.ProjectDbContext.Projects.ToList()");
                        //projects.Add(project);
                        await projectAssets.ProjectDbContext.Projects.AddAsync(project);
                        await projectAssets.ProjectDbContext.SaveChangesAsync();
                    }


                    return project;
                }
                throw new NullReferenceException($"The 'ProjectDbContext' for the project {projectName} could not be created.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"An unexpected exception occurred while getting the database context for the project named '{projectName}'");
                throw;
            }
        }
    }
}
