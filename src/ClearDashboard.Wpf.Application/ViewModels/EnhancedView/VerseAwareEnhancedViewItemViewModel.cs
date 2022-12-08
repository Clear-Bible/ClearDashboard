using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using System;
using System.CodeDom;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using System.Windows;
using System.Windows.Controls.Ribbon;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using Autofac.Core.Lifetime;
using Autofac;
using System.Windows.Navigation;
using Microsoft.Extensions.Logging;

// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class VerseAwareEnhancedViewItemViewModel : EnhancedViewItemViewModel,   
            IHandle<VerseChangedMessage>
    {

        public TokenizedTextCorpus? TokenizedTextCorpus { get; set; }

        private Guid _corpusId;
        public Guid CorpusId
        {
            get => _corpusId;
            set => Set(ref _corpusId, value);
        }


        private Guid _alignmentSetId;
        public Guid AlignmentSetId
        {
            get => _alignmentSetId;
            set => Set(ref _alignmentSetId, value);
        }


        private Guid _parallelCorpusId;
        public Guid ParallelCorpusId
        {
            get => _parallelCorpusId;
            set => Set(ref _parallelCorpusId, value);
        }

        private Guid _translationSetId;
        public Guid TranslationSetId
        {
            get => _translationSetId;
            set => Set(ref _translationSetId, value);
        }

        private BindableCollection<VerseDisplayViewModel> _verses = new();
        public BindableCollection<VerseDisplayViewModel> Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
        }

        private EnhancedViewItemMetadatum _enhancedViewItemMetadatum;
        public EnhancedViewItemMetadatum EnhancedViewItemMetadatum
        {
            get => _enhancedViewItemMetadatum;
            set => Set(ref _enhancedViewItemMetadatum, value);
        }

        #region FontFamily

        private FontFamily? _targetFontFamily;
        public FontFamily? TargetFontFamily
        {
            get => _targetFontFamily;
            set => Set(ref _targetFontFamily, value);
        }

        private FontFamily? _sourceFontFamily;
        public FontFamily? SourceFontFamily
        {
            get => _sourceFontFamily;
            set => Set(ref _sourceFontFamily, value);
        }

        private FontFamily? _translationFontFamily;
        public FontFamily? TranslationFontFamily
        {
            get => _translationFontFamily;
            set => Set(ref _translationFontFamily, value);
        }

        #endregion


        private bool _showTranslation;
        public bool ShowTranslation
        {
            get => _showTranslation;
            set => Set(ref _showTranslation, value);
        }

        private bool _isRtl;

        public bool IsRtl
        {
            get => _isRtl;
            set => Set(ref _isRtl, value);
        }

        private bool _isTargetRtl;

        public bool IsTargetRtl
        {
            get => _isTargetRtl;
            set => Set(ref _isTargetRtl, value);
        }

        private VerseDisplayViewModel? _selectedVerseDisplayViewModel;
       

        public VerseDisplayViewModel? SelectedVerseDisplayViewModel
        {
            get => _selectedVerseDisplayViewModel;
            set => Set(ref _selectedVerseDisplayViewModel, value);
        }

        public VerseAwareEnhancedViewItemViewModel(DashboardProjectManager? projectManager,
            INavigationService? navigationService, ILogger<VerseAwareEnhancedViewItemViewModel>? logger, IEventAggregator? eventAggregator,
        IMediator? mediator, ILifetimeScope? lifetimeScope) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
        }

        public void InnerListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is VerseDisplayViewModel verseDisplayViewModel)
                {
                    SelectedVerseDisplayViewModel = verseDisplayViewModel;
                }
            }
        }

        private Visibility? _progressBarVisibility = Visibility.Hidden;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        public async Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            EnhancedViewItemMetadatum = metadatum;
            await GetData(cancellationToken);
        }

        public async Task RefreshData(CancellationToken cancellationToken)
        {
            await GetData(cancellationToken);
        }

        private async Task GetData(CancellationToken cancellationToken)
        {
            try
            {
                ProgressBarVisibility = Visibility.Visible;
                switch (EnhancedViewItemMetadatum)
                {
                    case AlignmentEnhancedViewItemMetadatum alignmentEnhancedViewItemMetadatum:
                        await GetAlignmentData(alignmentEnhancedViewItemMetadatum, cancellationToken);
                        break;
                    case InterlinearEnhancedViewItemMetadatum interlinearEnhancedViewItemMetadatum:
                        await GetInterlinearData(interlinearEnhancedViewItemMetadatum, cancellationToken);
                        break;
                    case TokenizedCorpusEnhancedViewItemMetadatum tokenizedCorpusEnhancedViewItemMetadatum:
                        await GetTokenizedCorpusData(tokenizedCorpusEnhancedViewItemMetadatum, cancellationToken);
                        break;
                    case ParallelCorpusEnhancedViewItemMetadatum parallelCorpusEnhancedViewItemMetadatum:
                        await GetParallelCorpusData(parallelCorpusEnhancedViewItemMetadatum, cancellationToken);
                        break;
                }
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
            }
        }

        private async Task GetTokenizedCorpusData(TokenizedCorpusEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            try
            {
                ParatextProjectMetadata metadata;

                if (metadatum.ParatextProjectId == ManuscriptIds.HebrewManuscriptId)
                {
                    metadata = EnhancedViewModel.HebrewManuscriptMetadata;
                }
                else if (metadatum.ParatextProjectId == ManuscriptIds.GreekManuscriptId)
                {
                    metadata = EnhancedViewModel.GreekManuscriptMetadata;
                }
                else
                {
                    // regular Paratext corpus
                    var result = await ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken)!;
                    if (result.Success && result.HasData)
                    {
                        metadata = result.Data!.FirstOrDefault(b =>
                                       b.Id == metadatum.ParatextProjectId!.Replace("-", "")) ??
                                   throw new InvalidOperationException();
                    }
                    else
                    {
                        throw new InvalidOperationException(result.Message);
                    }
                }
                
                var tokenizationType = metadatum.TokenizationType;

                var bookFound = metadata.AvailableBooks.Any(b => b.Code == ParentViewModel.CurrentBcv.BookName);
                //CurrentBook = metadata.AvailableBooks.FirstOrDefault(b => b.Code == ParentViewModel.CurrentBcv.BookName);

                var currentBcv = ParentViewModel.CurrentBcv;
                var currentTokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!, new TokenizedTextCorpusId(metadatum!.TokenizedTextCorpusId!.Value));
                var offset = (ushort)ParentViewModel.VerseOffsetRange;
                var verseRange = currentTokenizedTextCorpus.GetByVerseRange(new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV()), offset, offset);
                var tokensTextRowsRange = verseRange.textRows.Select(v => new TokensTextRow(v)).ToArray();

                // set the title to include the verse range
                var title = $"{metadatum.ProjectName} - {metadatum.TokenizationType}";
                if (tokensTextRowsRange.Length == 1)
                {
                    title += $" ({currentBcv.BookName} {currentBcv.ChapterNum}:{currentBcv.VerseNum})";
                }
                else
                {
                    // check to see if we actually have a verse
                    if (tokensTextRowsRange.Length > 0)
                    {
                        var startNum = (VerseRef)tokensTextRowsRange[0].Ref;
                        var endNum = (VerseRef)tokensTextRowsRange[^1].Ref;
                        title += $" ({currentBcv.BookName} {currentBcv.ChapterNum}:{startNum.VerseNum} - {endNum.VerseNum})";

                    }
                    else
                    {
                        title += $" ({currentBcv.BookName} {currentBcv.ChapterNum}:{currentBcv.VerseNum})";
                    }
                }

                Title = title;

                // combine verse list into one VerseDisplayViewModel
                BindableCollection<VerseDisplayViewModel> verses = new();

                foreach (var textRow in tokensTextRowsRange)
                {
                    var verseDisplayViewModel = IoC.Get<VerseDisplayViewModel>();
                    //FIXME: detokenizer should come from message.Detokenizer.
                    await verseDisplayViewModel!.ShowCorpusAsync(
                        textRow,
                        //FIXME:surface serialization message.detokenizer,
                        new EngineStringDetokenizer(new LatinWordDetokenizer()),
                        metadatum.IsRtl.Value);

                    verses.Add(verseDisplayViewModel);
                }

                if (bookFound)
                {
                    OnUIThread(() =>
                    {
                        Verses = verses;
                    });
                }
                else
                {
                    //OnUIThread(async () => { await UpdateVerseDisplayWhenBookOutOfRange(message); });
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "An unexpected error occurred while displaying corpus tokens.");
                ProgressBarVisibility = Visibility.Collapsed;
                //if (!localCancellationToken.IsCancellationRequested)
                //{
                //    await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
                //        new BackgroundTaskStatus
                //        {
                //            Name = "Fetch Book",
                //            EndTime = DateTime.Now,
                //            ErrorMessage = $"{ex}",
                //            TaskLongRunningProcessStatus = LongRunningTaskStatus.Failed
                //        }), cancellationToken);
                //}

                //OnUIThread(async () => { await UpdateVerseDisplayWhenBookOutOfRange(message); });
            }
            finally
            {
                 ProgressBarVisibility = Visibility.Collapsed;
            }
        }

        private async Task GetParallelCorpusData(ParallelCorpusEnhancedViewItemMetadatum parallelCorpusEnhancedViewItemMetadatum,CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            await Task.CompletedTask;
        }

        private async Task GetInterlinearData(InterlinearEnhancedViewItemMetadatum interlinearEnhancedViewItemMetadatum, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(7), cancellationToken);
            await Task.CompletedTask;
        }

        private async Task GetAlignmentData(AlignmentEnhancedViewItemMetadatum alignmentEnhancedViewItemMetadatum, CancellationToken cancellationToken)
        {

            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            await Task.CompletedTask;
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            await GetData(cancellationToken);
            await Task.CompletedTask;
        }
    }
}
