using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TextCollectionViewModel : ToolViewModel
    {

        #region Member Variables

     
        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties


        #endregion //Observable Properties

        #region Constructor
        public TextCollectionViewModel()
        {

        }

        public TextCollectionViewModel(INavigationService navigationService, ILogger<TextCollectionViewModel> logger, ProjectManager projectManager) :base(navigationService, logger, projectManager)
        {
            this.Title = "🗐 TEXT COLLECTION";
            this.ContentId = "TEXTCOLLECTION";
        }

        #endregion //Constructor

        #region Methods

        #endregion // Methods


    }
}
