using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Sample.Module.Services;
using ClearDashboard.Sample.Module.ViewModels.SampleDialog;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.Sample.Module.ViewModels.Menus
{
    public class SampleCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected IEnhancedViewManager EnhancedViewManager { get; set; }
        protected ISampleManager SampleManager { get; set; }
        protected ILogger Logger { get; set; }
        protected ILocalizationService LocalizationService { get; set; }
        protected IWindowManager WindowManager { get; set; }
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; }
        public ILifetimeScope LifetimeScope { get; set; }
        public LongRunningTaskManager LongRunningTaskManager { get; set; }

        public SampleCorpusAnalysisMenuItemViewModel()
        {

        }

        public SampleCorpusAnalysisMenuItemViewModel(
            ISampleManager aquaManager,
            ILogger<SampleCorpusAnalysisMenuItemViewModel> logger,
            IWindowManager windowManager,
            CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel,
            TokenizedTextCorpusId tokenizedTextCorpusId,
            ILifetimeScope lifetimeScope,
            LongRunningTaskManager longRunningTaskManager,
            [KeyFilter("Sample")] ILocalizationService localizationService,
            IEnhancedViewManager enhancedViewManager)
        {
            Logger = logger;
            LifetimeScope = lifetimeScope;
            LongRunningTaskManager = longRunningTaskManager;
            LocalizationService = localizationService;
            WindowManager = windowManager;
            Header = LocalizationService.Get("Pds_SampleDialogMenuId");
            Id = ISampleManager.SampleDialogMenuId;
            IconKind = PackIconPicolIconsKind.Api.ToString();

            SampleManager = aquaManager;
            EnhancedViewManager = enhancedViewManager;

            CorpusNodeViewModel = corpusNodeMenuItemViewModel.CorpusNodeViewModel;
            TokenizedTextCorpusId = tokenizedTextCorpusId;
        }
        private async Task ShowSampleDialog()
        {
            var parameters = new List<Autofac.Core.Parameter>
            {
                new NamedParameter("dialogMode", DialogMode.Edit),
                new NamedParameter("corpusNodeViewModel", CorpusNodeViewModel),
                new NamedParameter("tokenizedTextCorpusId", TokenizedTextCorpusId),
            };

            var dialogViewModel = LifetimeScope?.Resolve<SampleDialogViewModel>(parameters);

            try
            {
                var result = await WindowManager!.ShowDialogAsync(dialogViewModel, null,
                    DialogSettings.AddParatextCorpusDialogSettings);

                if (result)
                {
                    //do nothing

                    /* //FIXMEAQUA: when adding to enhanced view add this metadatum.
                    var metadatum = new SampleCorpusAnalysisEnhancedViewItemMetadatum
                    {

                        ParatextProjectId = paratextProjectId,
                        DisplayName = $"{CorpusNodeViewModel!.Name} - Sample Analysis",
                        IsNewWindow = addToNewEnhancedView
                    };
                    await EnhancedViewManager.AddMetadatumEnhancedView(metadatum, CancellationToken.None);
                    */
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An unexpected error occurred while initiating the Sample Analysis.");
            }

        }

        protected override async void Execute(CancellationToken cancellationToken)
        {
            switch (Id)
            {
                case ISampleManager.SampleDialogMenuId:
                    await ShowSampleDialog();
                    break;
            }
        }
    }
}
