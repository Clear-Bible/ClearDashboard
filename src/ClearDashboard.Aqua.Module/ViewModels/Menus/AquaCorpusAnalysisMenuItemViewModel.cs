using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearDashboard.Aqua.Module.Models;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Aqua.Module.ViewModels.Menus
{
    public class AquaCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel //MenuItemViewModel<AquaCorpusAnalysisMenuItemViewModel>
    {
        protected IEnhancedViewManager EnhancedViewManager { get; set; }
        protected IAquaManager AquaManager { get; set; }
        protected ILogger Logger { get; set; }
        protected ILocalizationService LocalizationService { get; set; }

        //private CorpusNodeViewModel? _corpusNodeViewModel;
        //public CorpusNodeViewModel? CorpusNodeViewModel
        //{
        //    get => _corpusNodeViewModel;
        //    set => Set(ref _corpusNodeViewModel, value);

        //}
        protected IWindowManager WindowManager { get; set; }

        public ILifetimeScope LifetimeScope { get; set; }
        public LongRunningTaskManager LongRunningTaskManager { get; set; }

        public AquaCorpusAnalysisMenuItemViewModel()
        {

        }

        public AquaCorpusAnalysisMenuItemViewModel(
            IAquaManager aquaManager, 
            ILogger<AquaCorpusAnalysisMenuItemViewModel> logger, 
            IWindowManager windowManager,
            CorpusNodeViewModel corpusNodeViewModel,
            ILifetimeScope lifetimeScope, 
            LongRunningTaskManager longRunningTaskManager,
            [KeyFilter("Aqua")] ILocalizationService localizationService,
            IEnhancedViewManager enhancedViewManager) 
        {
            EnhancedViewManager = enhancedViewManager;
            AquaManager = aquaManager;
            Logger = logger;
            CorpusNodeViewModel = corpusNodeViewModel;
            LifetimeScope = lifetimeScope;
            LongRunningTaskManager = longRunningTaskManager;
            LocalizationService = localizationService;
            WindowManager = windowManager;
            Header = LocalizationService.Get("Pds_AquaDialogMenuId");
            Id = IAquaManager.AquaDialogMenuId;
            IconKind = PackIconPicolIconsKind.Api.ToString();

            AddChildMenuItems();
        }

        private void AddChildMenuItems()
        {
            MenuItems!.Add(new AquaCorpusAnalysisMenuItemViewModel
            {
                Id = IAquaManager.AquaDialogAddToFocusedEnhancedView,
                LifetimeScope = LifetimeScope,
                AquaManager = AquaManager,
                Logger = Logger,
                WindowManager = WindowManager,
                CorpusNodeViewModel = CorpusNodeViewModel,
                LongRunningTaskManager = LongRunningTaskManager,
                LocalizationService = LocalizationService,
                EnhancedViewManager = EnhancedViewManager,
                Header = LocalizationService.Get("Pds_AquaDialogAddToFocusedEnhancedView"),
                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString()
            });

            MenuItems.Add(new AquaCorpusAnalysisMenuItemViewModel
            {
                Id = IAquaManager.AquaDialogAddToNewEnhancedView,
                LifetimeScope = LifetimeScope,
                AquaManager = AquaManager,
                Logger = Logger,
                WindowManager = WindowManager,
                CorpusNodeViewModel = CorpusNodeViewModel,
                LongRunningTaskManager = LongRunningTaskManager,
                LocalizationService = LocalizationService,
                EnhancedViewManager = EnhancedViewManager,
                Header = LocalizationService.Get("Pds_AquaDialogAddToNewEnhancedView"),
                IconKind = PackIconPicolIconsKind.DocumentTextAdd.ToString()
            });
        }

        private async Task ShowAquaDialog(string paratextProjectId, string? versionId, bool addToNewEnhancedView)
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Edit),
                new NamedParameter("paratextProjectId", paratextProjectId),
                new NamedParameter("versionId", versionId ?? "")
            };

            var dialogViewModel = LifetimeScope?.Resolve<AquaDialogViewModel>(parameters);

            try
            {
                var result = await WindowManager!.ShowDialogAsync(dialogViewModel, null,
                    DialogSettings.AddParatextCorpusDialogSettings);

                if (result)
                {
                    var metadatum = new AquaCorpusAnalysisEnhancedViewItemMetadatum
                    {
                        ParatextProjectId = paratextProjectId,
                        DisplayName = $"{CorpusNodeViewModel!.Name} - Aqua Analysis",
                        IsNewWindow = addToNewEnhancedView
                    };
                    await EnhancedViewManager.AddMetadatumEnhancedView(metadatum, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected error occurred while initiating the Aqua Analysis.");
            }
           
        }
        protected override async void Execute()
        {
            switch (Id)
            {
                case IAquaManager.AquaDialogMenuId:
                    //no-op - this is the root menu item
                    break;
                case IAquaManager.AquaDialogAddToFocusedEnhancedView:
                case IAquaManager.AquaDialogAddToNewEnhancedView:
                    await ShowAquaDialog(CorpusNodeViewModel!.ParatextProjectId, "123", Id == IAquaManager.AquaDialogAddToNewEnhancedView);
                    break;
              
                //case IAquaManager.AquaListRevisionsAndAssessmentsAndAddRevision:
                //    await ShowAquaListRevisionsAndAssessmentsAndAddRevisionDialog(CorpusNodeViewModel!.ParatextProjectId, "35");
                //    break;
            }
        }
    }
}
