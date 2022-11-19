// Uncomment this preprocessor definition to use mock data for dev/test purposes.
//#define MOCK

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Display.Messages;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class EnhancedViewDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        #region Demo data - for demo page only
        public EngineStringDetokenizer Detokenizer { get; set; } = new EngineStringDetokenizer(new LatinWordDetokenizer());

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
                Logger?.LogInformation($"Retrieved parallel corpus {corpus.ParallelCorpusId.Id} in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.Seconds} seconds)");
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
                Logger?.LogInformation($"Retrieved parallel corpus verse {bbbcccvvv} in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.Seconds} seconds)");
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
                var translationSet = await TranslationSet.Get(translationSetIds.First(), Mediator!);
                stopwatch.Stop();
                Logger?.LogInformation($"Retrieved first translation set {translationSet.TranslationSetId.Id} in {stopwatch.ElapsedMilliseconds} ms ({stopwatch.Elapsed.Seconds} seconds)");
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
                //await ProjectManager!.LoadProject("EnhancedViewDemo");
                await ProjectManager!.LoadProject("EnhancedViewDemo2");
                //var row = await GetVerseTextRow(40001001);
                var row = await GetVerseTextRow(001001001);
                var translationSet = await GetFirstTranslationSet();
                //await VerseDisplayViewModel!.BindAsync(row, translationSet, Detokenizer);

                await VerseDisplayViewModel!.ShowTranslationAsync(row, translationSet, Detokenizer, false);
#endif
                NotifyOfPropertyChange(nameof(VerseDisplayViewModel));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion Demo data

        private IServiceProvider ServiceProvider { get; }

        public NoteManager NoteManager { get; }
        public VerseDisplayViewModel VerseDisplayViewModel { get; set; }

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => Set(ref _message, value);
        }

        private TokenDisplayViewModel _tokenForTranslation;
        public TokenDisplayViewModel TokenForTranslation
        {
            get => _tokenForTranslation;
            set => Set(ref _tokenForTranslation, value);
        }

        private TokenDisplayViewModelCollection _selectedTokens = new();
        public TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set => Set(ref _selectedTokens, value);
        }

        private IEnumerable<TranslationOption> _translationOptions;
        public IEnumerable<TranslationOption> TranslationOptions
        {
            get => _translationOptions;
            set => Set(ref _translationOptions, value);
        }

        private TranslationOption? _currentTranslationOption;
        public TranslationOption? CurrentTranslationOption
        {
            get => _currentTranslationOption;
            set => Set(ref _currentTranslationOption, value);
        }

        private Visibility _notePaneVisibility = Visibility.Collapsed;
        public Visibility NotePaneVisibility
        {
            get => _notePaneVisibility;
            set => Set(ref _notePaneVisibility, value);
        }

        private Visibility _translationPaneVisibility = Visibility.Collapsed;
        public Visibility TranslationPaneVisibility
        {
            get => _translationPaneVisibility;
            set => Set(ref _translationPaneVisibility, value);
        }

        private async Task DisplayTranslationPane(TranslationEventArgs e)
        {
            TokenForTranslation = e.TokenDisplayViewModel;
            TranslationOptions = await VerseDisplayViewModel.GetTranslationOptionsAsync(e.Translation);
            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText) ?? null;
            TranslationPaneVisibility = Visibility.Visible;
        }

        private static string GetModifierKeysText(ModifierKeys modifierKeys)
        {
            var modifiers = string.Empty;
            if ((modifierKeys & ModifierKeys.Control) > 0) modifiers += "Ctrl+";
            if ((modifierKeys & ModifierKeys.Shift) > 0) modifiers += "Shift+";
            if ((modifierKeys & ModifierKeys.Alt) > 0) modifiers += "Alt+";
            if ((modifierKeys & ModifierKeys.Windows) > 0) modifiers += "Win+";
            return modifiers;
        }

        private void UpdateSelection(TokenDisplayViewModel token, TokenDisplayViewModelCollection selectedTokens, bool addToSelection)
        {
            if (addToSelection)
            {
                foreach (var selectedToken in selectedTokens)
                {
                    if (!SelectedTokens.Contains(selectedToken))
                    {
                        SelectedTokens.Add(selectedToken);
                    }
                }
                if (!token.IsSelected)
                {
                    SelectedTokens.Remove(token);
                }
            }
            else
            {
                SelectedTokens = selectedTokens;
            }
            EventAggregator.PublishOnUIThreadAsync(new SelectionUpdatedMessage(SelectedTokens));
        }

        #region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            Task.Run(() => TokenClickedAsync(e).GetAwaiter());
        }

        public async Task TokenClickedAsync(TokenEventArgs e)
        {
            UpdateSelection(e.TokenDisplayViewModel, e.SelectedTokens, (e.ModifierKeys & ModifierKeys.Control) > 0);
            await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NotePaneVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplayViewModel.SurfaceText}' token ({e.TokenDisplayViewModel.Token.TokenId}) {GetModifierKeysText(e.ModifierKeys)}clicked";
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Task.Run(() => TokenRightButtonDownAsync(e).GetAwaiter());
        }

        public async Task TokenRightButtonDownAsync(TokenEventArgs e)
        {
            UpdateSelection(e.TokenDisplayViewModel, e.SelectedTokens, false);
            await NoteManager.SetCurrentNoteIds(SelectedTokens.NoteIds);
            NotePaneVisibility = SelectedTokens.Any(t => t.HasNote) ? Visibility.Visible : Visibility.Collapsed;
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) right-clicked";
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
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

        public void NoteLeftButtonDown(NoteEventArgs e)
        {
            e.TokenDisplayViewModel.IsSelected = true;
            SelectedTokens = new TokenDisplayViewModelCollection(e.TokenDisplayViewModel);
            NotePaneVisibility = Visibility.Visible;
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
        }

        public void NoteCreate(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public void FilterPins(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public void TranslateQuick(NoteEventArgs e)
        {
            NotePaneVisibility = Visibility.Visible;
        }

        public async Task TranslationApplied(TranslationEventArgs e)
        {
            await VerseDisplayViewModel.PutTranslationAsync(e.Translation, e.TranslationActionType);
            NotifyOfPropertyChange(nameof(VerseDisplayViewModel));

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
            await NoteManager.AddNoteAsync(e.Note, e.EntityIds);
            Message = $"Note '{e.Note.Text}' added to tokens {string.Join(", ",e.EntityIds.Select(id => id.ToString()))}";
        }

        public async Task NoteDeleted(NoteEventArgs e)
        {
            await NoteManager.DeleteNoteAsync(e.Note, e.EntityIds);
            Message = $"Note '{e.Note.Text}' deleted from tokens ({string.Join(", ", e.EntityIds.Select(id => id.ToString()))})";
        }

        public async Task NoteEditorMouseEnter(NoteEventArgs e)
        {
            await NoteManager.NoteMouseEnterAsync(e.Note, e.EntityIds);
        }

        public async Task NoteEditorMouseLeave(NoteEventArgs e)
        {
            await NoteManager.NoteMouseLeaveAsync(e.Note, e.EntityIds);
        }

        public async Task NoteUpdated(NoteEventArgs e)
        {
            await NoteManager.UpdateNoteAsync(e.Note);
            Message = $"Note '{e.Note.Text}' updated on tokens {string.Join(", ", e.EntityIds.Select(id => id.ToString()))}";
        }

        public async Task LabelAdded(LabelEventArgs e)
        {
            // If this is a new note, we'll handle any labels when the note is added.
            if (e.Note.NoteId != null)
            {
                await NoteManager.CreateAssociateNoteLabelAsync(e.Note, e.Label.Text);
            }
            Message = $"Label '{e.Label.Text}' added for note";
        }

        public async Task LabelSelected(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.AssociateNoteLabelAsync(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' selected for note";
        }

        public async Task LabelRemoved(LabelEventArgs e)
        {
            if (e.Note.NoteId != null)
            {
                await NoteManager.DetachNoteLabel(e.Note, e.Label);
            }
            Message = $"Label '{e.Label.Text}' removed for note";
        }

        public void NoteAssociationClicked(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' clicked";
        }

        public void NoteAssociationDoubleClicked(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' double-clicked";
        }

        public void NoteAssociationRightButtonDown(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' right button down";
        }

        public void NoteAssociationMouseEnter(NoteAssociationEventArgs e)
        {
            Message = $"Note association '{e.AssociatedEntityId}' hovered";
        }

        public void NoteAssociationMouseLeave(NoteAssociationEventArgs e)
        {
            Message = string.Empty;
        }

        public void CloseNotePaneRequested(RoutedEventArgs args)
        {
            NotePaneVisibility = Visibility.Collapsed;
            SelectedTokens = new TokenDisplayViewModelCollection();
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

        public EnhancedViewDemoViewModel(INavigationService navigationService, ILogger<EnhancedViewDemoViewModel> logger, DashboardProjectManager projectManager, NoteManager noteManager, IEventAggregator eventAggregator, IMediator mediator, IServiceProvider serviceProvider, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            NoteManager = noteManager;
            ServiceProvider = serviceProvider;
        }
#pragma warning restore CS8618
    }
}
