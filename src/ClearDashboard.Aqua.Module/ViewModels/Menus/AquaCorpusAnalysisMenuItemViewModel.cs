using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Aqua.Module.ViewModels.Menus
{
    public class AquaCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected IEnhancedViewManager EnhancedViewManager { get; set; }
        protected IAquaManager AquaManager { get; set; }
        protected ILogger Logger { get; set; }
        protected ILocalizationService LocalizationService { get; set; }
        protected IWindowManager WindowManager { get; set; }
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; }
        public ILifetimeScope LifetimeScope { get; set; }
        public LongRunningTaskManager LongRunningTaskManager { get; set; }

        public AquaCorpusAnalysisMenuItemViewModel()
        {

        }

        public AquaCorpusAnalysisMenuItemViewModel(
            IAquaManager aquaManager, 
            ILogger<AquaCorpusAnalysisMenuItemViewModel> logger, 
            IWindowManager windowManager,
            CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel,
            TokenizedTextCorpusId tokenizedTextCorpusId,
            ILifetimeScope lifetimeScope, 
            LongRunningTaskManager longRunningTaskManager,
            [KeyFilter("Aqua")] ILocalizationService localizationService,
            IEnhancedViewManager enhancedViewManager) 
        {
            Logger = logger;
            LifetimeScope = lifetimeScope;
            LongRunningTaskManager = longRunningTaskManager;
            LocalizationService = localizationService;
            WindowManager = windowManager;
            Header = LocalizationService.Get("Pds_AquaDialogMenuId");
            Id = IAquaManager.AquaDialogMenuId;
            IconKind = PackIconPicolIconsKind.Api.ToString();

            AquaManager = aquaManager;
            EnhancedViewManager = enhancedViewManager;

            CorpusNodeViewModel = corpusNodeMenuItemViewModel.CorpusNodeViewModel;
            TokenizedTextCorpusId = tokenizedTextCorpusId;
        }
        private async Task ShowAquaDialog()
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Edit),
                new NamedParameter("tokenizedTextCorpusId", TokenizedTextCorpusId),
            };

            var dialogViewModel = LifetimeScope?.Resolve<AquaDialogViewModel>(parameters);

            try
            {
                var result = await WindowManager!.ShowDialogAsync(dialogViewModel, null,
                    DialogSettings.AddParatextCorpusDialogSettings);

                if (result)
                {
                    //do nothing

                    /* //FIXMEAQUA: when adding to enhanced view add this metadatum.
                    var metadatum = new AquaCorpusAnalysisEnhancedViewItemMetadatum
                    {

                        ParatextProjectId = paratextProjectId,
                        DisplayName = $"{CorpusNodeViewModel!.Name} - Aqua Analysis",
                        IsNewWindow = addToNewEnhancedView
                    };
                    await EnhancedViewManager.AddMetadatumEnhancedView(metadatum, CancellationToken.None);
                    */
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
                    await ShowAquaDialog();
                    break;
            }
        }
    }
}
