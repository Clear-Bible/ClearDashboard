using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DataAccessLayer.Features.Grammar;
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.Wpf.Application.Infrastructure;
using ClearDashboard.Wpf.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;

// ReSharper disable UnusedMember.Global

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class SplitTokenDialogViewModel : DashboardApplicationScreen
    {
        public VerseManager? VerseManager { get; }


        public BindableCollection<SplitInstructionViewModel> SplitInstructionsViewModels { get; private set; } = new();

        public BindableCollection<Grammar> GrammarSuggestions { get; private set; } = new();

        //private SplitInstructions? _splitInstructions;

        public SplitInstructionsViewModel SplitInstructionsViewModel
        {
            get => _splitInstructionsViewModel;
            private set => Set(ref _splitInstructionsViewModel, value);
        }

        public List<SplitTokenPropagationComboItem> PropagationOptions { get; } = new();

        public SplitTokenPropagationScope SelectedPropagationOption
        {
            get => _selectedPropagationOption;
            set => Set(ref _selectedPropagationOption, value);
        }

        public SplitTokenPropagationComboItem SelectedSplitTokenPropagationComboItem
        {
            get => _selectedPropagationKeyComboItem;
            set
            {
                Set(ref _selectedPropagationKeyComboItem, value);
                SelectedPropagationOption = value.SelectedValuePath;
            }
        }

        private FontFamily? _tokenFontFamily;
        public FontFamily? TokenFontFamily
        {
            get => _tokenFontFamily;
            set
            {
                _tokenFontFamily = value;
                NotifyOfPropertyChange();
            }
        }

        private TokenDisplayViewModel? _tokenDisplay;
        public TokenDisplayViewModel TokenDisplay
        {
            get
            {
                if (_tokenDisplay == null) throw new Exception("Cannot perform operation as TokenDisplay is null");
                return _tokenDisplay;
            }
            set
            {
                _tokenDisplay = value;
                //_splitInstructions ??= SplitInstructions.CreateSplits(_tokenDisplay.SurfaceText, []);
                SplitInstructionsViewModel =
                    SplitInstructionViewModelFactory.CreateSplits(_tokenDisplay.SurfaceText, [], LifetimeScope!);
                NotifyOfPropertyChange(nameof(TokenCharacters));
            }
        }

        private TokenCharacterViewModelCollection? _tokenCharacters;
        public TokenCharacterViewModelCollection TokenCharacters
        {
            get
            {
                return _tokenCharacters ??= new TokenCharacterViewModelCollection(TokenDisplay.SurfaceText);
            }
        }

        /// <summary>
        /// The <see cref="InterlinearDisplayViewModel"/> .
        /// </summary>
        public InterlinearDisplayViewModel? InterlinearDisplay { get; set; }

        /// <summary>
        /// The <see cref="Translation"/> to which this event pertains.
        /// </summary>
        public Translation? Translation { get; set; }

        public string TranslationActionType { get; set; } = string.Empty;


        private string _wordGloss = string.Empty;
        public string WordGloss
        {
            get => _wordGloss;
            set
            {
                _wordGloss = value;
                NotifyOfPropertyChange();
            }
        }

        public string DialogTitle => $"{LocalizationService!["EnhancedView_SplitToken"]}: {TokenDisplay.SurfaceText}";

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => Set(ref _progressBarVisibility, value);
        }

        public bool ApplyEnabled
        {
            get
            {
                var enabled = true;


                return enabled;
            }
        }

        private bool _cancelEnabled = true;
        private SplitInstructionsViewModel _splitInstructionsViewModel;
        private SplitTokenPropagationComboItem _selectedPropagationKeyComboItem;
        private SplitTokenPropagationScope _selectedPropagationOption = SplitTokenPropagationScope.All;

        public bool CancelEnabled
        {
            get => _cancelEnabled;
            private set => Set(ref _cancelEnabled, value);
        }

        //private bool _isLoaded;
        //public bool IsLoaded
        //{
        //    get => _isLoaded;
        //    private set => Set(ref _isLoaded, value);
        //}



        //public void CharacterClicked(object source, RoutedEventArgs args)
        //{
        //    var tokenCharacterArgs = args as TokenCharacterEventArgs;

        //    var tokenCharacter = tokenCharacterArgs!.TokenCharacter;
        //    var splitIndex = tokenCharacter.Index + 1;

        //    //SplitInstructionsViewModel.UpdateInstructions(tokenCharacter.Index, tokenCharacter.IsSelected);

        //    if (tokenCharacter.IsSelected)
        //    {

        //        if (!SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
        //        {

        //            var indexes = SplitInstructionsViewModel.SplitIndexes;
        //            indexes.Add(splitIndex);
        //            indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();

        //            var newSplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes, LifetimeScope);

        //            if (SplitInstructionsViewModel.Count > newSplitInstructionsViewModel.Count)
        //            {
        //                var removedItems =
        //                    SplitInstructionsViewModel.Instructions.Except(newSplitInstructionsViewModel.Instructions, new SplitInstructionComparer());

        //                foreach (var removedItem in removedItems)
        //                {
        //                    SplitInstructionsViewModel.Instructions.Remove(removedItem);
        //                }
        //            }
        //            else if (SplitInstructionsViewModel.Count > 0 && SplitInstructionsViewModel.Count < newSplitInstructionsViewModel.Count)
        //            {
        //                var addedItems =
        //                    newSplitInstructionsViewModel.Instructions.Except(SplitInstructionsViewModel.Instructions, new SplitInstructionComparer());
        //                foreach (var addedItem in addedItems)
        //                {
        //                    SplitInstructionsViewModel.Instructions.Add(addedItem);
        //                }

        //            }
        //            else
        //            {
        //                SplitInstructionsViewModel = newSplitInstructionsViewModel;

        //            }

        //            NotifyOfPropertyChange(nameof(ApplyEnabled));
        //        }
        //    }
        //    else
        //    {

        //        //if (SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
        //        {
        //            //SplitInstructionsViewModel.SplitIndexes.Remove(splitIndex);
        //            //var indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();
        //            //SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes, LifetimeScope!);
        //            //NotifyOfPropertyChange(nameof(ApplyEnabled));

        //            var indexes = SplitInstructionsViewModel.SplitIndexes;
        //            if (indexes.Contains(splitIndex))
        //            {
        //                indexes.Remove(splitIndex);

        //                indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();

        //                var newSplitInstructionsViewModel =
        //                    SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes,
        //                        LifetimeScope);

        //                var commonItems = SplitInstructionsViewModel.Instructions
        //                    .Intersect<SplitInstructionViewModel>(newSplitInstructionsViewModel.Instructions, new SplitInstructionComparer()).ToList();

        //                var removedItems =
        //                    SplitInstructionsViewModel.Instructions.Except(commonItems, new SplitInstructionComparer()).ToList();

        //                if (removedItems.Any())
        //                {
        //                  SplitInstructionsViewModel.Instructions.RemoveRange(removedItems);
        //                }
        //            }

        //            NotifyOfPropertyChange(nameof(ApplyEnabled));
        //        }

        //    }

        //}

        public void CharacterClicked(object source, RoutedEventArgs args)
        {
            var tokenCharacterArgs = args as TokenCharacterEventArgs;

            var tokenCharacter = tokenCharacterArgs!.TokenCharacter;
            var splitIndex = tokenCharacter.Index + 1;

            //SplitInstructionsViewModel.UpdateInstructions(tokenCharacter.Index, tokenCharacter.IsSelected);

            if (tokenCharacter.IsSelected)
            {

                if (!SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
                {
                    SplitInstructionsViewModel.SplitIndexes.Add(splitIndex);
                    var indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();

                    SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes, LifetimeScope);
                    NotifyOfPropertyChange(nameof(ApplyEnabled));
                }
            }
            else
            {

                if (SplitInstructionsViewModel!.SplitIndexes.Contains(splitIndex))
                {
                    SplitInstructionsViewModel.SplitIndexes.Remove(splitIndex);
                    var indexes = SplitInstructionsViewModel.SplitIndexes.OrderBy(i => i).ToList();
                    SplitInstructionsViewModel = SplitInstructionViewModelFactory.CreateSplits(TokenDisplay.SurfaceText, indexes, LifetimeScope!);
                    NotifyOfPropertyChange(nameof(ApplyEnabled));
                }

            }

        }

        private class SplitInstructionComparer : IEqualityComparer<SplitInstructionViewModel>
    
        {
            public bool Equals(SplitInstructionViewModel x, SplitInstructionViewModel y)
            {
                return x.TokenText.Equals(y.TokenText);
            }
            // GetHashCode() must return the same value for equal objects.
            public int GetHashCode(SplitInstructionViewModel vm)
            {
                return vm.TokenText.GetHashCode();
            }
        }

        public async void ApplySplit()
        {
            try
            {
                OnUIThread(() =>
                {
                    ProgressBarVisibility = Visibility.Visible;
                    CancelEnabled = false;
                });

                async Task SaveSplitToken()
                {
                    // TODO: Implement this method

                    //await VerseManager.SplitToken(TokenDisplay.Corpus!,
                    //    TokenDisplay.Token.TokenId,
                    //    Subtoken3Enabled ? CharacterThreshold1 : 0,
                    //    Subtoken3Enabled ? CharacterThreshold2 : CharacterThreshold1,
                    //    Subtoken1,
                    //    Subtoken2,
                    //    Subtoken3,
                    //    !TokenDisplay.IsCorpusView,
                    //    SelectedPropagationOption);
                    await Task.CompletedTask;
                }
                await Task.Run(async () => await SaveSplitToken());
            }
            finally
            {
                OnUIThread(() => ProgressBarVisibility = Visibility.Collapsed);
                await TryCloseAsync(true);
            }
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var results = await Mediator!.Send(new GetGrammarSuggestionsQuery(ProjectManager!.CurrentProject!.ProjectName!), cancellationToken);
            if (results is { Success: true, HasData: true })
            {
                GrammarSuggestions = new BindableCollection<Grammar>(results.Data);
            }

            SelectedSplitTokenPropagationComboItem = PropagationOptions.Last();

            await base.OnActivateAsync(cancellationToken);
        }

        public async void CancelSplit()
        {
            await TryCloseAsync(false);
        }


        public dynamic DialogSettings()
        {
            dynamic settings = new ExpandoObject();
            settings.WindowStyle = WindowStyle.SingleBorderWindow;
            settings.ShowInTaskbar = false;
            settings.WindowState = WindowState.Normal;
            settings.ResizeMode = ResizeMode.CanResizeWithGrip;
            settings.PopupAnimation = PopupAnimation.Fade;
            settings.WindowStartupLocation = WindowStartupLocation.Manual;
            //settings.Top = 0;
            settings.Left = System.Windows.Application.Current.MainWindow!.ActualWidth / 2 - 258;
            settings.Width = 1000;
            settings.Height = 820;
            settings.Title = DialogTitle;

            // Keep the window on top
            //settings.Topmost = true;
            settings.Owner = System.Windows.Application.Current.MainWindow;

            return settings;
        }

        public SplitTokenDialogViewModel()
        {
            // Required for designer support.
        }

        public SplitTokenDialogViewModel(
            DashboardProjectManager? projectManager,
            VerseManager verseManager,
            INavigationService navigationService,
            ILogger<SplitTokenDialogViewModel> logger,
            IEventAggregator eventAggregator,
            IMediator mediator,
            ILifetimeScope? lifetimeScope,
            ILocalizationService localizationService)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            VerseManager = verseManager;
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.None, DisplayMemberPath = localizationService["None"] });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapterVerse, DisplayMemberPath = $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Verse"]}" });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapter, DisplayMemberPath = $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Chapter"]}" });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapter, DisplayMemberPath = $"{localizationService["Current"]} {localizationService["BiblicalTermsBcv_Book"]}" });
            PropagationOptions.Add(new SplitTokenPropagationComboItem { SelectedValuePath = SplitTokenPropagationScope.BookChapter, DisplayMemberPath = localizationService["BiblicalTermsBcv_All"] });

            SelectedSplitTokenPropagationComboItem = PropagationOptions.Last();
        }
    }

    public class SplitTokenPropagationComboItem : PropertyChangedBase
    {
        private string? _displayMemberPath;
        private SplitTokenPropagationScope _selectedValuePath;

        public SplitTokenPropagationScope SelectedValuePath
        {
            get => _selectedValuePath;
            set => Set(ref _selectedValuePath, value);
        }

        public string? DisplayMemberPath
        {
            get => _displayMemberPath;
            set => Set(ref _displayMemberPath, value);
        }
    }
}
