using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.SQLite;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pipes_Shared;
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
                            // run these tasks without an await
                            _ = ProcessSourceVerseData(CurrentBcv);
                            ProcessTargetVerseData(pipeMessage);
                            _ = ReloadWordMeanings();
                        }
                    }

                    break;
            }
        }

        private void ProcessTargetVerseData(PipeMessage message)
        {
            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _sourceVerses = "";

                // deserialize the list
                var sourceVerseXML = JsonConvert.DeserializeObject<string>((string)message.Payload);

                //File.WriteAllText(@"D:\temp\file.usx", sourceVerseXML, Encoding.UTF8);

                // Make the Unformatted version
                _targetInlinesText.Clear();
                var usxList = Helpers.UsxParser.ParseXMLToList(sourceVerseXML);
                foreach (var line in usxList)
                {
                    _targetInlinesText.Add(line);
                }

                NotifyOfPropertyChange(() => TargetInlinesText);


                // Make the Formatted version
                string startupPath = AppDomain.CurrentDomain.BaseDirectory;
                string xsltPath = Path.Combine(startupPath, @"resources\usx.xslt");
                if (File.Exists(xsltPath))
                {
                    var usxHtml = Helpers.UsxParser.TransformXMLToHTML(sourceVerseXML, xsltPath);
                    TargetHTML = usxHtml;
                }




                // File.WriteAllText(@"D:\temp\output.html", TargetHTML);

            });
        }

        /// <summary>
        /// Get the Biblical Words from 
        /// </summary>
        /// <returns></returns>
        private async Task ReloadWordMeanings()
        {
            //// detect if Paratext is installed
            //ParatextUtils paratextUtils = new ParatextUtils();
            //bool isParatextInstalled = await paratextUtils.IsParatextInstalledAsync().ConfigureAwait(true);

            //if (isParatextInstalled)
            //{
            //    // get the current verse
            //    Debug.WriteLine(CurrentBcv.VerseLocationId);
            //}

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

        private async Task ProcessSourceVerseData(BookChapterVerse bcv)
        {
            List<CoupleOfStrings> verseData = new List<CoupleOfStrings>();

            await Task.Run(() =>
            {
                // connect to the manuscript database
                var appPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string verseFile = Path.Combine(appPath, "Clear_Projects", "manuscriptverses.sqlite");
                // read in the info
                Connection connVerse = new Connection(verseFile);
                ReadData rdVerse = new ReadData(connVerse.Conn);

                // get the manuscript verse from the database
                verseData = rdVerse.GetSourceChapterText(bcv.VerseLocationId);
            }).ConfigureAwait(false);

            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _sourceInlinesText.Clear();
                foreach (var verse in verseData)
                {
                    if (verse.stringA.EndsWith(bcv.VerseIdText))
                    {
                        _sourceInlinesText.Add(
                            new Run("(" + Convert.ToInt16(verse.stringA.Substring(5, 3)) + ") " + verse.stringB)
                            {
                                Foreground = Brushes.Cyan
                            });
                        _sourceInlinesText.Add(new Run("\n"));
                    }
                    else
                    {
                        _sourceInlinesText.Add(
                            new Run("(" + Convert.ToInt16(verse.stringA.Substring(5, 3)) + ") " + verse.stringB)
                            {
                                Foreground = Brushes.AntiqueWhite
                            });
                        _sourceInlinesText.Add(new Run("\n"));
                    }
                }

                NotifyOfPropertyChange(() => SourceInlinesText);
            });
        }

        #endregion // Methods


    }
}
