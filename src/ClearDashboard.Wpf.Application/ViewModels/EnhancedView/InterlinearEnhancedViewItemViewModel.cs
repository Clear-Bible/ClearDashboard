using Autofac;
using Caliburn.Micro;

using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;


// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class InterlinearEnhancedViewItemViewModel : VerseAwareEnhancedViewItemViewModel
    {
        public InterlinearEnhancedViewItemViewModel(
            DashboardProjectManager? projectManager, 
            IEnhancedViewManager enhancedViewManager,  
            INavigationService? navigationService, 
            ILogger<VerseAwareEnhancedViewItemViewModel>? logger, 
            IEventAggregator? eventAggregator, 
            IMediator? mediator, ILifetimeScope? 
            lifetimeScope, 
            IWindowManager windowManager, 
            ILocalizationService localizationService,
            NoteManager noteManager) : 
            base(
                projectManager,
                enhancedViewManager, 
                navigationService, 
                logger, 
                eventAggregator, 
                mediator, 
                lifetimeScope, 
                windowManager, 
                localizationService, 
                noteManager)
        {
        }

        public void EnterPressed()
        {
           Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Enter has been pressed");;
        }

        public void CtrlEnterPressed()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Ctrl+Enter has been pressed"); 
        }

        public void ShiftEnterPressed()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Shift+Enter has been pressed");
          
        }

        public void Save()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Ctrl+S has been pressed");
        }

        public void AltEnterPressed()
        {
            Logger.LogInformation("InterlinearEnhancedViewItemViewModel - Alt+Enter has been pressed");
        }
    }
}
