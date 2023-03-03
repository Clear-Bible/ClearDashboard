﻿using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.ParatextPlugin.CQRS.Features.Project;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Dialogs;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Models.EnhancedView;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;

// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class VerseAwareEnhancedViewItemViewModel : EnhancedViewItemViewModel,
            IHandle<TokensJoinedMessage>, 
            IHandle<TokenUnjoinedMessage>,
            IHandle<AlignmentAddedMessage>,
            IHandle<AlignmentDeletedMessage>
    {
        public IWindowManager WindowManager { get; }

        public VerseAwareConductorOneActive ParentViewModel => (VerseAwareConductorOneActive)Parent;

        //public TokenizedTextCorpus? TokenizedTextCorpus { get; set; }

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

        private IEnumerable<AlignmentDisplayViewModel> AlignedVerses => Verses.Where(v => v.AlignmentManager != null).Cast<AlignmentDisplayViewModel>();

        private BindableCollection<VerseDisplayViewModel> _verses = new();
        public BindableCollection<VerseDisplayViewModel> Verses
        {
            get => _verses;
            set
            {
                Set(ref _verses, value);
                NotifyOfPropertyChange(nameof(AlignedVerses));
            }
        }

        private EnhancedViewItemMetadatum? _enhancedViewItemMetadatum;
        public EnhancedViewItemMetadatum? EnhancedViewItemMetadatum
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
        IMediator? mediator, ILifetimeScope? lifetimeScope, IWindowManager windowManager, ILocalizationService localizationService) : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            WindowManager = windowManager;
        }

      
        public async void VerseSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                if (e.AddedItems[0] is VerseDisplayViewModel verseDisplayViewModel)
                {
                    SelectedVerseDisplayViewModel = verseDisplayViewModel;
                    await EventAggregator.PublishOnUIThreadAsync(new VerseSelectedMessage(verseDisplayViewModel));
                }
            }
        }



        public async  void TranslationClicked(object sender, TranslationEventArgs args)
        {
           _ = await WindowManager.ShowDialogAsync(new TranslationSelectionDialog(args.TokenDisplay!,
                args.InterlinearDisplay!));
        }

    

        private Visibility? _progressBarVisibility = Visibility.Hidden;
        public Visibility? ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }

        public override async Task GetData(EnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            EnhancedViewItemMetadatum = metadatum;
            await GetData(ReloadType.Refresh, cancellationToken);
        }

        public async Task RefreshData(ReloadType reloadType = ReloadType.Refresh, CancellationToken cancellationToken = default)
        {
            await GetData(reloadType, cancellationToken);
        }

        private async Task GetData(ReloadType reloadType = ReloadType.Refresh, CancellationToken cancellationToken = default)
        {
            try
            {
                _ = await Task.Factory.StartNew(async () =>
                    {
                        Verses.Clear();

                        ProgressBarVisibility = Visibility.Visible;
                        switch (EnhancedViewItemMetadatum)
                        {
                            case AlignmentEnhancedViewItemMetadatum alignmentEnhancedViewItemMetadatum:
                                await GetAlignmentData(alignmentEnhancedViewItemMetadatum, cancellationToken);
                                await Execute.OnUIThreadAsync(async () =>
                                {
                                    AlignmentSetId = Guid.Parse(alignmentEnhancedViewItemMetadatum.AlignmentSetId!);
                                    ParallelCorpusId = Guid.Parse(alignmentEnhancedViewItemMetadatum.ParallelCorpusId!);
                                    BorderColor = Brushes.DarkGreen;
                                    SourceFontFamily = await GetFontFamily(alignmentEnhancedViewItemMetadatum.SourceParatextId!);
                                    var targetFontFamily = await GetFontFamily(alignmentEnhancedViewItemMetadatum.TargetParatextId!);
                                    TargetFontFamily = targetFontFamily;
                                    TranslationFontFamily = targetFontFamily;
                                    ShowTranslation = true;
                                    IsRtl = alignmentEnhancedViewItemMetadatum.IsRtl ?? false;
                                    IsTargetRtl = alignmentEnhancedViewItemMetadatum.IsTargetRtl ?? false;
                                });
                            
                                break;
                            case InterlinearEnhancedViewItemMetadatum interlinearEnhancedViewItemMetadatum:
                                await GetInterlinearData(interlinearEnhancedViewItemMetadatum, cancellationToken);
                                await Execute.OnUIThreadAsync(async () =>
                                {
                                    TranslationSetId = Guid.Parse(interlinearEnhancedViewItemMetadatum.TranslationSetId!);
                                    ParallelCorpusId = Guid.Parse(interlinearEnhancedViewItemMetadatum.ParallelCorpusId!);
                                    BorderColor = Brushes.SaddleBrown;
                                    SourceFontFamily = await GetFontFamily(interlinearEnhancedViewItemMetadatum.SourceParatextId!);
                                    var targetFontFamily = await GetFontFamily(interlinearEnhancedViewItemMetadatum.TargetParatextId!);
                                    TargetFontFamily = targetFontFamily;
                                    TranslationFontFamily = targetFontFamily;
                                    ShowTranslation = true;
                                    IsRtl = interlinearEnhancedViewItemMetadatum.IsRtl ?? false;
                                    IsTargetRtl = interlinearEnhancedViewItemMetadatum.IsTargetRtl ?? false;
                                  
                                });
                               
                                break;
                            case TokenizedCorpusEnhancedViewItemMetadatum tokenizedCorpusEnhancedViewItemMetadatum:
                                await GetTokenizedCorpusData(tokenizedCorpusEnhancedViewItemMetadatum, cancellationToken, reloadType);
                                await Execute.OnUIThreadAsync(async () =>
                                {
                                    BorderColor = GetCorpusBrushColor(tokenizedCorpusEnhancedViewItemMetadatum.CorpusType);
                                    CorpusId = tokenizedCorpusEnhancedViewItemMetadatum.CorpusId ?? Guid.NewGuid();
                                    SourceFontFamily = await GetFontFamily(tokenizedCorpusEnhancedViewItemMetadatum.ParatextProjectId!);
                                    ShowTranslation = false;
                                    IsRtl = tokenizedCorpusEnhancedViewItemMetadatum.IsRtl ?? false;
                                });
                             
                                break;
                        }
                    }, cancellationToken);
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
            }
        }

        private async Task GetTokenizedCorpusData(TokenizedCorpusEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken, ReloadType reloadType = ReloadType.Refresh)
        {
            try
            {
                ParatextProjectMetadata metadata;

                if (metadatum.ParatextProjectId == ManuscriptIds.HebrewManuscriptId)
                {
                    metadata = ProjectMetadata.HebrewManuscriptMetadata;
                }
                else if (metadatum.ParatextProjectId == ManuscriptIds.GreekManuscriptId)
                {
                    metadata = ProjectMetadata.GreekManuscriptMetadata;
                }
                else
                {
                    // regular Paratext corpus
                    var result = await ExecuteRequest(new GetProjectMetadataQuery(), cancellationToken);
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

                var currentBcv = ParentViewModel.CurrentBcv;

                if (reloadType == ReloadType.Force)
                {
                    metadatum.TokenizedTextCorpus = await TokenizedTextCorpus.Get(Mediator!,
                        new TokenizedTextCorpusId(metadatum.TokenizedTextCorpusId!.Value), true);
                }
                else
                {
                    metadatum.TokenizedTextCorpus ??= await TokenizedTextCorpus.Get(Mediator!,
                        new TokenizedTextCorpusId(metadatum.TokenizedTextCorpusId!.Value), true);
                }

                var offset = (ushort)ParentViewModel.VerseOffsetRange;
                TokensTextRow[] tokensTextRowsRange;
                try
                {
                    var verseRange =
                        metadatum.TokenizedTextCorpus.GetByVerseRange(
                            new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV()), offset, offset);
                    tokensTextRowsRange = verseRange.textRows.Cast<TokensTextRow>().ToArray();
                }
                catch (KeyNotFoundException)
                {
                    Logger!.LogInformation($"Verses for the book '{ParentViewModel.CurrentBcv.BookName}' were not found in TokenizedTextCorpus '{metadatum.TokenizedTextCorpus.TokenizedTextCorpusId.DisplayName}'.");
                    OnUIThread(() => { Title = CreateNoVerseDataTitle(metadatum); });
                    return;
                }


                var bookFound = metadata.AvailableBooks.Any(b => b.Code == ParentViewModel.CurrentBcv.BookName);

                if (bookFound)
                {
                    // combine verse list into one VerseDisplayViewModel
                    BindableCollection<VerseDisplayViewModel> verses = new();

                    foreach (var textRow in tokensTextRowsRange)
                    {
                        verses.Add(await CorpusDisplayViewModel.CreateAsync(LifetimeScope!, textRow, metadatum.TokenizedTextCorpus.TokenizedTextCorpusId.Detokenizer, metadatum.IsRtl ?? false));
                    }
                    OnUIThread(() =>
                    {
                        Title = CreateTitle(metadatum, tokensTextRowsRange, currentBcv);

                        Verses = verses;
                    });
                }
                else
                {
                    OnUIThread(() => { Title = CreateNoVerseDataTitle(metadatum); });
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "An unexpected error occurred while displaying corpus tokens.");
                ProgressBarVisibility = Visibility.Collapsed;

                OnUIThread(() => { Title = CreateNoVerseDataTitle(metadatum); });
            }
            finally
            {
                ProgressBarVisibility = Visibility.Collapsed;
            }
        }

        private  string CreateNoVerseDataTitle(TokenizedCorpusEnhancedViewItemMetadatum metadatum)
        {
            var localizedMessage = LocalizationService.Get("EnhancedView_NoVerseData"); 
            return $"{metadatum.ProjectName} - {metadatum.TokenizationType}    {localizedMessage}";
        }

        private string CreateNoVerseDataTitle(ParallelCorpusEnhancedViewItemMetadatum metadatum)
        {
            var localizedMessage = LocalizationService.Get("EnhancedView_NoVerseData"); 
            return $"{metadatum.ParallelCorpusDisplayName}   {localizedMessage}";
        }

        private static string CreateTitle(TokenizedCorpusEnhancedViewItemMetadatum metadatum,
            IReadOnlyList<TokensTextRow>? tokensTextRowsRange, BookChapterVerseViewModel? currentBcv)
        {

            var title = $"{metadatum.ProjectName} - {metadatum.TokenizationType}";

            // set the title to include the verse range
            if (tokensTextRowsRange!.Count == 1)
            {
                title += $" ({currentBcv!.BookName} {currentBcv.ChapterNum}:{currentBcv.VerseNum})";
            }
            else
            {
                // check to see if we actually have a verse
                if (tokensTextRowsRange.Count > 0)
                {
                    var startNum = (VerseRef)tokensTextRowsRange[0].Ref;
                    var endNum = (VerseRef)tokensTextRowsRange[^1].Ref;
                    title += $" ({currentBcv!.BookName} {currentBcv.ChapterNum}:{startNum.VerseNum} - {endNum.VerseNum})";
                }
                else
                {
                    title += $" ({currentBcv!.BookName} {currentBcv.ChapterNum}:{currentBcv.VerseNum})";
                }
            }

            return title;
        }

        private async Task GetInterlinearData(InterlinearEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            try
            {
                var rows = await GetParallelCorpusVerseTextRows(ParentViewModel.CurrentBcv.GetBBBCCCVVV(), metadatum);

                if (rows == null || rows.Count == 0)
                {
                    Title = CreateNoVerseDataTitle(metadatum);
                    return;
                }
                Verses.Clear();
                foreach (var row in rows)
                {
                    Verses.Add(await InterlinearDisplayViewModel.CreateAsync(LifetimeScope!, row, metadatum.ParallelCorpus.ParallelCorpusId, metadatum.ParallelCorpus.Detokenizer, metadatum.IsRtl ?? false, new TranslationSetId(Guid.Parse(metadatum.TranslationSetId))));
                }

                Title = CreateParallelCorpusItemTitle(metadatum, "EnhancedView_Interlinear", rows.Count);
            }
            catch (Exception)
            {
                Title = CreateNoVerseDataTitle(metadatum);
            }

        }

        private async Task GetAlignmentData(AlignmentEnhancedViewItemMetadatum metadatum, CancellationToken cancellationToken)
        {
            try
            {
                var rows = await GetParallelCorpusVerseTextRows(ParentViewModel.CurrentBcv.GetBBBCCCVVV(), metadatum);
                if (rows == null || rows.Count == 0)
                {
                    Title = CreateNoVerseDataTitle(metadatum);
                    return;
                }
                Verses.Clear();
                foreach (var row in rows)
                {
                    Verses.Add(await AlignmentDisplayViewModel.CreateAsync(LifetimeScope!, 
                        row, 
                        metadatum.ParallelCorpus.ParallelCorpusId, 
                        metadatum.ParallelCorpus.ParallelCorpusId.SourceTokenizedCorpusId.Detokenizer,
                        metadatum.IsRtl ?? false,
                        metadatum.ParallelCorpus.ParallelCorpusId.TargetTokenizedCorpusId.Detokenizer,
                        metadatum.IsTargetRtl ?? false,
                        new AlignmentSetId(Guid.Parse(metadatum.AlignmentSetId))
                        ));
                }

                Title = CreateParallelCorpusItemTitle(metadatum, "EnhancedView_Alignment", rows.Count);

            }
            catch (Exception)
            {
                Title = CreateNoVerseDataTitle(metadatum);
            }
        }

      private string CreateParallelCorpusItemTitle(ParallelCorpusEnhancedViewItemMetadatum metadatum, string localizationKey, int rowCount)
        {
            var title = $"{metadatum.ParallelCorpusDisplayName ?? string.Empty} {LocalizationService.Get(localizationKey)}";

            var verseRange = GetValidVerseRange(ParentViewModel.CurrentBcv.BBBCCCVVV, ParentViewModel.VerseOffsetRange);

            var bcv = new BookChapterVerseViewModel();
            if (rowCount <= 1)
            {
                // only one verse
                bcv.SetVerseFromId(verseRange[0]);
                title += $"  ({bcv.BookName} {bcv.ChapterNum}:{bcv.VerseNum})";
            }
            else
            {
                // multiple verses
                bcv.SetVerseFromId(verseRange[0]);
                title += $"  ({bcv.BookName} {bcv.ChapterNum}:{bcv.VerseNum}-";
                bcv.SetVerseFromId(verseRange[^1]);
                title += $"{bcv.VerseNum})";
            }

            return title;
        }

        private List<string> GetValidVerseRange(string bbbcccvvv, int offset)
        {
            List<string> verseRange = new() { bbbcccvvv };

            var currentVerse = Convert.ToInt32(bbbcccvvv.Substring(6));

            // get lower range first
            var j = 1;
            while (j <= offset)
            {
                // check verse
                if (ParentViewModel.BcvDictionary.ContainsKey(bbbcccvvv.Substring(0, 6) + (currentVerse - j).ToString("000")))
                {
                    verseRange.Add(bbbcccvvv.Substring(0, 6) + (currentVerse - j).ToString("000"));
                }

                j++;
            }


            // get upper range
            j = 1;
            while (j <= offset)
            {
                // check verse
                if (ParentViewModel.BcvDictionary.ContainsKey(bbbcccvvv.Substring(0, 6) + (currentVerse + j).ToString("000")))
                {
                    verseRange.Add(bbbcccvvv.Substring(0, 6) + (currentVerse + j).ToString("000"));
                }

                j++;
            }

            // sort list
            verseRange.Sort();

            return verseRange;
        }

        private async Task<List<EngineParallelTextRow>> GetParallelCorpusVerseTextRows(int bbbcccvvv, ParallelCorpusEnhancedViewItemMetadatum metadatum)
        {
            try
            {

                if (metadatum.ParallelCorpus == null)
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        metadatum.ParallelCorpus = await ParallelCorpus.Get(Mediator!,
                            new ParallelCorpusId(Guid.Parse(metadatum.ParallelCorpusId!)), useCache: true);
                    }
                    finally
                    {
                        stopwatch.Stop();
                        Logger?.LogInformation($"Retrieved parallel corpus '{metadatum.ParallelCorpusId!}' in {stopwatch.ElapsedMilliseconds} ms");
                    }

                }

                var offset = (ushort)ParentViewModel.VerseOffsetRange;
                var verses = metadatum.ParallelCorpus.GetByVerseRange(new VerseRef(bbbcccvvv), offset, offset);
                var rows = verses.parallelTextRows.Select(v => (EngineParallelTextRow)v).ToList();
                return rows;
            }
            catch (Exception ex)
            {
                Logger!.LogError(ex, $"An unexpected error occurred while getting verses from the ParallelCorpus - '{metadatum.ParallelCorpusId!}");
                throw;
            }
        }



        public async Task<TranslationSet> GetTranslationSet(string translationSetId)
        {
           var translationSet =  await TranslationSet.Get(new TranslationSetId(Guid.Parse(translationSetId)), Mediator!);
           return translationSet;
        }
        public async Task<AlignmentSet> GetAlignmentSet(string alignmentSetId)
        {
            var alignmentSet =  await AlignmentSet.Get(new AlignmentSetId(Guid.Parse(alignmentSetId)), Mediator!);
            return alignmentSet;
        }

        private static Brush GetCorpusBrushColor(CorpusType corpusType)
        {
            Brush brush;
            switch (corpusType)
            {
                case CorpusType.Standard:
                    var converter = new BrushConverter();
                    brush = (Brush)converter.ConvertFromString("#7FC9FF");
                    break;
                case CorpusType.BackTranslation:
                    brush = Brushes.Orange;
                    break;
                case CorpusType.Resource:
                    brush = Brushes.PaleGoldenrod;
                    break;
                case CorpusType.Unknown:
                    brush = Brushes.Silver;
                    break;
                case CorpusType.ManuscriptHebrew:
                    brush = Brushes.MediumOrchid;
                    break;
                case CorpusType.ManuscriptGreek:
                    brush = Brushes.MediumOrchid;
                    break;
                default:
                    brush = Brushes.Blue;
                    break;
            }

            return brush;
        }

        private async Task<(FontFamily sourceFontFamily, FontFamily targetFontFamily)> GetSourceAndTargetFontFamilies(string sourceParatextProjectId, string targetParatextProjectId)
        {
            // get the font family for this project
            var sourceFontFamily = await GetFontFamily(sourceParatextProjectId);
            var targetFontFamily = await GetFontFamily(targetParatextProjectId);
            return (sourceFontFamily, targetFontFamily);
        }


        private async Task<FontFamily> GetFontFamily(string paratextProjectId)
        {
            var fontFamilyName = await GetFontFamilyName(paratextProjectId);

            FontFamily fontFamily;
            try
            {
                fontFamily = new(fontFamilyName);
            }
            catch (Exception)
            {
                fontFamily = new(FontNames.DefaultFontFamily);
            }

            return fontFamily;
        }
        private async Task<string?> GetFontFamilyName(string paratextProjectId)
        {
            var result = await Mediator!.Send(new GetProjectFontFamilyQuery(paratextProjectId));
            if (result is { HasData: true })
            {

                return result.Data;
            }

            return FontNames.DefaultFontFamily;
        }

        public async Task HandleAsync(TokensJoinedMessage message, CancellationToken cancellationToken)
        {
            if (EnhancedViewItemMetadatum is TokenizedCorpusEnhancedViewItemMetadatum tokenizedCorpusMetadatum)
            {
                tokenizedCorpusMetadatum.TokenizedTextCorpus = null;
            }
            if (EnhancedViewItemMetadatum is ParallelCorpusEnhancedViewItemMetadatum parallelCorpusMetadatum)
            {
                parallelCorpusMetadatum.ParallelCorpus = null;
            }

            await Task.CompletedTask;
        }

        public async Task HandleAsync(TokenUnjoinedMessage message, CancellationToken cancellationToken)
        {
            if (EnhancedViewItemMetadatum is TokenizedCorpusEnhancedViewItemMetadatum tokenizedCorpusMetadatum)
            {
                tokenizedCorpusMetadatum.TokenizedTextCorpus = null;
            }
            if (EnhancedViewItemMetadatum is ParallelCorpusEnhancedViewItemMetadatum parallelCorpusMetadatum)
            {
                parallelCorpusMetadatum.ParallelCorpus = null;
            }

            await Task.CompletedTask;
        }

        public async Task HandleAsync(AlignmentAddedMessage message, CancellationToken cancellationToken)
        {
            foreach(var verseDisplayViewModel in AlignedVerses)
            {
                await verseDisplayViewModel.HandleAlignmentAddedAsync(message, cancellationToken);
            }
        }

        public async Task HandleAsync(AlignmentDeletedMessage message, CancellationToken cancellationToken)
        {
            foreach (var verseDisplayViewModel in AlignedVerses)
            {
                await verseDisplayViewModel.HandleAlignmentDeletedAsync(message, cancellationToken);
            }
        }


        public async Task HighlightTokensAsync(HighlightTokensMessage message, CancellationToken cancellationToken)
        {
            foreach (var verseDisplayViewModel in AlignedVerses)
            {
                await verseDisplayViewModel.HighlightTokensAsync(message, cancellationToken);
            }
        }

       
        public override bool IsActive => base.IsActive;

        public async Task UnhighlightTokensAsync(UnhighlightTokensMessage message, CancellationToken cancellationToken)
        {
            foreach (var verseDisplayViewModel in AlignedVerses)
            {
                await verseDisplayViewModel.UnhighlightTokensAsync(message, cancellationToken);
            }
        }
    }
}
