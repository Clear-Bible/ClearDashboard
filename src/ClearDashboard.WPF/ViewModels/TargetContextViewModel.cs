using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Caliburn.Micro;
using ClearDashboard.Common.Models;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.NamedPipes;
using ClearDashboard.DataAccessLayer.Paratext;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using Pipes_Shared;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TargetContextViewModel : ToolViewModel
    {

        #region Member Variables

        private readonly ILogger _logger;
        private readonly ProjectManager _projectManager;

        private string _currentVerse = "";
        private string _currentBook = "";

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

        private string _currentFullVerse;
        public string CurrentFullVerse
        {
            get => _currentFullVerse;
            set
            {
                _currentFullVerse = value;
                NotifyOfPropertyChange(() => CurrentFullVerse);
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

        #endregion //Observable Properties

        #region Constructor
        public TargetContextViewModel()
        {

        }

        public TargetContextViewModel(INavigationService navigationService, ILogger<TargetContextViewModel> logger, ProjectManager projectManager)
        {
            this.Title = "⬓ TARGET CONTEXT";
            this.ContentId = "TARGETCONTEXT";

            _logger = logger;
            _projectManager = projectManager;

            flowDirection = _projectManager.CurrentLanguageFlowDirection;

            // listen to the DAL event messages coming in
            _projectManager.NamedPipeChanged += HandleEventAsync;
        }


        #endregion //Constructor

        #region Methods

        protected override void Dispose(bool disposing)
        {
            // unsubscribe to the pipes listener
            _projectManager.NamedPipeChanged -= HandleEventAsync;

            base.Dispose(disposing);
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
                        string newBook = pipeMessage.Text.Substring(0, 2);
                        if (_currentBook != newBook)
                        {
                            _currentBook = newBook;

                            // send a message to get this book
                            _projectManager.SendPipeMessage(ProjectManager.PipeAction.GetUSX, newBook);

                            _currentVerse = pipeMessage.Text;
                            CurrentBcv.SetVerseFromId(_currentVerse);
                            if (_currentVerse.EndsWith("000"))
                            {
                                // a zero based verse
                                TargetInlinesText.Clear();
                                NotifyOfPropertyChange(() => TargetInlinesText);
                                TargetHTML = "";
                            }
                            else
                            {
                                // a normal verse
                                Verse verse = new Verse();
                                verse.VerseBBCCCVVV = _currentVerse;

                                if (verse.BookNum < 40)
                                {
                                    _isOT = true;
                                }
                                else
                                {
                                    _isOT = false;
                                }
                            }
                        }
                    }
                    break;

                case ActionType.SetUSX:
                    ProcessTargetVerseData(pipeMessage);
                    break;
            }
        }

        private void ProcessTargetVerseData(PipeMessage message)
        {
            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                // deserialize the payload
                string payload = message.Payload.ToString();
                string xmlData = JsonSerializer.Deserialize<string>(payload);


                //File.WriteAllText(@"D:\temp\file.usx", xmlData, Encoding.UTF8);

                // Make the Unformatted version
                _targetInlinesText.Clear();
                var usxList = Helpers.UsxParser.ParseXMLToList(xmlData);
                foreach (var line in usxList)
                {
                    _targetInlinesText.Add(line);
                }

                NotifyOfPropertyChange(() => TargetInlinesText);


                // Make the Formatted version
                string xsltPath = Path.Combine(Environment.CurrentDirectory, @"resources\usx.xslt");
                if (File.Exists(xsltPath))
                {
                    var usxHtml = Helpers.UsxParser.TransformXMLToHTML(xmlData, xsltPath);
                    TargetHTML = usxHtml;
                }


                File.WriteAllText(@"D:\temp\output.html", TargetHTML);

            });
        }

        #endregion // Methods


    }
}
