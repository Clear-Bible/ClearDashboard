using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Windows;
using ClearDashboard.DataAccessLayer.Wpf;

namespace ClearDashboard.Wpf.ViewModels
{
    public class NotesViewModel : ToolViewModel
    {

        #region Member Variables

       

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

      

        #endregion //Observable Properties

        #region Constructor
        public NotesViewModel()
        {

        }

        public NotesViewModel(INavigationService navigationService, ILogger<NotesViewModel> logger, ProjectManager projectManager):base(navigationService, logger, projectManager)
        {
            this.Title = "🖉 NOTES";
            this.ContentId = "NOTES";

        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
