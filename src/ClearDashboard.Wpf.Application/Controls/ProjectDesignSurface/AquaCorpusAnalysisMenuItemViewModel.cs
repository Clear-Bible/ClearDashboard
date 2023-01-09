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

namespace ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface
{
    public class AquaCorpusAnalysisMenuItemViewModel : CorpusNodeMenuItemViewModel
    {
        protected IAquaManager AquaManager { get; }
        protected ILogger Logger { get;}

        protected IWindowManager WindowManager { get; }
        protected new BindableCollection<CorpusNodeMenuItemViewModel> MenuItems { get; }  //FIXME: Binable collection should not be generically typed to CorpusNodeMenuItemViewModel.
                                                                                          //FIXME: should base generic MenuItemViewModel have menuitems too? What is it for? 

        public AquaCorpusAnalysisMenuItemViewModel(IAquaManager aquaManager, ILogger<DesignSurfaceViewModel> logger, IWindowManager windowManager,
            BindableCollection<CorpusNodeMenuItemViewModel> menuItems) //FIXME: Binable collection should not be generically typed to CorpusNodeMenuItemViewModel.
        {
            AquaManager = aquaManager;
            Logger = logger;
            MenuItems = menuItems;
            WindowManager = windowManager;

            Header = LocalizationStrings.Get("Pds_AquaRequestCorpusAnalysisMenu", Logger);
            Id = IAquaManager.AquaRequestCorpusAnalysis;
            ProjectDesignSurfaceViewModel = ProjectDesignSurfaceViewModel;
            IconKind = PackIconPicolIconsKind.Api.ToString();
        }

        private async Task ShowRequestCorpusAnalysisDialog()
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;
            
            var requestCoprusAnalysisDialogViewModel = new RequestCorpusAnalysisDialogViewModel(); //FIXME: register and use DI if desired/necessary.

            try
            {
                var result = await WindowManager.ShowDialogAsync(requestCoprusAnalysisDialogViewModel) ?? throw new Exception("no result");

                if (result)
                {
                    await AquaManager!.RequestCorpusAnalysis(CorpusNodeViewModel!.ParatextProjectId, cancellationToken);
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
                    await ShowRequestCorpusAnalysisDialog();


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
