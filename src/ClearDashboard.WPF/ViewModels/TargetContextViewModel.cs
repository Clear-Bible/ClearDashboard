using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.Wpf.Helpers;
using ClearDashboard.Wpf.ViewModels.Panes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using ClearDashboard.DAL.ViewModels;

namespace ClearDashboard.Wpf.ViewModels
{
    public class TargetContextViewModel : ToolViewModel
    {

        #region Member Variables

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

        private double _zoomFactor = 1;
        public double ZoomFactor
        {
            get => _zoomFactor;
            set
            {
                _zoomFactor = value;
                NotifyOfPropertyChange(() => ZoomFactor);
            }
        }


        private string _formattedHtml;
        public string FormattedHTML
        {
            get => _formattedHtml;
            set
            {
                _formattedHtml = value;
                NotifyOfPropertyChange(() => FormattedHTML);
            }
        }

        private string _anchorRef = string.Empty;
        public string FormattedAnchorRef
        {
            get => _anchorRef;
            set
            {
                _anchorRef = value;
                NotifyOfPropertyChange(() => FormattedAnchorRef);
            }
        }

        private string _unformattedHtml;
        public string UnformattedHTML
        {
            get => _unformattedHtml;
            set
            {
                _unformattedHtml = value;
                NotifyOfPropertyChange(() => UnformattedHTML);
            }
        }

        private string _unformattedAnchorRef = string.Empty;
        public string UnformattedAnchorRef
        {
            get => _unformattedAnchorRef;
            set
            {
                _unformattedAnchorRef = value;
                NotifyOfPropertyChange(() => UnformattedAnchorRef);
            }
        }

        private string _unformattedPath;
        public string UnformattedPath
        {
            get => _unformattedPath;
            set
            {
                _unformattedPath = value;
                NotifyOfPropertyChange(() => UnformattedPath);
            }
        }


        private string _HtmlPath;
        public string HtmlPath
        {
            get => _HtmlPath; 
            set
            {
                _HtmlPath = value;
                NotifyOfPropertyChange(() => HtmlPath);
            }
        }

        #endregion //Public Properties

        #region Observable Properties

      

        ObservableCollection<Inline> _targetInlinesText = new ObservableCollection<Inline>();
        public ObservableCollection<Inline> TargetInlinesText
        {
            get => _targetInlinesText;
            set
            {
                _targetInlinesText = value;
                NotifyOfPropertyChange(() => TargetInlinesText);
            }
        }

        private ObservableCollection<MarbleResource> _wordData = new ObservableCollection<MarbleResource>();
        public ObservableCollection<MarbleResource> WordData
        {
            get => _wordData;
            set
            {
                _wordData = value;
                NotifyOfPropertyChange(() => WordData);
            }
        }

        #endregion //Observable Properties

        #region Commands

        public ICommand ZoomInCommand { get; set; }
        public ICommand ZoomOutCommand { get; set; }

        #endregion

        #region Constructor
        public TargetContextViewModel()
        {

        }

        public TargetContextViewModel(INavigationService navigationService, ILogger<TargetContextViewModel> logger, DashboardProjectManager projectManager):base(navigationService, logger, projectManager)
        {
            this.Title = "⬓ TARGET CONTEXT";
            this.ContentId = "TARGETCONTEXT";

            // wire up the commands
            ZoomInCommand = new RelayCommand(ZoomIn);
            ZoomOutCommand = new RelayCommand(ZoomOut);
        }

        #endregion //Constructor

        #region Methods

        private void ZoomOut(object obj)
        {
            ZoomFactor = ZoomFactor * 1.1;
        }

        private void ZoomIn(object obj)
        {
            ZoomFactor = ZoomFactor * 0.9;
        }


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
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
            //            // check for book change
            //            string newBook = pipeMessage.Text.Substring(0, 2);
            //            if (_currentBook != newBook)
            //            {
            //                _currentBook = newBook;

            //                // send a message to get this book
            //                //await ProjectManager.SendPipeMessage(PipeAction.GetUSX, newBook);

            //                _currentVerse = pipeMessage.Text;
            //                CurrentBcv.SetVerseFromId(_currentVerse);
            //                if (_currentVerse.EndsWith("000"))
            //                {
            //                    // a zero based verse
            //                    TargetInlinesText.Clear();
            //                    NotifyOfPropertyChange(() => TargetInlinesText);
            //                    FormattedHTML = "";
            //                    UnformattedHTML = "";
            //                }
            //                else
            //                {
            //                    // a normal verse
            //                    var verse = new Verse
            //                    {
            //                        VerseBBCCCVVV = _currentVerse
            //                    };

            //                    if (verse.BookNum < 40)
            //                    {
            //                        _isOT = true;
            //                    }
            //                    else
            //                    {
            //                        _isOT = false;
            //                    }
            //                }
            //            } else if (CurrentBcv.VerseLocationId != pipeMessage.Text)
            //            {
            //                CurrentBcv.SetVerseFromId(pipeMessage.Text);
            //                FormattedAnchorRef = CurrentBcv.GetVerseRefAbbreviated();
            //                UnformattedAnchorRef = CurrentBcv.GetVerseId();
            //            }
            //        }
            //        break;

            //    case ActionType.SetUSX:
            //        ProcessTargetVerseData(pipeMessage);
            //        break;
            //}
        }

        private void ProcessTargetVerseData(/*PipeMessage message*/)
        {
            // invoke to get it to run in STA mode
            Application.Current.Dispatcher.Invoke((System.Action)delegate
            {
                // deserialize the payload
                string payload = string.Empty;// message.Payload.ToString();
                string xmlData = JsonSerializer.Deserialize<string>(payload);

                string fontFamily = "Doulos SIL";
                double fontSize = 16;
                if (ProjectManager.ParatextProject is not null)
                {
                    // pull out the project font family
                    fontFamily =ProjectManager.ParatextProject.Language.FontFamily;
                    fontSize = ProjectManager.ParatextProject.Language.Size / (double)12;
                }


                // Make the Unformatted version
                _targetInlinesText.Clear();
                var usfmHtml = UsxParser.ConvertXMLToHTML(xmlData, _currentBook, fontFamily, fontSize);
                UnformattedPath = Path.Combine(ProjectPath.GetProjectPath(ProjectManager), "unformatted.html");

                UnformattedHTML = usfmHtml;
                UnformattedAnchorRef = CurrentBcv.GetVerseId();

                try
                {
                    File.WriteAllText(UnformattedPath, usfmHtml);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "An unexpected error occurred.");
                    Console.WriteLine(e);
                    throw;
                }

                // Make the Formatted version
                string xsltPath = Path.Combine(Environment.CurrentDirectory, @"resources\usx.xslt");
                if (File.Exists(xsltPath))
                {
                    var usxHtml = Helpers.UsxParser.TransformXMLToHTML(xmlData, xsltPath);
                    FormattedHTML = usxHtml;
                }

                // inject into the FormattedHTML the proper font family
                var spot = FormattedHTML.IndexOf("body {") + "body {".Length;
                if (spot > -1)
                {
                    FormattedHTML = FormattedHTML.Insert(spot, "font-family: '" + fontFamily + "';font-size=" + fontSize + "rem;");
                }

                FormattedHTML = FormattedHTML.Replace("font-size: 17px;", $"font-size: {fontSize}rem;");

                FormattedAnchorRef = CurrentBcv.GetVerseRefAbbreviated();

                HtmlPath = Path.Combine(ProjectPath.GetProjectPath(ProjectManager), "formatted.html");

                try
                {
                    File.WriteAllText(HtmlPath, FormattedHTML);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            });
        }

        //private void ConvertListToHTML(List<ParsedXML> usxList)
        //{

        //    int versePosition = 0; // used to calculate the percent that the scrollgroup needs to scroll
        //    int count = 0;
        //    for (int i = 0; i < usxList.Count; i++)
        //    {
        //        if (usxList[i].VerseID != "")
        //        {
        //            string verseID = usxList[i].VerseID.Substring("VERSEID=".Length);
        //            if (CurrentBcv.ChapterIdText + CurrentBcv.VerseIdText == verseID)
        //            {
        //                versePosition = count;
        //            }
        //        }
        //        else
        //        {
        //            _targetInlinesText.Add(usxList[i].Inline);
        //            count++;
        //        }
        //    }

        //}

        #endregion // Methods


    }
}
