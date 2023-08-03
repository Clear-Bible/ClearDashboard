using Autofac;
using Caliburn.Micro;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Data;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Models.Common;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.ParatextPlugin.CQRS.Features.CheckUsfm;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using ClearDashboard.Wpf.Application.ViewStartup.ProjectTemplate;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using static ClearBible.Engine.Persistence.FileGetBookIds;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    public class BuildProjectStepViewModel : DashboardApplicationWorkflowStepViewModel<StartupDialogViewModel>, IHandle<BackgroundTaskChangedMessage>
    {
        private readonly ProjectTemplateProcessRunner _processRunner;
        private Task? _runningTask;
        private ProjectDbContextFactory _projectNameDbContextFactory;

        private string _createAction;
        private string _closeAction;
        private string _backAction;
        private string _cancelAction;

        private string _backOrCancelAction;
        public string BackOrCancelAction
        {
            get => _backOrCancelAction;
            set => Set(ref _backOrCancelAction, value);
        }

        private string _createOrCloseAction;
        public string CreateOrCloseAction
        {
            get => _createOrCloseAction;
            set => Set(ref _createOrCloseAction, value);
        }

        private Visibility _progressIndicatorVisibility = Visibility.Hidden;
        public Visibility ProgressIndicatorVisibility
        {
            get => _progressIndicatorVisibility;
            set => Set(ref _progressIndicatorVisibility, value);
        }

        public BuildProjectStepViewModel(DashboardProjectManager projectManager, ProjectTemplateProcessRunner processRunner,
            INavigationService navigationService, ILogger<ProjectSetupViewModel> logger, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope, TranslationSource translationSource, ILocalizationService localizationService,
            ProjectDbContextFactory projectNameDbContextFactory)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _processRunner = processRunner;
            _runningTask = null;
            _projectNameDbContextFactory = projectNameDbContextFactory;

            _createAction = LocalizationService!.Get("Create");
            _closeAction = LocalizationService!.Get("Close");
            _backAction = LocalizationService!.Get("Back");
            _cancelAction = LocalizationService!.Get("Cancel");

            _backOrCancelAction = _backAction;
            _createOrCloseAction = _createAction;
        }
        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _runningTask = null;

            BackOrCancelAction = _backAction;
            CreateOrCloseAction = _createAction;

            CanMoveForwards = true;
            CanMoveBackwards = true;
            EnableControls = true;

            CanMoveForwards = ParentViewModel!.SelectedParatextProject != null && ParentViewModel!.SelectedBookIds.Any();
            await base.OnActivateAsync(cancellationToken);
        }

        public async Task CreateOrClose()
        {
            if (!ParentViewModel!.SelectedBookIds.Any() || ParentViewModel!.SelectedParatextProject is null)
            {
                // FIXME:  
                await ParentViewModel!.GoToStep(1);
                return;
            }

            Stopwatch sw = new();
            sw.Start();

            Action<ILogger>? errorCleanupAction = null;
            BackOrCancelAction = _cancelAction;
            CreateOrCloseAction = _createAction;
            ProgressIndicatorVisibility = Visibility.Visible;

            CanMoveForwards = false;
            CanMoveBackwards = true;

            _processRunner.StartRegistration();

            string? manuscriptHebrewTaskName = null;
            string? manuscriptGreekTaskName = null;
            if (ParentViewModel!.ShowBiblicalTexts)
            {
                manuscriptHebrewTaskName = _processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptHebrew);
                manuscriptGreekTaskName = _processRunner.RegisterManuscriptCorpusTask(CorpusType.ManuscriptGreek);
            }
            string? paratextTaskName = _processRunner.RegisterParatextProjectCorpusTask(ParentViewModel!.SelectedParatextProject, Tokenizers.LatinWordTokenizer, ParentViewModel!.SelectedBookIds);
            string? paratextBtTaskName = null;
            string? paratextLwcTaskName = null;

            if (ParentViewModel!.SelectedParatextBtProject is not null)
            {
                paratextBtTaskName = _processRunner.RegisterParatextProjectCorpusTask(ParentViewModel!.SelectedParatextBtProject, Tokenizers.LatinWordTokenizer, ParentViewModel!.SelectedBookIds);
            }
            if (ParentViewModel!.SelectedParatextLwcProject is not null)
            {
                paratextLwcTaskName = _processRunner.RegisterParatextProjectCorpusTask(ParentViewModel!.SelectedParatextLwcProject, Tokenizers.LatinWordTokenizer, ParentViewModel!.SelectedBookIds);

                var taskNameSetParatextLwc = _processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    paratextLwcTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());
            }

            if (paratextLwcTaskName is not null && paratextBtTaskName is not null)
            {
                var taskNameSetLwcBt = _processRunner.RegisterParallelizationTasks(
                    paratextLwcTaskName,
                    paratextBtTaskName,
                    true,
                    SmtModelType.FastAlign.ToString());
            }

            if (manuscriptHebrewTaskName is not null && paratextLwcTaskName is not null)
            {
                var taskNameLwcHebrewBt = _processRunner.RegisterParallelizationTasks(
                    paratextLwcTaskName,
                    manuscriptHebrewTaskName,
                    false,
                    SmtModelType.FastAlign.ToString());
            }

            if (manuscriptGreekTaskName is not null)
            {
                var taskNameParatextGreekBt = _processRunner.RegisterParallelizationTasks(
                    paratextTaskName,
                    manuscriptGreekTaskName,
                    false,
                    SmtModelType.FastAlign.ToString());
            }

            ProjectManager!.PauseDenormalization = true;

            // This should throw if the project wasn't successfully created:
            await Task.Run(async () => await ProjectManager!.CreateNewProject(ProjectManager!.CurrentDashboardProject.ProjectName));
            try
            {
                _runningTask = _processRunner.RunRegisteredTasks(sw);
                await _runningTask;

                PlaySound.PlaySoundFromResource();
                await TryCloseAsync(false);
            }
            catch (OperationCanceledException)
            {
                errorCleanupAction = await GetErrorCleanupAction(ProjectManager!.CurrentDashboardProject.ProjectName!);
                ProjectManager!.CurrentProject = null;

                PlaySound.PlaySoundFromResource(SoundType.Error);

                await ParentViewModel!.GoToStep(1);
            }
            catch (Exception)
            {
                errorCleanupAction = await GetErrorCleanupAction(ProjectManager!.CurrentDashboardProject.ProjectName!);
                ProjectManager!.CurrentProject = null;

                PlaySound.PlaySoundFromResource(SoundType.Error);

                await ParentViewModel!.GoToStep(1);
            }
            finally
            {
                _runningTask?.Dispose();
                _runningTask = null;

                ProgressIndicatorVisibility = Visibility.Hidden;
                CanMoveForwards = false;
                CanMoveBackwards = false;

                errorCleanupAction?.Invoke(Logger!);

                ProjectManager!.PauseDenormalization = false;

                sw.Stop();
            }
        }

        private async Task<Action<ILogger>?> GetErrorCleanupAction(string projectName)
        {
            await using (var requestScope = _projectNameDbContextFactory!.ServiceScope
                .BeginLifetimeScope(Autofac.Core.Lifetime.MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                await using (var projectContext = await _projectNameDbContextFactory!.GetDatabaseContext(
                    projectName,
                    false,
                    requestScope).ConfigureAwait(false))
                {
                    return InitializeDatabaseCommandHandler.GetErrorCleanupAction(projectContext, Logger!);
                }
            }
        }

        public async Task BackOrCancel()
        {
            if (_runningTask is not null)
            {
                _processRunner.Cancel();
                await Task.WhenAny(tasks: new Task[] { _runningTask, Task.Delay(30000) });
            }
        }

        public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
        {
            Logger!.LogInformation($"RunProcessViewModel:  {message.Status.TaskLongRunningProcessStatus}: {message.Status.Name}: {message.Status.Description}");
            await Task.CompletedTask;
        }
    }
}
