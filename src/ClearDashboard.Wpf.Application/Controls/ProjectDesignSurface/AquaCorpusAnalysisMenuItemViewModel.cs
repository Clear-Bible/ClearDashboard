using Autofac;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project.Aqua;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Infrastructure;

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

            Header = LocalizationStrings.Get("Pds_AquaDialogMenuId", Logger);
            Id = IAquaManager.AquaDialogMenuId;
            ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
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
