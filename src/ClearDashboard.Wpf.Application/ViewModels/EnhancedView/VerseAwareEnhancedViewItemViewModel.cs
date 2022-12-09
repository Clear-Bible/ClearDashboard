using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Exceptions;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.ViewModels;
using ClearDashboard.DataAccessLayer;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.Projects;
using ClearDashboard.Wpf.Application.Models.ProjectSerialization;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
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
using ClearDashboard.Wpf.Application.Helpers;
using ParallelCorpus = ClearDashboard.DAL.Alignment.Corpora.ParallelCorpus;
using ClearDashboard.DAL.Alignment.Translation;
using AlignmentSet = ClearDashboard.DAL.Alignment.Translation.AlignmentSet;
using TranslationSet = ClearDashboard.DAL.Alignment.Translation.TranslationSet;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

// ReSharper disable InconsistentNaming

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class VerseAwareEnhancedViewItemViewModel : EnhancedViewItemViewModel,
            IHandle<VerseChangedMessage>
    {

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

        private BindableCollection<VerseDisplayViewModel> _verses = new();
        public BindableCollection<VerseDisplayViewModel> Verses
        {
            get => _verses;
            set => Set(ref _verses, value);
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
                _ = await Task.Factory.StartNew(async () =>
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
                                await GetTokenizedCorpusData(tokenizedCorpusEnhancedViewItemMetadatum,
                                    cancellationToken);
                                break;
                        }
                    }, cancellationToken);
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

                metadatum.TokenizedTextCorpus ??= await TokenizedTextCorpus.Get(Mediator!, new TokenizedTextCorpusId(metadatum.TokenizedTextCorpusId!.Value));

                var offset = (ushort)ParentViewModel.VerseOffsetRange;
                var verseRange = metadatum.TokenizedTextCorpus.GetByVerseRange(new VerseRef(ParentViewModel.CurrentBcv.GetBBBCCCVVV()), offset, offset);
                var tokensTextRowsRange = verseRange.textRows.Select(v => new TokensTextRow(v)).ToArray();

                var bookFound = metadata.AvailableBooks.Any(b => b.Code == ParentViewModel.CurrentBcv.BookName);

                if (bookFound)
                {
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
                            metadatum.IsRtl!.Value);

                        verses.Add(verseDisplayViewModel);
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

        private static string CreateNoVerseDataTitle(TokenizedCorpusEnhancedViewItemMetadatum metadatum)
        {
            // TODO:  localize the message
            var localizedMessage = "No verse data in this verse range.";
            return $"{metadatum.ProjectName} - {metadatum.TokenizationType}    {localizedMessage}";
        }

        private static string CreateNoVerseDataTitle(ParallelCorpusEnhancedViewItemMetadatum metadatum)
        {
            var localizedMessage = "No verse data in this verse range.";
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

        //private async Task GetParallelCorpusData(ParallelCorpusEnhancedViewItemMetadatum parallelCorpusEnhancedViewItemMetadatum, CancellationToken cancellationToken)
        //{
        //    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
        //    await Task.CompletedTask;
        //}

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

                foreach (var row in rows)
                {
                    var verseDisplayViewModel = IoC.Get<VerseDisplayViewModel>();
                    await verseDisplayViewModel!.ShowTranslationAsync(
                        row ?? throw new InvalidDataEngineException(name: "row", value: "null"),
                        await GetTranslationSet(metadatum.TranslationSetId ??
                                                throw new InvalidDataEngineException(name: "message.TranslationSetId",
                                                    value: "null")),
                        //FIXME:surface serialization message.SourceDetokenizer, 
                        new EngineStringDetokenizer(new LatinWordDetokenizer()),
                        metadatum.IsRtl);

                    Verses.Add(verseDisplayViewModel);
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
                foreach (var row in rows)
                {
                    var verseDisplayViewModel = IoC.Get<VerseDisplayViewModel>();

                    await verseDisplayViewModel!.ShowAlignmentsAsync(
                        row ?? throw new InvalidDataEngineException(name: "row", value: "null"),
                        await GetAlignmentSet(metadatum.AlignmentSetId!),
                        //FIXME:surface serialization message.SourceDetokenizer, 
                        new EngineStringDetokenizer(new LatinWordDetokenizer()),
                        metadatum.IsRtl,
                        //FIXME:surface serialization message.TargetDetokenizer ?? throw new InvalidParameterEngineException(name: "message.TargetDetokenizer", value: "null", message: "message.TargetDetokenizer must not be null when message.AlignmentSetId is not null."),
                        new EngineStringDetokenizer(new LatinWordDetokenizer()),
                        metadatum.IsTargetRtl ?? throw new InvalidDataEngineException(name: "IsTargetRTL", value: "null"));


                    Verses.Add(verseDisplayViewModel);
                }

                Title = CreateParallelCorpusItemTitle(metadatum, "EnhancedView_Alignment", rows.Count);

            }
            catch (Exception)
            {
                Title = CreateNoVerseDataTitle(metadatum);
            }
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            await GetData(cancellationToken);
            await Task.CompletedTask;
        }

        private string CreateParallelCorpusItemTitle(ParallelCorpusEnhancedViewItemMetadatum metadatum, string localizationKey, int rowCount)
        {
            var title = $"{metadatum.ParallelCorpusDisplayName ?? string.Empty} {LocalizationStrings.Get(localizationKey, Logger!)}";

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
                            new ParallelCorpusId(Guid.Parse(metadatum.ParallelCorpusId!)));
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
            return await TranslationSet.Get(new TranslationSetId(Guid.Parse(translationSetId)), Mediator!);
        }
        public async Task<AlignmentSet> GetAlignmentSet(string alignmentSetId)
        {
            return await AlignmentSet.Get(new AlignmentSetId(Guid.Parse(alignmentSetId)), Mediator!);
        }
    }
}
