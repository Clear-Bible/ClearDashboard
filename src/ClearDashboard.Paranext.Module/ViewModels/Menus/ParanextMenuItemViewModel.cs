using Autofac;
using Autofac.Features.AttributeFilters;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Paranext.Module.Models;
using ClearDashboard.Paranext.Module.Services;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClearDashboard.Paranext.Module.ViewModels.Menus
{
    public class ParanextMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected IEnhancedViewManager EnhancedViewManager { get; set; }
        protected IParanextManager ParanextManager { get; set; }
        protected ILogger Logger { get; set; }
        protected ILocalizationService LocalizationService { get; set; }
        protected IWindowManager WindowManager { get; set; }
        public TokenizedTextCorpusId TokenizedTextCorpusId { get; }
        public ParanextEnhancedViewItemMetadatum ParanextExtensionInfo { get; }
        public ILifetimeScope LifetimeScope { get; set; }
        public LongRunningTaskManager LongRunningTaskManager { get; set; }

        public ParanextMenuItemViewModel()
        {

        }

        public ParanextMenuItemViewModel(
            //IParanextManager paranextManager,
            ILogger<ParanextMenuItemViewModel> logger,
            IWindowManager windowManager,
            CorpusNodeMenuItemViewModel corpusNodeMenuItemViewModel,
            TokenizedTextCorpusId tokenizedTextCorpusId,
            ParanextEnhancedViewItemMetadatum paranextExtensionInfo,
            ILifetimeScope lifetimeScope,
            LongRunningTaskManager longRunningTaskManager,
            string paranextExtensionMenuId,
            [KeyFilter("ParanextExtension")] ILocalizationService localizationService,
            IEnhancedViewManager enhancedViewManager)
        {
            Logger = logger;
            LifetimeScope = lifetimeScope;
            LongRunningTaskManager = longRunningTaskManager;
            LocalizationService = localizationService;
            WindowManager = windowManager;
            Id = IParanextManager.SampleDialogMenuId;
            IconKind = PackIconPicolIconsKind.Api.ToString();

            //ParanextManager = paranextManager;
            EnhancedViewManager = enhancedViewManager;

            CorpusNodeViewModel = corpusNodeMenuItemViewModel.CorpusNodeViewModel;
            TokenizedTextCorpusId = tokenizedTextCorpusId;
            ParanextExtensionInfo = paranextExtensionInfo;
            Header = LocalizationService.Get(paranextExtensionMenuId);
        }

        public void AddItemToEnhancedView()
        {
            Logger.LogInformation($"AddItemToEnhancedView - {ParanextExtensionInfo.DisplayName}");
            EnhancedViewManager!.AddMetadatumEnhancedView(ParanextExtensionInfo, default);
        }
        protected override async void Execute()
        {
            AddItemToEnhancedView();
        }
    }
}
