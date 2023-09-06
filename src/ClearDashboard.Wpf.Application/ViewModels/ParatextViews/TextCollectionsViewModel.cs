using Autofac;
using Caliburn.Micro;
using CefSharp;
using CefSharp.Wpf;
using ClearApplicationFoundation.Framework.Input;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using HtmlAgilityPack;
using MediatR;
using Microsoft.Extensions.Logging;
using Paratext.PluginInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.DataAccessLayer;

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TextCollectionsViewModel : ToolViewModel,
        IHandle<VerseChangedMessage>,
        IHandle<RefreshTextCollectionsMessage>,
        IHandle<ParatextSyncMessage>
    {
        private readonly DashboardProjectManager? _projectManager;

        #region Member Variables

        private string _currentVerse = "";

        #endregion //Member Variables

        #region Public Properties

        public ILogger<TextCollectionsViewModel> Logger;

        #endregion //Public Properties

        #region Commands

        public ICommand RefreshCommand { get; set; }


        #endregion


        #region Observable Properties

        private ObservableCollection<TextCollectionList> _textCollectionLists = new();

        public ObservableCollection<TextCollectionList> TextCollectionLists
        {
            get { return _textCollectionLists; }
            set
            {
                _textCollectionLists = value;
                NotifyOfPropertyChange(() => TextCollectionLists);
            }
        }

        private string _myHtml;

        public string MyHtml
        {
            get { return _myHtml; }
            set
            {
                _myHtml = value;
                NotifyOfPropertyChange(() => MyHtml);
            }
        }

        private bool _textCollectionCallInProgress = false;
        public bool TextCollectionCallInProgress
        {
            get { return _textCollectionCallInProgress; }
            set
            {
                _textCollectionCallInProgress = value;
                NotifyOfPropertyChange(() => TextCollectionCallInProgress);
            }
        }

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

        private Visibility _bcvUserControlVisibility = Visibility.Hidden;
        public Visibility BcvUserControlVisibility
        {
            get => _bcvUserControlVisibility;
            set
            {
                _bcvUserControlVisibility = value;
                NotifyOfPropertyChange(() => BcvUserControlVisibility);
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

        private bool _verseByVerseEnabled;
        public bool VerseByVerseEnabled
        {
            get => _verseByVerseEnabled;
            set
            {
                _verseByVerseEnabled = value;
                NotifyOfPropertyChange(() => _verseByVerseEnabled);
            }
        }

        #endregion BCV

        #endregion //Observable Properties

        #region Constructor
        // ReSharper disable once UnusedMember.Global
        public TextCollectionsViewModel()
        {
            // no-op this is here for the XAML design time
        }

        public TextCollectionsViewModel(INavigationService navigationService, ILogger<TextCollectionsViewModel> logger,
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope, ILocalizationService localizationService) : base(
            navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            _projectManager = projectManager;
            Logger = logger;
            Title = "🗐 " + LocalizationService!.Get("Windows_TextCollection");
            this.ContentId = "TEXTCOLLECTION";

            VerseByVerseEnabled = Settings.Default.VerseByVerseTextCollectionsEnabled;

            // wire up the commands
            RefreshCommand = new RelayCommand(Refresh);

        }

        protected override async void OnViewAttached(object view, object context)
        {
            // grab the dictionary of all the verse lookups
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                BcvDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;

                var books = BcvDictionary.Values.GroupBy(b => b.Substring(0, 3))
                    .Select(g => g.First())
                    .ToList();

                foreach (var book in books)
                {
                    var bookId = book.Substring(0, 3);

                    var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                    CurrentBcv.BibleBookList?.Add(bookName);
                }
            }
            else
            {
                BcvDictionary = new Dictionary<string, string>();
            }
            
            CurrentBcv.SetVerseFromId(_projectManager.CurrentVerse);
            NotifyOfPropertyChange(() => CurrentBcv);
            VerseChange = _projectManager.CurrentVerse;


            await CallGetTextCollections();
            base.OnViewAttached(view, context);
        }

        #endregion //Constructor

        #region Methods

        private async Task CallGetTextCollections()
        {
            if (!TextCollectionCallInProgress)
            {
                TextCollectionCallInProgress = true;

                var workWithUsx = true;
                var showVerseByVerse = Settings.Default.VerseByVerseTextCollectionsEnabled;
                try
                {
                    var result = await ExecuteRequest(new GetTextCollectionsQuery(workWithUsx, showVerseByVerse), CancellationToken.None);

                    if (result.Success)
                    {
                        OnUIThread(async () =>
                        {
                            TextCollectionLists.Clear();
                            var data = result.Data;
                            
                            string collectiveBody = string.Empty;
                            string head =string.Empty;
                            List<string> titles = new List<string>();

                            foreach (var textCollection in data)
                            {
                                TextCollectionList tc = new();

                                var endPart = textCollection.Data;
                                var startPart = textCollection.ReferenceShort.Replace('/', '_');

                                if (titles.Contains(startPart))
                                {
                                    break;
                                }
                                titles.Add(startPart);

                                try
                                {
                                    string xsltPath = Path.Combine(Environment.CurrentDirectory, @"resources\usx.xslt");
                                    var html = UsxParser.TransformXMLToHTML(endPart, xsltPath);

                                    HtmlDocument htmlSnippet = new();
                                    htmlSnippet.LoadHtml(html);

                                    if (head == string.Empty)
                                    {
                                        head = htmlSnippet.DocumentNode.SelectNodes("//head").FirstOrDefault().OuterHtml;
                                    }
                                    var currentBody = htmlSnippet.DocumentNode.SelectNodes("//body");

                                    endPart = currentBody.FirstOrDefault().InnerHtml;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                var classSpecification = string.Empty;
                                if (!showVerseByVerse)
                                {
                                    classSpecification = "class=\"vh\"";
                                }

                                var fontQueryResult = await ExecuteRequest(new GetProjectFontFamilyQuery(textCollection.Id), CancellationToken.None);
                                var fontFamily = FontNames.DefaultFontFamily;
                                if (fontQueryResult.Success)
                                {
                                    fontFamily = fontQueryResult.Data;
                                }

                                collectiveBody +=
                                    "<div id='"+startPart+"'>" +
                                    "<details open>" +
                                    "<summary>" +
                                    startPart+":" +
                                    "</summary>"
                                    +"<span " +
                                classSpecification +
                                    "style=\"font-family:"+fontFamily+";\""+
                                    ">"
                                    +endPart+
                                    "</span>"+
                                    "</details>" +
                                    "</div>" +
                                    "<hr>";
                            }

                            string topAnchor = string.Empty;
                            foreach (var title in titles)
                            {
                                topAnchor += "<a id=\'Link" + title + "\' href=#" + title + ">" + title + "</a>";
                            }
                            var scripts = "<script src=\"https://ajax.googleapis.com/ajax/libs/jquery/3.6.4/jquery.min.js\"></script>\r\n  <script>\r\n    // onScreen jQuery plugin v0.2.1\r\n    // (c) 2011-2013 Ben Pickles\r\n    //\r\n    // http://benpickles.github.io/onScreen\r\n    //\r\n    // Released under MIT license.\r\n    ;(function($) {\r\n    $.expr[\":\"].onTop = function(elem) {\r\n      var $window = $(window);\r\n      var viewport_top = $window.scrollTop();\r\n      var viewport_height = $window.height();\r\n      var viewport_bottom = viewport_top + viewport_height;\r\n      var viewport_middle = viewport_top + viewport_height/100;\r\n\r\n      var half = 20;\r\n\r\n      var $elem = $(elem)\r\n      var top = $elem.offset().top\r\n      var height = $elem.height()\r\n      var bottom = top + height\r\n      var header = 63;\r\n      var result = false;\r\n    \r\n      if(top-21 < viewport_top+header+half && bottom + 2 > viewport_top+header+half){\r\n        result = true;\r\n        var divId = $elem.attr('id');\r\n        var desiredLink = $('#' + \"Link\" + divId);\r\n        desiredLink.siblings('a').removeClass('active');\r\n        desiredLink.addClass('active');\r\n      }\r\n      return result\r\n  \t\t}\r\n\t})(jQuery);\r\n    </script>\r\n    \r\n    \r\n    <script>\r\n    \r\n    window.addEventListener(\"scroll\", function(){\r\n\t\t\t$(\"div\")\r\n            //.css(\"background-color\",\"blue\")\r\n            .filter(\":onTop\")\r\n            //.css(\"background-color\", \"red\")\r\n      });\r\n    </script>";

                            collectiveBody = scripts + "<div id='Home' class='navbar'>" + topAnchor + "</div>" + collectiveBody;
                            MyHtml = "<html>" +
                                        "<link rel=\"stylesheet\" href=\"https://fonts.googleapis.com/icon?family=Material+Icons\">" + 
                                        head + 
                                        collectiveBody + 
                                    "</html>";
                        });
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"BiblicalTermsViewModel Deserialize BiblicalTerms: {e.Message}");
                }

                TextCollectionCallInProgress = false;
            }
        }

        public async void Refresh(object? obj)
        {
            await CallGetTextCollections();
        }

        public void ParagraphVerseToggle(bool value)
        {
            Settings.Default.VerseByVerseTextCollectionsEnabled = VerseByVerseEnabled;
            Settings.Default.Save();

            Refresh(null);
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight, this.Title);
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            var incomingVerse = message.Verse.PadLeft(9, '0');

            if (_currentVerse == incomingVerse)
            {
                return;
            }

            _currentVerse = incomingVerse;
            CurrentBcv.SetVerseFromId(_currentVerse);

            await CallGetTextCollections();
        }

        #endregion // Methods

        public Task HandleAsync(RefreshTextCollectionsMessage message, CancellationToken cancellationToken)
        {
            Refresh(null);
            return Task.CompletedTask;
        }

        public Task HandleAsync(ParatextSyncMessage message, CancellationToken cancellationToken)
        {
            
            BcvUserControlVisibility = message.Synced ?  Visibility.Hidden : Visibility.Visible;
            return Task.CompletedTask;
        }
    }

    public class ChromiumWebBrowserHelper
    {
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.RegisterAttached("Body", typeof(string), typeof(ChromiumWebBrowserHelper), new PropertyMetadata(OnBodyChanged));

        public static string GetBody(DependencyObject dependencyObject)
        {
            return (string)dependencyObject.GetValue(BodyProperty);
        }

        public static void SetBody(DependencyObject dependencyObject, string body)
        {
            dependencyObject.SetValue(BodyProperty, body);
        }

        private static void OnBodyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var chromiumWebBrowser = (ChromiumWebBrowser)d;
                chromiumWebBrowser.LoadHtml((string)e.NewValue, "http://ClearDashboard.Wpf.Application.TextCollection/"); //.NavigateToString((string)e.NewValue);
            }
            catch(Exception ex)
            {
                var textCollectionViewModel = IoC.Get<TextCollectionsViewModel>();
                textCollectionViewModel.Logger.LogError("Body failed to set", ex);
            }
        }
    }
}