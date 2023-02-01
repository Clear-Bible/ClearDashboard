using Autofac;
using Caliburn.Micro;
using CefSharp;
using CefSharp.Wpf;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Messages;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using HtmlAgilityPack;

namespace ClearDashboard.Wpf.Application.ViewModels.ParatextViews
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TextCollectionsViewModel : ToolViewModel,
        IHandle<VerseChangedMessage>
    {
        private readonly DashboardProjectManager? _projectManager;

        #region Member Variables

        private string _currentVerse = "";

        #endregion //Member Variables

        #region Public Properties


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
            Title = "🗐 " + LocalizationService!.Get("Windows_TextCollection");
            this.ContentId = "TEXTCOLLECTION";


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


            await CallGetTextCollections().ConfigureAwait(false);
            base.OnViewAttached(view, context);
        }

        #endregion //Constructor

        #region Methods

        private async Task CallGetTextCollections()
        {
            if (!_textCollectionCallInProgress)
            {
                _textCollectionCallInProgress = true;

                var workWithUsx = true;
                try
                {
                    var result = await ExecuteRequest(new GetTextCollectionsQuery(workWithUsx), CancellationToken.None).ConfigureAwait(false);

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
                                var startPart = textCollection.ReferenceShort;

                                titles.Add(startPart);

                                if (workWithUsx)
                                {
                                    try
                                    {
                                        string xsltPath = Path.Combine(Environment.CurrentDirectory, @"resources\usx.xslt");
                                        var html = UsxParser.TransformXMLToHTML(endPart, xsltPath);
                                        tc.MyHtml = html;

                                        HtmlDocument thisHtml = new();
                                        thisHtml.LoadHtml(html);

                                        head = thisHtml.DocumentNode.SelectNodes("//head").FirstOrDefault().OuterHtml;

                                        HtmlDocument htmlSnippet = new HtmlDocument();
                                        htmlSnippet.LoadHtml(html);
                                        var body = htmlSnippet.DocumentNode.SelectNodes("//body");
                                        var text = body.FirstOrDefault().InnerHtml;

                                        collectiveBody += 
                                            "<div id='"+startPart+"'>" +
                                                "<details open>" +
                                                    "<summary>" +
                                                        "<a href='#Home'>" +
                                                            "Home" +
                                                        "<a/>"+
                                                        startPart+":" +
                                                    "</summary>"
                                                    +text+
                                                "</details>" +
                                            "</div>" +
                                            "<hr>";
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        tc.Inlines.Insert(0, new Run(endPart) { FontWeight = FontWeights.Normal });
                                    }
                                }
                                else
                                {
                                    tc.Inlines.Insert(0, new Run(endPart) { FontWeight = FontWeights.Normal });
                                }

                                SolidColorBrush PrimaryHueDarkBrush = System.Windows.Application.Current.TryFindResource("PrimaryHueDarkBrush") as SolidColorBrush;
                                tc.Inlines.Insert(0, new Run(startPart + ":  ") { FontWeight = FontWeights.Bold, Foreground = PrimaryHueDarkBrush });

                                TextCollectionLists.Add(tc);
                            }

                            string topAnchor = string.Empty;
                            foreach (var title in titles)
                            {
                                topAnchor += "<a href=#" + title + ">" + title + "</a>";
                            }

                            collectiveBody = "<div id='Home' class='navbar'>" + topAnchor + "</div>" + collectiveBody;
                            MyHtml = "<html>" + head + collectiveBody + "</html>";
                        });
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"BiblicalTermsViewModel Deserialize BiblicalTerms: {e.Message}");
                }

                _textCollectionCallInProgress = false;
            }

            
        }

        public async void Refresh(object obj)
        {
            await CallGetTextCollections();
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
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

            await CallGetTextCollections().ConfigureAwait(false);
        }

        #endregion // Methods
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
            var chromiumWebBrowser = (ChromiumWebBrowser)d;
            chromiumWebBrowser.LoadHtml((string)e.NewValue, "http://ClearDashboard.Wpf.Application.TextCollection/"); //.NavigateToString((string)e.NewValue);
        }
    }
}
