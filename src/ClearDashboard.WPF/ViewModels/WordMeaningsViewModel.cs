using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Action = System.Action;

namespace ClearDashboard.Wpf.ViewModels
{
    public class WordMeaningsViewModel : ToolViewModel
    {

        #region Member Variables

        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;

        private string _currentVerse = "";

        #endregion //Member Variables

        #region Public Properties


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

        private ObservableCollection<MARBLEresource> _wordData = new ObservableCollection<MARBLEresource>();
        public ObservableCollection<MARBLEresource> WordData
        {
            get => _wordData;
            set
            {
                _wordData = value;
                NotifyOfPropertyChange(() => WordData);
            }
        }

        ObservableCollection<Inline> _targetInlinesText = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> TargetInlinesText
        {
            get
            {
                return _targetInlinesText;
            }
            set
            {
                _targetInlinesText = value;
                NotifyOfPropertyChange(() => TargetInlinesText);
            }
        }

        private string _targetHTML;
        public string TargetHTML
        {
            get => _targetHTML;
            set
            {
                _targetHTML = value;
                NotifyOfPropertyChange(() => TargetHTML);
            }
        }

        ObservableCollection<Inline> _sourceInlinesText = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> SourceInlinesText
        {
            get
            {
                return _sourceInlinesText;
            }
            set
            {
                _sourceInlinesText = value;
                NotifyOfPropertyChange(() => SourceInlinesText);
            }
        }

        private string _sourceVerses = "";
        public string SourceVerses
        {
            get => _sourceVerses;
            set
            {
                _sourceVerses = value;
                NotifyOfPropertyChange(() => SourceVerses);
            }
        }


        #endregion //Observable Properties

        #region Constructor
        public WordMeaningsViewModel()
        {

        }

        public WordMeaningsViewModel(INavigationService navigationService, ILogger<WordMeaningsViewModel> logger, ProjectManager projectManager)
        {
            this.Title = "⌺ WORD MEANINGS";
            this.ContentId = "WORDMEANINGS";
            this.DockSide = EDockSide.Left;

            _logger = logger;

            _projectManager = projectManager;
            CurrentBcv.SetVerseFromId(_projectManager.CurrentVerse);

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

            // listen to the DAL event messages coming in
            _projectManager.NamedPipeChanged += HandleEventAsync;

        }

        #endregion //Constructor

        #region Methods

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
                        if (_currentVerse.EndsWith("000"))
                        {
                            // a zero based verse
                            TargetInlinesText.Clear();
                            NotifyOfPropertyChange(() => TargetInlinesText);
                            TargetHTML = "";
                            WordData.Clear();
                            NotifyOfPropertyChange(() => WordData);
                        }
                        else
                        {
                            // a normal verse
                            Verse  verse = new Verse();
                            verse.VerseBBCCCVVV = _currentVerse;

                            if (verse.BookNum < 40)
                            {
                                _isOT = true;
                            }
                            else
                            {
                                _isOT = false;
                            }

                            _ = ReloadWordMeanings();
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task ReloadWordMeanings()
        {
            GetWhatIsThisWord sd = new GetWhatIsThisWord();
            List<MARBLEresource> whatIsThisWord = sd.GetSemanticDomainData(CurrentBcv);

            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _wordData.Clear();
                foreach (var marbleResource in whatIsThisWord)
                {
                    _wordData.Add(marbleResource);
                }
            });

            NotifyOfPropertyChange(() => WordData);
        }


        #endregion // Methods
    }
}
