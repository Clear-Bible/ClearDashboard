using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.ViewModels
{
    public class VersePopUpViewModel : ApplicationScreen
    {
        #region   Member Variables



        private readonly INavigationService _navigationService;
        private readonly ProjectManager _projectManager;
        private readonly ILogger _logger;

        #endregion

        #region Observable Objects

        #endregion


        #region Constructor

        public VersePopUpViewModel(INavigationService navigationService, ILogger<WorkSpaceViewModel> logger,
            ProjectManager projectManager, Verse verseBBCCCVVV)
        {
            _navigationService = navigationService;
            _projectManager = projectManager;
            _logger = logger;


        }


        #endregion

        #region Methods

        #endregion
    }
}
