using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
        private Verse _verse;

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

        public DataView VersesView { get; set; }

        #endregion


        #region Constructor

        public VersePopUpViewModel(INavigationService navigationService, ILogger logger,
            ProjectManager projectManager, Verse verse)
        {
            _navigationService = navigationService;
            _projectManager = projectManager;
            _logger = logger;
            _verse = verse;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;
        }

        protected override void OnViewLoaded(object view)
        {
            var project = _projectManager.CurrentDashboardProject;
            DataTable dt = new DataTable();
            dt.Columns.Add("Highlight", typeof(bool));
            dt.Columns.Add("Verse", typeof(string));
            dt.Columns.Add(project.TargetProject.Name, typeof(string));
            var verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(_logger, project.TargetProject, _verse);

            foreach (var verse in verses)
            {
                // split off verse from text
                var verseNum = verse.Substring(0, verse.IndexOf(' '));
                if (verseNum == @"\v")
                {
                    verseNum = verse.Substring(3, verse.IndexOf(' '));
                    var verseText = verse.Substring(verseNum.Length + 2);

                    var row = dt.NewRow();
                    if (_verse.VerseNum == verseNum.PadLeft(3, '0'))
                    {
                        row[0] = true;
                    }
                    else
                    {
                        row[0] = false;
                    }
                    row[1] = verseNum.Trim();
                    row[2] = verseText.Trim();

                    dt.Rows.Add(row);
                }
            }

            // add each back translation to grid
            foreach (var btProject in project.BackTranslationProjects)
            {
                verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(_logger, btProject, _verse);
                if (verses.Count > 0)
                {
                    dt.Columns.Add(btProject.Name);

                    foreach (var verse in verses)
                    {
                        // split off verse from text
                        var verseNum = verse.Substring(0, verse.IndexOf(' '));
                        if (verseNum == @"\v")
                        {
                            verseNum = verse.Substring(3, verse.IndexOf(' '));
                            var verseText = verse.Substring(verseNum.Length + 2).Trim();
                            verseNum = verseNum.Trim();

                            // find this verse in the existing datagrid
                            foreach (DataRow row in dt.Rows)   
                            {
                                if ((string)row["Verse"] == verseNum)
                                {
                                    row[btProject.Name] = verseText;
                                    break;
                                }
                            }
                        }
                    }
                }
            }


            VersesView = dt.DefaultView;
            NotifyOfPropertyChange(() => VersesView);
        }


        #endregion

        #region Methods


        #endregion
    }
}
