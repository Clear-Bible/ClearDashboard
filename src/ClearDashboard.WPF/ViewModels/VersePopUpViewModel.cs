using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.Helpers;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using SIL.Machine.Corpora;

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

        private FlowDirection _flowDirection = FlowDirection.LeftToRight;
        public FlowDirection flowDirection
        {
            get => _flowDirection;
            set
            {
                _flowDirection = value;
                NotifyOfPropertyChange(() => flowDirection);
            }
        }



        #endregion


        #region Constructor

        public VersePopUpViewModel(INavigationService navigationService, ILogger logger,
            ProjectManager projectManager, Verse verse)
        {
            _navigationService = navigationService;
            _projectManager = projectManager;
            _logger = logger;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;


            var project = _projectManager.CurrentDashboardProject;
            foreach (var btProject in project.BackTranslationProjects)
            {
                // TODO HACK WITH FIXED REFERENCES
                var verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(logger, btProject, verse);
            }

            



        }


        #endregion

        #region Methods



        #endregion
    }
}
