using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;
using Autofac.Core.Lifetime;
using Autofac;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Project.AddParatextCorpusDialog;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using MediatR;
using System.Collections.Generic;
using ClearDashboard.Wpf.Application.ViewModels.Project.ParallelCorpusDialog;

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public class AquaCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected IAquaManager AquaManager { get; }
        protected ILogger Logger { get;}

        protected IWindowManager WindowManager { get; }
        protected new BindableCollection<CorpusNodeMenuItemViewModel> MenuItems { get; }  //FIXME: Binable collection should not be generically typed to CorpusNodeMenuItemViewModel.
        public ILifetimeScope LifetimeScope { get; }
        public LongRunningTaskManager LongRunningTaskManager { get; }

        //FIXME: should base generic MenuItemViewModel have menuitems too? What is it for? 

        public AquaCorpusAnalysisMenuItemViewModel(
            IAquaManager aquaManager, 
            ILogger<DesignSurfaceViewModel> logger, 
            IWindowManager windowManager,
            BindableCollection<CorpusNodeMenuItemViewModel> menuItems, 
            ILifetimeScope lifetimeScope, 
            LongRunningTaskManager longRunningTaskManager) //FIXME: Binable collection should not be generically typed to CorpusNodeMenuItemViewModel.
        {
            AquaManager = aquaManager;
            Logger = logger;
            MenuItems = menuItems;
            LifetimeScope = lifetimeScope;
            LongRunningTaskManager = longRunningTaskManager;
            WindowManager = windowManager;

            Header = LocalizationStrings.Get("Pds_AquaRequestCorpusAnalysisMenu", Logger);
            Id = IAquaManager.AquaRequestCorpusAnalysis;
            ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
            IconKind = PackIconPicolIconsKind.Api.ToString();
        }

        private async Task ShowRequestCorpusAnalysisDialog(string selectedParatextProjectId)
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Edit),
                new NamedParameter("paratextProjectId", selectedParatextProjectId)
            };

            var dialogViewModel = LifetimeScope?.Resolve<AquaRequestCorpusAnalysisDialogViewModel>(parameters);

            try
            {
                var result = await WindowManager!.ShowDialogAsync(dialogViewModel, null,
                    DialogSettings.AddParatextCorpusDialogSettings);

                if (result)
                {
                    /*
                    var selectedProject = dialogViewModel!.SelectedProject;
                    var bookIds = dialogViewModel.BookIds;

                    var taskName = $"{selectedProject!.Name}";
                    //_busyState.Add(taskName, true);

                    var task = LongRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
                    var cancellationToken = task.CancellationTokenSource!.Token;

                    _ = Task.Run(async () =>
                    {
                        try
                        {

                            var node = DesignSurfaceViewModel!.CorpusNodes
                                .Single(cn => cn.ParatextProjectId == selectedProject.Id);

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                               description: $"Tokenizing and transforming '{selectedProject.Name}' corpus...", cancellationToken: cancellationToken);

                            var textCorpus = await GetTokenizedTransformedParatextProjectTextCorpus(
                                selectedProject.Id!,
                                bookIds,
                                tokenizer,
                                cancellationToken
                            );

                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                               description: $"Updating verses in tokenized text corpus for '{selectedProject.Name}' corpus...Completed", cancellationToken: cancellationToken);

                            var tokenizedTextCorpusId = (await TokenizedTextCorpus.GetAllTokenizedCorpusIds(
                                    Mediator!,
                                    new CorpusId(node.CorpusId)))
                                .FirstOrDefault(tc => tc.TokenizationFunction == tokenizer.ToString());

                            if (tokenizedTextCorpusId is null)
                            {
                                throw new ArgumentException($"No TokenizedTextCorpusId found for corpus '{node.CorpusId}' and tokenization '{tokenizer.ToString()}'");
                            }

                            var tokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, tokenizedTextCorpusId);
                            await tokenizedTextCorpus.UpdateOrAddVerses(Mediator!, textCorpus, cancellationToken);

                            //await EventAggregator.PublishOnUIThreadAsync(new ReloadDataMessage(ReloadType.Force), cancellationToken);

                            await EventAggregator.PublishOnUIThreadAsync(new TokenizedCorpusUpdatedMessage(tokenizedTextCorpusId), cancellationToken);

                            _longRunningTaskManager.TaskComplete(taskName);
                        }
                        catch (OperationCanceledException)
                        {
                            Logger!.LogInformation("UpdateParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                        }
                        catch (MediatorErrorEngineException ex)
                        {
                            if (ex.Message.Contains("The operation was canceled"))
                            {
                                Logger!.LogInformation($"UpdateParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                            }
                            else
                            {
                                Logger!.LogError(ex, "an unexpected Engine exception was thrown.");
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {selectedProject.Name} ");
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                   exception: ex, cancellationToken: cancellationToken);
                            }
                        }
                        finally
                        {
                            _longRunningTaskManager.TaskComplete(taskName);
                            _busyState.Remove(taskName);

                            PlaySound.PlaySoundFromResource();
                        }
                    }, cancellationToken);
                */
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            finally
            {
                //await SaveDesignSurfaceData();
            }
        }
        protected override async void Execute()
        {
            switch (Id)
            {
                case IAquaManager.AquaRequestCorpusAnalysis:
                    await ShowRequestCorpusAnalysisDialog(CorpusNodeViewModel!.ParatextProjectId);


                    Header = LocalizationStrings.Get("Pds_AquaGetCorpusAnalysisMenu", Logger);
                    Id = IAquaManager.AquaGetCorpusAnalysis;
                    ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
                    IconKind = PackIconPicolIconsKind.Api.ToString();

                    break;

                case IAquaManager.AquaAddLatestCorpusAnalysisToCurrentEnhancedView:
                    //AquaManager!.AddCorpusAnalysisToEnhancedView();

                    Header = LocalizationStrings.Get("Pds_AquaRequestCorpusAnalysisMenu", Logger);
                    Id = IAquaManager.AquaRequestCorpusAnalysis;
                    ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
                    IconKind = PackIconPicolIconsKind.Api.ToString();

                    break;

                case IAquaManager.AquaGetCorpusAnalysis:
                    //AquaManager!.GetCorpusAnalysis(CorpusNodeViewModel!.ParatextProjectId, cancellationToken);

                    Header = LocalizationStrings.Get("Pds_AquaAddCorpusAnalysisToEnhancedViewMenu", Logger);
                    Id = IAquaManager.AquaAddLatestCorpusAnalysisToCurrentEnhancedView;
                    ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
                    IconKind = PackIconPicolIconsKind.Api.ToString();

                    break;
            }
        }
    }
}
