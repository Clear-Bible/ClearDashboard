using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer.Features.Bcv;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Verse;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClearDashboard.Wpf.Application.ViewModels.Project
{
    public class EnhancedCorpusViewModel : PaneViewModel, IHandle<VerseChangedMessage>, IHandle<ProjectChangedMessage>
    {

        #region Member Variables      

        #endregion //Member Variables

        #region Public Properties
        
        public string ContentID => this.ContentID;

        public bool IsRtl { get; set; }

        private bool InComingChangesStarted { get; set; }

        private bool _paratextSync = true;
        public bool ParatextSync
        {
            get => _paratextSync;
            set
            {
                _paratextSync = value;
                NotifyOfPropertyChange(() => ParatextSync);
            }
        }

        private Dictionary<string, string> _bcvDictionary;

        private Dictionary<string, string> BCVDictionary
        {
            get => _bcvDictionary;
            set
            {
                _bcvDictionary = value;
                NotifyOfPropertyChange(() => BCVDictionary);
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

        private int _verseRange = 1;
        public int VerseRange
        {
            get => _verseRange;
            set
            {
                _verseRange = value;
                NotifyOfPropertyChange(() => _verseRange);
            }
        }

        #endregion //Public Properties

        #region Observable Properties

        #endregion //Observable Properties

        #region Constructor

        public EnhancedCorpusViewModel()
        {
            
        }

        public EnhancedCorpusViewModel(INavigationService navigationService, ILogger<EnhancedCorpusViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope) :
            base(navigationService: navigationService, logger: logger, projectManager: projectManager, eventAggregator: eventAggregator, mediator: mediator, lifetimeScope: lifetimeScope)
        {
            Title = "⳼ " + LocalizationStrings.Get("Windows_EnhancedCorpus", Logger);
            this.ContentId = "ENHANCEDCORPUS";

            //BcvInit();
        }

        #endregion //Constructor

        #region Methods

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        }

        private async void BcvInit(string paratextProjectId = "")
        {

            var result = await ProjectManager.Mediator.Send(new GetBcvDictionariesQuery(paratextProjectId));
            if (result.Success)
            {
                BCVDictionary = result.Data;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }

            InComingChangesStarted = true;

            // set the CurrentBcv prior to listening to the event
            CurrentBcv.SetVerseFromId(ProjectManager?.CurrentVerse);

            CalculateBooks();
            CalculateChapters();
            CalculateVerses();
            InComingChangesStarted = false;

            // Subscribe to changes of the Book Chapter Verse data object.
            CurrentBcv.PropertyChanged += BcvChanged;
        }

        private void BcvChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ParatextSync && InComingChangesStarted == false)
            {
                string verseId;
                bool somethingChanged = false;
                if (e.PropertyName == "BookNum")
                {
                    // book switch so find the first chapter and verse for that book
                    verseId = BCVDictionary.Values.First(b => b[..3] == CurrentBcv.Book);
                    if (verseId != "")
                    {
                        InComingChangesStarted = true;
                        CurrentBcv.SetVerseFromId(verseId);

                        CalculateChapters();
                        CalculateVerses();
                        InComingChangesStarted = false;
                        somethingChanged = true;
                    }
                }
                else if (e.PropertyName == "Chapter")
                {
                    // ReSharper disable once InconsistentNaming
                    var BBBCCC = CurrentBcv.Book + CurrentBcv.ChapterIdText;

                    // chapter switch so find the first verse for that book and chapter
                    verseId = BCVDictionary.Values.First(b => b.Substring(0, 6) == BBBCCC);
                    if (verseId != "")
                    {
                        InComingChangesStarted = true;
                        CurrentBcv.SetVerseFromId(verseId);

                        CalculateVerses();
                        InComingChangesStarted = false;
                        somethingChanged = true;
                    }
                }
                else if (e.PropertyName == "Verse")
                {
                    InComingChangesStarted = true;
                    CurrentBcv.SetVerseFromId(CurrentBcv.BBBCCCVVV);
                    InComingChangesStarted = false;
                    somethingChanged = true;
                }

                if (somethingChanged)
                {
                    // send to the event aggregator for everyone else to hear about a verse change
                    EventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(CurrentBcv.BBBCCCVVV));

                    // push to Paratext
                    if (ParatextSync)
                    {
                        _ = Task.Run(() => ExecuteRequest(new SetCurrentVerseCommand(CurrentBcv.BBBCCCVVV), CancellationToken.None));
                    }
                }

            }
        }

        private void CalculateBooks()
        {
            CurrentBcv.BibleBookList?.Clear();

            var books = BCVDictionary.Values.GroupBy(b => b.Substring(0, 3))
                .Select(g => g.First())
                .ToList();

            foreach (var book in books)
            {
                var bookId = book.Substring(0, 3);

                var bookName = BookChapterVerseViewModel.GetShortBookNameFromBookNum(bookId);

                CurrentBcv.BibleBookList?.Add(bookName);
            }

        }

        private void CalculateChapters()
        {
            // CHAPTERS
            var bookId = CurrentBcv.Book;
            var chapters = BCVDictionary.Values.Where(b => bookId != null && b.StartsWith(bookId)).ToList();
            for (int i = 0; i < chapters.Count; i++)
            {
                chapters[i] = chapters[i].Substring(3, 3);
            }

            chapters = chapters.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> chapterNumbers = new List<int>();
                foreach (var chapter in chapters)
                {
                    chapterNumbers.Add(Convert.ToInt16(chapter));
                }

                CurrentBcv.ChapterNumbers = chapterNumbers;
            });
        }

        private void CalculateVerses()
        {
            // VERSES
            var bookId = CurrentBcv.Book;
            var chapId = CurrentBcv.ChapterIdText;
            var verses = BCVDictionary.Values.Where(b => b.StartsWith(bookId + chapId)).ToList();

            for (int i = 0; i < verses.Count; i++)
            {
                verses[i] = verses[i].Substring(6);
            }

            verses = verses.DistinctBy(v => v).ToList().OrderBy(b => b).ToList();
            // invoke to get it to run in STA mode
            System.Windows.Application.Current.Dispatcher.Invoke(delegate
            {
                List<int> verseNumbers = new List<int>();
                foreach (var verse in verses)
                {
                    verseNumbers.Add(Convert.ToInt16(verse));
                }

                CurrentBcv.VerseNumbers = verseNumbers;
            });
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (CurrentBcv.BibleBookList.Count == 0)
            {
                return;
            }

            if (message.Verse != "" && CurrentBcv.BBBCCCVVV != message.Verse.PadLeft(9, '0'))
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Project Change"), cancellationToken);

                InComingChangesStarted = true;
                CurrentBcv.SetVerseFromId(message.Verse);

                CalculateChapters();
                CalculateVerses();
                InComingChangesStarted = false;
            }

            return;
        }

        public async Task HandleAsync(ProjectChangedMessage message, CancellationToken cancellationToken)
        {
            if (ProjectManager?.CurrentParatextProject is not null)
            {
                // send to log
                await EventAggregator.PublishOnUIThreadAsync(new LogActivityMessage($"{this.DisplayName}: Project Change"), cancellationToken);


                //BCVDictionary = ProjectManager.CurrentParatextProject.BcvDictionary;
                InComingChangesStarted = true;

                // add in the books to the dropdown list
                CalculateBooks();

                // set the CurrentBcv prior to listening to the event
                CurrentBcv.SetVerseFromId(ProjectManager.CurrentVerse);

                CalculateChapters();
                CalculateVerses();

                NotifyOfPropertyChange(() => CurrentBcv);
                InComingChangesStarted = false;
            }
            else
            {
                BCVDictionary = new Dictionary<string, string>();
            }

            return;
        }


        #endregion // Methods
    }
}
