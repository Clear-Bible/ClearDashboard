using Caliburn.Micro;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.ManuscriptVerses;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SourceContextViewModel : ToolViewModel
    {

        #region Member Variables

        
        private string _currentVerse = "";

        #endregion //Member Variables

        #region Public Properties

        private bool _isOT;
        public bool IsOT
        {
            get => _isOT;
            set
            {
                _isOT = value;
                NotifyOfPropertyChange(() => IsOT);
            }
        }

        private BookChapterVerse _currentBcv = new();
        public BookChapterVerse CurrentBcv
        {
            get => _currentBcv;
            set
            {
                _currentBcv = value;
                NotifyOfPropertyChange(() => CurrentBcv);

                if (_currentBcv.BookNum < 40)
                {
                    IsOT = true;
                }
                else
                {
                    IsOT = false;
                }
            }
        }

        #endregion //Public Properties

        #region Observable Properties

       
        private ObservableCollection<SourceVerses> _sourceInlinesText = new ObservableCollection<SourceVerses>();
        public ObservableCollection<SourceVerses> SourceInlinesText
        { 
            get => _sourceInlinesText;
            set
            {
                _sourceInlinesText = value;
                NotifyOfPropertyChange(() => SourceInlinesText);
            }
        }


        #endregion //Observable Properties

        #region Constructor
        public SourceContextViewModel()
        {

        }

        public SourceContextViewModel(INavigationService navigationService, 
            ILogger<SourceContextViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator) 
            : base(navigationService, logger, projectManager, eventAggregator)
        {
            this.Title = "⬒ SOURCE CONTEXT";
            this.ContentId = "SOURCECONTEXT";
        }


        #endregion //Constructor

        #region Methods

        private async Task ProcessSourceVerseData(BookChapterVerseViewModel bcv)
        {
            var verseDataResult = await ExecuteCommand(new GetManuscriptVerseByIdQuery(bcv.VerseLocationId), CancellationToken.None).ConfigureAwait(false);
            if (verseDataResult.Success == false)
            {
                Logger.LogError(verseDataResult.Message);
                return;
            }

            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                _sourceInlinesText.Clear();
                if (verseDataResult.Data != null)
                {
                    foreach (var verse in verseDataResult.Data)
                    {
                        if (verse.stringA.EndsWith(bcv.VerseIdText))
                        {
                            _sourceInlinesText.Add(
                                new SourceVerses
                                {
                                    IsSelected = true,
                                    VerseNum = Convert.ToInt16(verse.stringA.Substring(5, 3)),
                                    VerseText = verse.stringB,
                                });
                        }
                        else
                        {
                            _sourceInlinesText.Add(
                                new SourceVerses
                                {
                                    IsSelected = false,
                                    VerseNum = Convert.ToInt16(verse.stringA.Substring(5, 3)),
                                    VerseText = verse.stringB,
                                });
                        }
                    }
                }
                else
                {
                   Logger.LogError("Data returned form query is null.");
                  
                }

                NotifyOfPropertyChange(() => SourceInlinesText);
            });
        }

        /// <summary>
        /// Listen for changes in the DAL regarding any messages coming in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void HandleEventAsync(object sender, EventArgs args)
        {
            //TODO:  Refactor to use EventAggregator

            //if (args == null) return;

            //var pipeMessage = args.PipeMessage;

            //switch (pipeMessage.Action)
            //{
            //    case ActionType.CurrentVerse:
            //        if (_currentVerse != pipeMessage.Text)
            //        {
            //            _currentVerse = pipeMessage.Text;
            //            CurrentBcv.SetVerseFromId(_currentVerse);

            //            await ProcessSourceVerseData(CurrentBcv).ConfigureAwait(false);
            //        }

            //        break;
            //}
        }

        protected override void Dispose(bool disposing)
        {
            
            base.Dispose(disposing);
        }

        #endregion // Methods


    }
}
