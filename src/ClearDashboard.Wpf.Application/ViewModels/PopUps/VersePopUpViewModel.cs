using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.VerseText;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using ClearApplicationFoundation.Services;

namespace ClearDashboard.Wpf.Application.ViewModels.PopUps
{
    public class VersePopUpViewModel : DashboardApplicationScreen
    {
        #region   Member Variables


        private VerseViewModel _verse;

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


        private FlowDirection _windowFlowDirection = FlowDirection.LeftToRight;
        public new FlowDirection WindowFlowDirection
        {
            get => _windowFlowDirection;
            set
            {
                _windowFlowDirection = value;
                NotifyOfPropertyChange(() => WindowFlowDirection);
            }
        }

        public DataView VersesView { get; set; }

        #endregion

        #region Constructor

        public VersePopUpViewModel()
        {

        }

        /// <summary>
        /// Entry Point for BiblicalTerms verse click
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="logger"></param>
        /// <param name="projectManager"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="verse"></param>
        ///
        public VersePopUpViewModel(INavigationService navigationService, ILogger<VersePopUpViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, VerseViewModel verse, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope,localizationService)
        {
            _verse = verse;

            BookChapter = verse.VerseId.Substring(0, verse.VerseId.IndexOf(':'));

            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        /// <summary>
        /// Entry Point for PINS verse click
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name="logger"></param>
        /// <param name="projectManager"></param>
        /// <param name="eventAggregator"></param>
        /// <param name="verse"></param>
        public VersePopUpViewModel(INavigationService navigationService, ILogger<VersePopUpViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator eventAggregator, IMediator mediator, PinsVerseListViewModel verse, ILifetimeScope? lifetimeScope, ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            VerseViewModel verseViewModel = new VerseViewModel();
            _verse = verseViewModel.SetVerseFromBBBCCCVVV(verse.VerseBBCCCVVV);

            BookChapter = verse.VerseIdShort.Substring(0, verse.VerseIdShort.IndexOf(':'));

            WindowFlowDirection = ProjectManager.CurrentLanguageFlowDirection;
        }

        /// <summary>
        /// Generate the chapter data for display
        /// </summary>
        /// <param name="view"></param>
        protected override async void OnViewReady(object view)
        {
            var project = ProjectManager.CurrentDashboardProject;
            _dt.Columns.Add("Highlight", typeof(bool));
            _dt.Columns.Add("Verse", typeof(string));
            _dt.Columns.Add(project.ProjectName, typeof(string));

            // ReSharper disable once InconsistentNaming
            var BBBCCCVVV = _verse.Entity.VerseBBBCCCVVV.PadLeft(9, '0');

            var verseIdShort = BookChapterVerseViewModel.GetVerseStrShortFromBBBCCCVVV(BBBCCCVVV);

            var bookNum = int.Parse(BBBCCCVVV.Substring(0, 3));
            var chapterNum = int.Parse(BBBCCCVVV.Substring(3, 3));
            var verseNum = int.Parse(BBBCCCVVV.Substring(6, 3));

            var verseTextResult = await ExecuteRequest(new GetParatextVerseTextQuery(bookNum, chapterNum, verseNum),
                CancellationToken.None);
            var verseText = "";
            if (verseTextResult.Success)
                verseText = verseTextResult.Data.Name;
            else
            {
                verseText = "There was an issue getting the text for this verse.";
                //_logger.LogInformation("Failure to GetParatextVerseTextQuery");
            }

            var row = _dt.NewRow();

            //if (_verse.VerseString == verseNum.PadLeft(3, '0'))
            //{
            //    row[0] = true;
            //}
            //else
            //{
            //    row[0] = false;
            //}
            //row[1] = verseNum.Trim();
            row[2] = verseText.Trim();

            //var verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(project.TargetProject, _verse.Entity);

            //foreach (var verse in verses)
            //{
            //    // split off verse from text
            //    var verseNum = verse.Substring(0, verse.IndexOf(' '));
            //    if (verseNum == @"\v")
            //    {
            //        verseNum = verse.Substring(3, verse.IndexOf(' ')).Trim();
            //        var verseText = verse.Substring(verseNum.Length + 3);

            //        var row = _dt.NewRow();
            //        if (_verse.VerseString == verseNum.PadLeft(3, '0'))
            //        {
            //            row[0] = true;
            //        }
            //        else
            //        {
            //            row[0] = false;
            //        }
            //        row[1] = verseNum.Trim();
            //        row[2] = verseText.Trim();

            //        _dt.Rows.Add(row);
            //    }
            //}

 
            //// add each back translation to grid
            //foreach (var btProject in project.BackTranslationProjects)
            //{
            //    verses = DataAccessLayer.Paratext.ExtractVersesFromChapter.ParseUSFM(btProject, _verse.Entity);
            //    if (verses.Count > 0)
            //    {
            //        _dt.Columns.Add(btProject.Name);

            //        foreach (var verse in verses)
            //        {
            //            // split off verse from text
            //            var verseNum = verse.Substring(0, verse.IndexOf(' '));
            //            if (verseNum == @"\v")
            //            {
            //                verseNum = verse.Substring(3, verse.IndexOf(' '));
            //                var verseText = verse.Substring(verseNum.Length + 3).Trim();
            //                verseNum = verseNum.Trim();

            //                // find this verse in the existing datagrid
            //                foreach (DataRow row in _dt.Rows)
            //                {
            //                    if ((string)row["Verse"] == verseNum)
            //                    {
            //                        row[btProject.Name] = verseText;
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}


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
