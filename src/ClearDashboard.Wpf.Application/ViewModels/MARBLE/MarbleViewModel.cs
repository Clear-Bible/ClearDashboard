using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.MarbleDataRequests;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.ViewModels.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Translation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models.ViewModels.WordMeanings;
using ClearDashboard.Wpf.Application.Views.MARBLE;
using ClearDashboard.Wpf.Application.Views.ParatextViews;

namespace ClearDashboard.Wpf.Application.ViewModels.MARBLE
{
    public class MarbleViewModel : ToolViewModel, IHandle<VerseChangedMessage>
    {
        private readonly ILogger<WordMeaningsViewModel> _logger;
        private readonly DashboardProjectManager? _projectManager;
        private readonly IEventAggregator? _eventAggregator;
        private readonly TranslationSource _translationSource;

        #region Member Variables   

        private List<SemanticDomainLookup> _lookup;


        #region BCV
        private bool _paratextSync = false;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                if (value == true)
                {
                    // TODO do we return back the control to what Paratext is showing
                    // or do we change Paratext to this new verse?  currently set to 
                    // change Paratext to this new verse
                    _ = Task.Run(() =>
                        ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                }

                _paratextSync = value;
                NotifyOfPropertyChange(() => ParatextSync);
            }
        }

        private Dictionary<string, string> _bcvDictionary;
        public Dictionary<string, string> BcvDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
                NotifyOfPropertyChange(() => BcvDictionary);
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
            }
        }

        private int _verseOffsetRange = 0;
        public int VerseOffsetRange
        {
            get => _verseOffsetRange;
            set
            {
                _verseOffsetRange = value;
                NotifyOfPropertyChange(() => _verseOffsetRange);
            }
        }



        private string _verseChange = string.Empty;
        public string VerseChange
        {
            get => _verseChange;
            set
            {
                if (_verseChange == "")
                {
                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
                else if (_verseChange != value)
                {
                    // push to Paratext
                    if (ParatextSync)
                    {
                        _ = Task.Run(() =>
                            ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                    }

                    _verseChange = value;
                    NotifyOfPropertyChange(() => VerseChange);
                }
            }
        }



        #endregion BCV

        #endregion //Member Variables


        #region Public Properties

        #endregion //Public Properties


        #region Observable Properties

        #endregion //Observable Properties

        public ObservableCollection<Senses> Senses;

        private string _selectedHebrew = "";
        public string SelectedHebrew
        {
            get => _selectedHebrew;
            set
            {
                _selectedHebrew = value;
                NotifyOfPropertyChange(() => SelectedHebrew);
            }
        }

        private List<LexicalLink> _lexicalLinks;
        public List<LexicalLink> LexicalLinks
        {
            get => _lexicalLinks;
            set
            {
                _lexicalLinks = value;
                NotifyOfPropertyChange(() => LexicalLinks);
            }
        }

        private LexicalLink _selectedLexicalLink;
        public LexicalLink SelectedLexicalLink
        {
            get => _selectedLexicalLink;
            set
            {
                _selectedLexicalLink = value;
                NotifyOfPropertyChange(() => SelectedLexicalLink);
            }
        }

        #region Constructor

        public MarbleViewModel()
        {
            // no-op
        }

        public MarbleViewModel(INavigationService navigationService, ILogger<WordMeaningsViewModel> logger,
            DashboardProjectManager? projectManager, TranslationSource translationSource,
            IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope)
            : base(navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            _logger = logger;
            _projectManager = projectManager;
            _eventAggregator = eventAggregator;
            _translationSource = translationSource;


            Title = "◕ " + "MARBLE";
            ContentId = "MARBLE";
            DockSide = EDockSide.Bottom;
        }

        protected override void OnViewAttached(object view, object context)
        {
            BcvDictionary = _projectManager.CurrentParatextProject.BcvDictionary;
            CurrentBcv.SetVerseFromId(_projectManager.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = _projectManager.CurrentVerse;

            base.OnViewAttached(view, context);
        }

        protected override async void OnViewReady(object view)
        {
            _lookup = LoadSearchCSV().Result;


            if (ProjectManager.CurrentVerse != String.Empty)
            {
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);
                await ReloadWordMeanings().ConfigureAwait(false);
            }

            base.OnViewReady(view);
        }

        #endregion //Constructor


        #region Methods

        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task ReloadWordMeanings()
        {
            // SDBH & SDBG support the following language codes:
            // en, fr, sp, pt, sw, zht, zhs

            var languageCode = "";

            switch (_translationSource.Language)
            {
                case "es":
                    languageCode = "sp";
                    break;
                case "fr":
                    languageCode = "fr";
                    break;
                case "zh-CN":
                    languageCode = "zhs";
                    break;
                case "zh-TW":
                    languageCode = "zht";
                    break;
                default:
                    // default to English for everyone else
                    languageCode = "en";
                    break;
            }

            //var queryResult = await ExecuteRequest(new GetWhatIsThisWordByBcvQuery(CurrentBcv, languageCode), CancellationToken.None).ConfigureAwait(false);
            //if (queryResult.Success == false)
            //{
            //    Logger!.LogError(queryResult.Message);
            //    return;
            //}


            //if (queryResult.Data == null)
            //{
            //    WordData.Clear();
            //    return;
            //}

            //_whatIsThisWord = queryResult.Data;


            //// invoke to get it to run in STA mode

            //OnUIThread(() =>
            //{
            //    WordData.Clear();
            //    foreach (var marbleResource in _whatIsThisWord)
            //    {
            //        if (marbleResource.IsSense)
            //        {
            //            _wordData.Add(marbleResource);
            //        }
            //    }
            //});

            //NotifyOfPropertyChange(() => WordData);
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<MarbleView>.Show(this, actualWidth, actualHeight);
        }


        public Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #endregion // Methods


    }
}
