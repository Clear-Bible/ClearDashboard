// Uncomment this preprocessor definition to use mock data for dev/test purposes.
//#define MOCK

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Corpora;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SIL.Machine.Tokenization;
using SIL.Scripture;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class EnhancedViewDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        public async Task<ParallelCorpus> GetParallelCorpus(ParallelCorpusId? corpusId = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                if (corpusId == null)
                {
                    var corpusIds = await ParallelCorpus.GetAllParallelCorpusIds(Mediator!);
                    corpusId = corpusIds.First();
                }
                var corpus = await ParallelCorpus.Get(Mediator!, corpusId);

                Detokenizer = corpus.Detokenizer;
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved parallel corpus {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms");
                return corpus;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<EngineParallelTextRow?> GetVerseTextRow(int bbbcccvvv, ParallelCorpusId? corpusId = null)
        {
            try
            {
                var corpus = await GetParallelCorpus(corpusId);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var verse = corpus.GetByVerseRange(new VerseRef(bbbcccvvv), 0, 0);
                var row = verse.parallelTextRows.FirstOrDefault() as EngineParallelTextRow;
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved parallel corpus verse {bbbcccvvv} in {stopwatch.ElapsedMilliseconds} ms");
                return row;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<TranslationSet?> GetFirstTranslationSet()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var translationSetIds = await TranslationSet.GetAllTranslationSetIds(Mediator!);
                var translationSet = await TranslationSet.Get(translationSetIds.First().translationSetId, Mediator!);
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved first translation set {translationSet.TranslationSetId.Id} in {stopwatch.ElapsedMilliseconds} ms");
                return translationSet;
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e.ToString());
                throw;
            }
        }

        private async Task PopulateData()
        {
            try
            {
                VerseDisplayViewModel = ServiceProvider!.GetService<VerseDisplayViewModel>();
#if MOCK
                await VerseDisplayViewModel.BindMockVerseAsync();
#else
                await ProjectManager!.LoadProject("EnhancedViewDemo");
                var row = await GetVerseTextRow(01001001);
                var translationSet = await GetFirstTranslationSet();
                await VerseDisplayViewModel!.BindAsync(row, translationSet, Detokenizer);
#endif
                NotifyOfPropertyChange(nameof(VerseDisplayViewModel));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IServiceProvider ServiceProvider { get; }

        public VerseDisplayViewModel VerseDisplayViewModel { get; set; } = new();
        public EngineStringDetokenizer Detokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());

        private string _message = string.Empty;
        private Visibility _notePaneVisibility = Visibility.Collapsed;
        private Visibility _translationPaneVisibility = Visibility.Collapsed;
        private TokenDisplayViewModel _currentToken;
        private IEnumerable<TranslationOption> _translationOptions;
        private TranslationOption? _currentTranslationOption;
        private TokenDisplayViewModelCollection _selectedTokens = new();

        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        public TokenDisplayViewModel CurrentToken
        {
            get => _currentToken;
            set => Set(ref _currentToken, value);
        }

        public TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set
            {
                Set(ref _selectedTokens, value);
                NotifyOfPropertyChange(nameof(SelectedTokensSurfaceText));
            }
        }

        public string SelectedTokensSurfaceText => String.Join(",", SelectedTokens.Select(t => t.SurfaceText));

        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            set => Set(ref _translationOptions, value);
        }

        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            set => Set(ref _currentTranslationOption, value);
        }

        public Visibility NotePaneVisibility
        {
            get => _notePaneVisibility;
            set => Set(ref _notePaneVisibility, value);
        }

        public Visibility TranslationPaneVisibility
        {
            get => _translationPaneVisibility;
            set => Set(ref _translationPaneVisibility, value);
        }

        private async Task DisplayTranslationPane(TranslationEventArgs e)
        {
            CurrentToken = e.TokenDisplayViewModel;
            TranslationOptions = await VerseDisplayViewModel.GetTranslationOptionsAsync(e.Translation);
            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText) ?? null;
            TranslationPaneVisibility = Visibility.Visible;
        }

        private void DisplayNotePane(TokenDisplayViewModel tokenDisplayViewModel)
        {
            CurrentToken = tokenDisplayViewModel;
            NotePaneVisibility = Visibility.Visible;
        }

#region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            SelectedTokens = e.SelectedTokens;
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) clicked";
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            if (e.TokenDisplayViewModel.HasNote)
            {
                DisplayNotePane(e.TokenDisplayViewModel);
            }

            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) hovered";
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
        }

        public async Task TranslationClicked(TranslationEventArgs e)
        {
            await DisplayTranslationPane(e);
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} clicked";
        }

        public void TranslationMouseEnter(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token {e.Translation.SourceToken.TokenId} hovered";
        }

        public void TranslationMouseLeave(TranslationEventArgs e)
        {
            Message = string.Empty;
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
            DisplayNotePane(e.TokenDisplayViewModel);
            Message = $"'Note indicator for token {e.EntityId} hovered";
        }

        public void NoteCreate(NoteEventArgs e)
        {
            DisplayNotePane(e.TokenDisplayViewModel);
            Message = $"Opening new note panel for token {e.EntityId}";
        }

        public async Task TranslationApplied(TranslationEventArgs e)
        {
            await VerseDisplayViewModel.PutTranslationAsync(e.Translation, e.TranslationActionType);

            Message = $"Translation '{e.Translation.TargetTranslationText}' ({e.TranslationActionType}) applied to token '{e.TokenDisplayViewModel.SurfaceText}' ({e.TokenDisplayViewModel.Token.TokenId})";
            TranslationPaneVisibility = Visibility.Collapsed;
        }

        public void TranslationCancelled(RoutedEventArgs e)
        {
            Message = "Translation cancelled.";
            TranslationPaneVisibility = Visibility.Collapsed;
        }        
        
        public async Task NoteAdded(NoteEventArgs e)
        {
            await VerseDisplayViewModel.AddNoteAsync(e.Note, e.EntityId);
            Message = $"Note '{e.Note.Text}' added to token ({e.EntityId})";
        }

        public async Task NoteUpdated(NoteEventArgs e)
        {
            await VerseDisplayViewModel.UpdateNoteAsync(e.Note);
            Message = $"Note '{e.Note.Text}' updated on token ({e.EntityId})";
        }

        public async Task NoteDeleted(NoteEventArgs e)
        {
            await VerseDisplayViewModel.DeleteNoteAsync(e.Note, e.EntityId);
            Message = $"Note '{e.Note.Text}' deleted from token ({e.EntityId})";
        }

        public async Task LabelAdded(LabelEventArgs e)
        {
            // If this is a new note, we'll handle any labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await VerseDisplayViewModel.CreateAssociateNoteLabelAsync(e.Note, e.Label.Text);
            }
            Message = $"Label '{e.Label.Text}' added for note on token ({e.EntityId})";
        }

        public async Task LabelSelected(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await VerseDisplayViewModel.AssociateNoteLabelAsync(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' selected for note on token ({e.EntityId})";
        }

        public void CloseNotePaneRequested(RoutedEventArgs args)
        {
            NotePaneVisibility = Visibility.Collapsed;
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await PopulateData();
        }

#endregion

#pragma warning disable CS8618
        /// <summary>
        /// Default constructor for design-time support only.
        /// </summary>
        public EnhancedViewDemoViewModel()
        {
        }

        public EnhancedViewDemoViewModel(INavigationService navigationService, ILogger<EnhancedViewDemoViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, IServiceProvider serviceProvider, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            ServiceProvider = serviceProvider;
        }
#pragma warning restore CS8618
    }
}
