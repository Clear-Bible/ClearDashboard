using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Models.Paratext;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using ClearDashboard.ParatextPlugin.CQRS.Features.UnifiedScripture;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;

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
            DashboardProjectManager? projectManager, IEventAggregator? eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(
            navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            _projectManager = projectManager;
            Title = "🗐 " + LocalizationStrings.Get("Windows_TextCollection", Logger);
            this.ContentId = "TEXTCOLLECTION";
        }

        protected override async void OnViewAttached(object view, object context)
        {
            BcvDictionary = _projectManager.CurrentParatextProject.BcvDictionary;
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
                    await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: TextCollections read"));

                    if (result.Success)
                    {
                        OnUIThread(async () =>
                        {
                            TextCollectionLists.Clear();
                            var data = result.Data;

                            foreach (var textCollection in data)
                            {
                                TextCollectionList tc = new();

                                var endPart = textCollection.Data;
                                var startPart = textCollection.ReferenceShort;

                                if (workWithUsx)
                                {
                                    try
                                    {
                                        //var list = UsxParser.ParseXMLToList(endPart);
                                        //list.Reverse();
                                        //var count = 0;
                                        //foreach (var item in list)
                                        //{
                                        //    if (count < 100)
                                        //    {
                                        //        count++;
                                        //        tc.Inlines.Insert(0, item.Inline);
                                        //    }
                                        //}

                                        string xsltPath = Path.Combine(Environment.CurrentDirectory, @"resources\usx.xslt");
                                        var html = UsxParser.TransformXMLToHTML(endPart, xsltPath);
                                        //MyHtml = html;
                                        tc.MyHtml = html;

                                        //_myHtml = @"<!DOCTYPE html>
                                        //<html lang=""en"">
                                        //    <body>
                                        //    <div>My Test HTML 'single quote', ""double quote""</div>
                                        //    </body>
                                        //</html>";
                                        //var htmlCollection = new TextCollection()
                                        //{
                                        //    Data = html
                                        //};
                                        //var textCollectionList = new List<TextCollection>();
                                        //textCollectionList.Add(htmlCollection);
                                        //await EventAggregator.PublishOnUIThreadAsync(new TextCollectionChangedMessage(textCollectionList));
                                        //break;
                                        //var html = UsxParser.ConvertXMLToHTML(endPart, CurrentBcv.Book, ProjectManager.CurrentParatextProject.Language.FontFamily,1);

                                        //var strang = UsxParser.ParseXMLstring(endPart);
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

    public class WebBrowserHelper
    {
        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.RegisterAttached("Body", typeof(string), typeof(WebBrowserHelper), new PropertyMetadata(OnBodyChanged));

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
            var webBrowser = (WebBrowser)d;
            webBrowser.NavigateToString((string)e.NewValue);
        }
    }
}
