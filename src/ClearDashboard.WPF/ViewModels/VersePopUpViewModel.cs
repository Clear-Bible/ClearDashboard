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
using ClearDashboard.DataAccessLayer.Paratext;
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
        
        private DataTable _dt = new DataTable();


        #endregion

        #region Observable Objects

        private int _windowWidth = 800;
        public int WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                NotifyOfPropertyChange(() => WindowWidth);
            }
        }

        private int _windowHeight = 450;
        public int WindowHeight
        {
            get => _windowHeight;
            set
            {
                _windowHeight = value;
                NotifyOfPropertyChange(() => WindowHeight);
            }
        }

        private string _BookChapter = "book chapter";
        public string BookChapter
        {
            get => _BookChapter;
            set
            {
                _BookChapter = value; 
                NotifyOfPropertyChange(() => BookChapter);
            }
        }
        

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

        public VersePopUpViewModel()
        {
            
        }

        public VersePopUpViewModel(INavigationService navigationService, ILogger logger,
            ProjectManager projectManager, Verse verse)
        {
            _navigationService = navigationService;
            _projectManager = projectManager;
            _logger = logger;
            _verse = verse;

            BookChapter = verse.VerseID.Substring(0, verse.VerseID.IndexOf(':'));

            flowDirection = _projectManager.CurrentLanguageFlowDirection;
        }

        /// <summary>
        /// Generate the chapter data for display
        /// </summary>
        /// <param name="view"></param>
        protected override void OnViewReady(object view)
        {
            var project = _projectManager.CurrentDashboardProject;
            _dt.Columns.Add("Highlight", typeof(bool));
            _dt.Columns.Add("Verse", typeof(string));
            _dt.Columns.Add(project.TargetProject.Name, typeof(string));
            var verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(_logger, project.TargetProject, _verse);

            foreach (var verse in verses)
            {
                // split off verse from text
                var verseNum = verse.Substring(0, verse.IndexOf(' '));
                if (verseNum == @"\v")
                {
                    verseNum = verse.Substring(3, verse.IndexOf(' '));
                    var verseText = verse.Substring(verseNum.Length + 3);

                    var row = _dt.NewRow();
                    if (_verse.VerseStr == verseNum.PadLeft(3, '0'))
                    {
                        row[0] = true;
                    }
                    else
                    {
                        row[0] = false;
                    }
                    row[1] = verseNum.Trim();
                    row[2] = verseText.Trim();

                    _dt.Rows.Add(row);
                }
            }

            // add each back translation to grid
            foreach (var btProject in project.BackTranslationProjects)
            {
                verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(_logger, btProject, _verse);
                if (verses.Count > 0)
                {
                    _dt.Columns.Add(btProject.Name);

                    foreach (var verse in verses)
                    {
                        // split off verse from text
                        var verseNum = verse.Substring(0, verse.IndexOf(' '));
                        if (verseNum == @"\v")
                        {
                            verseNum = verse.Substring(3, verse.IndexOf(' '));
                            var verseText = verse.Substring(verseNum.Length + 3).Trim();
                            verseNum = verseNum.Trim();

                            // find this verse in the existing datagrid
                            foreach (DataRow row in _dt.Rows)
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


            VersesView = _dt.DefaultView;
            NotifyOfPropertyChange(() => VersesView);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Function to scroll the selected verse into view
        /// </summary>
        /// <param name="grid"></param>
        public void SetScrollView(DataGrid grid)
        {
            for (int i = 0; i < grid.Items.Count; i++)
            {
                if ((bool)_dt.Rows[i][0] == true)
                {
                    DataRowView drv = _dt.DefaultView[i];
                    grid.ScrollIntoView(drv);
                    break;
                }
            }
        }

        #endregion
    }
}
