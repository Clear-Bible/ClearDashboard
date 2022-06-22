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
    public class SourceContextViewModel : ToolViewModel, IHandle<VerseChangedMessage>
    {

        #region Member Variables

        private readonly ILogger<SourceContextViewModel> _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly DashboardProjectManager _projectManager;
        private readonly INavigationService _navigationService;

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

        private BookChapterVerseViewModel _currentBcv = new();
        public BookChapterVerseViewModel CurrentBcv
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
            _navigationService = navigationService;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _logger = logger;
            this.Title = "⬒ SOURCE CONTEXT";
            this.ContentId = "SOURCECONTEXT";
        }

        protected override void OnViewReady(object view)
        {
            _currentVerse = _projectManager.CurrentVerse;

            CurrentBcv.SetVerseFromId(_currentVerse);

            // do not await this otherwise it freezes the UI
            Task.Run(() =>
            {
                ProcessSourceVerseData(CurrentBcv.BBBCCCVVV).ConfigureAwait(false);
            }).ConfigureAwait(false);

            base.OnViewReady(view);
        }

        protected override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _eventAggregator.SubscribeOnUIThread(this);
            return base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }


        #endregion //Constructor

        #region Methods

        private async Task ProcessSourceVerseData(string BBBCCCVVV)
        {
            var verseDataResult = await ExecuteRequest(new GetManuscriptVerseByIdQuery(BBBCCCVVV), CancellationToken.None).ConfigureAwait(false);
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
                        if (verse.stringA.EndsWith(BBBCCCVVV.Substring(6,3)))
                        {
                            _currentBcv.SetVerseFromId(verse.stringA);
                            _sourceInlinesText.Add(
                                new SourceVerses
                                {
                                    IsSelected = true,
                                    VerseNum = Convert.ToInt16(_currentBcv.VerseNum),
                                    VerseText = verse.stringB,
                                });
                        }
                        else
                        {
                            _currentBcv.SetVerseFromId(verse.stringA);
                            _sourceInlinesText.Add(
                                new SourceVerses
                                {
                                    IsSelected = false,
                                    VerseNum = Convert.ToInt16(_currentBcv.VerseNum),
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

        protected override void Dispose(bool disposing)
        {
            
            base.Dispose(disposing);
        }

        /// <summary>
        /// Listen for changes in the DAL regarding any verse change messages coming in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (_currentVerse == "" || _currentVerse != message.Verse.PadLeft(9,'0'))
            {
                _currentVerse = message.Verse.PadLeft(9, '0');

                CurrentBcv.SetVerseFromId(_currentVerse);

                await ProcessSourceVerseData(CurrentBcv.BBBCCCVVV).ConfigureAwait(false);
            }
            else
            {
                _currentVerse = message.Verse;
            }

            await Task.CompletedTask;
        }

        #endregion // Methods
    }
}
