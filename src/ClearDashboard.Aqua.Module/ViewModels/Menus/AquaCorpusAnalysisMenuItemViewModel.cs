using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.Aqua.Module.Services;
using ClearDashboard.Aqua.Module.ViewModels.AquaDialog;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Aqua.Module.ViewModels.Menus
{
    public class AquaCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected IAquaManager AquaManager { get; }
        protected ILogger Logger { get;}
        protected ILocalizationService LocalizationService { get; }

        protected IWindowManager WindowManager { get; }
        protected new BindableCollection<CorpusNodeMenuItemViewModel> MenuItems { get; }  //FIXME: Bindable collection should not be generically typed to CorpusNodeMenuItemViewModel.
        public ILifetimeScope LifetimeScope { get; }
        public LongRunningTaskManager LongRunningTaskManager { get; }

        //FIXME: should base generic MenuItemViewModel have menuitems too? What is it for? 

        public AquaCorpusAnalysisMenuItemViewModel(
            IAquaManager aquaManager, 
            ILogger<DesignSurfaceViewModel> logger, 
            IWindowManager windowManager,
            BindableCollection<CorpusNodeMenuItemViewModel> menuItems,
            CorpusNodeViewModel corpusNodeViewModel,
            ILifetimeScope lifetimeScope, 
            LongRunningTaskManager longRunningTaskManager, ILocalizationService localizationService) //FIXME: Bindable collection should not be generically typed to CorpusNodeMenuItemViewModel.
        {
            AquaManager = aquaManager;
            Logger = logger;
            MenuItems = menuItems;
            CorpusNodeViewModel = corpusNodeViewModel;
            LifetimeScope = lifetimeScope;
            LongRunningTaskManager = longRunningTaskManager;
            LocalizationService = localizationService;
            WindowManager = windowManager;

            Header = LocalizationService.Get("Pds_AquaDialogMenuId");
            Id = IAquaManager.AquaDialogMenuId;
            //ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
            IconKind = PackIconPicolIconsKind.Api.ToString();
        }

        

        private async Task ShowAquaDialog(string paratextProjectId, string? versionId)
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
                case IAquaManager.AquaDialogMenuId:
                    await ShowAquaDialog(CorpusNodeViewModel!.ParatextProjectId, "123");


                    //Header = LocalizationStrings.Get("Pds_AquaGetCorpusAnalysisMenu", Logger);
                    //Id = IAquaManager.AquaListRevisionsAndAssessmentsAndAddRevision;
                    //ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
                    //IconKind = PackIconPicolIconsKind.Api.ToString();

                    break;

                //case IAquaManager.AquaListRevisionsAndAssessmentsAndAddRevision:
                //    await ShowAquaListRevisionsAndAssessmentsAndAddRevisionDialog(CorpusNodeViewModel!.ParatextProjectId, "35");
                //    break;
            }
        }
    }
}
