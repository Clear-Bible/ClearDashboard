using System;
using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.SQLite;
using Pipes_Shared;
using Action = Caliburn.Micro.Action;

namespace ClearDashboard.Wpf.ViewModels
{
    public class SourceContextViewModel : ToolViewModel
    {

        #region Member Variables

        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;

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

        public SourceContextViewModel(INavigationService navigationService, ILogger<SourceContextViewModel> logger, ProjectManager projectManager)
        {
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

            this.Title = "⬒ SOURCE CONTEXT";
            this.ContentId = "SOURCECONTEXT";

            // listen to the DAL event messages coming in
            _projectManager.NamedPipeChanged += HandleEventAsync;
        }


        #endregion //Constructor

        #region Methods

        private async Task ProcessSourceVerseData(BookChapterVerse bcv)
        {
            List<CoupleOfStrings> verseData = new List<CoupleOfStrings>();

            await Task.Run(() =>
            {
                // connect to the manuscript database
                var verseFile = Path.Combine(Environment.CurrentDirectory, @"Resources\manuscriptverses.sqlite");
                if (! File.Exists(verseFile))
                {
                    _logger.LogError(@"{verseFile} does not exist");
                    return;
                }

                // read in the info
                Connection connVerse = new Connection(verseFile);
                ReadData rdVerse = new ReadData(connVerse.Conn);

                // get the manuscript verse from the database
                verseData = rdVerse.GetSourceChapterText(bcv.VerseLocationId);
            }).ConfigureAwait(false);

            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                _sourceInlinesText.Clear();
                foreach (var verse in verseData)
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

                NotifyOfPropertyChange(() => SourceInlinesText);
            });
        }

        /// <summary>
        /// Listen for changes in the DAL regarding any messages coming in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public async void HandleEventAsync(object sender, PipeEventArgs args)
        {
            if (args == null) return;

            PipeMessage pipeMessage = args.PM;

            switch (pipeMessage.Action)
            {
                case ActionType.CurrentVerse:
                    if (_currentVerse != pipeMessage.Text)
                    {
                        _currentVerse = pipeMessage.Text;
                        CurrentBcv.SetVerseFromId(_currentVerse);

                        await ProcessSourceVerseData(CurrentBcv).ConfigureAwait(false);
                    }

                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            // unsubscribe to the pipes listener
            _projectManager.NamedPipeChanged -= HandleEventAsync;

            base.Dispose(disposing);
        }

        #endregion // Methods


    }
}
