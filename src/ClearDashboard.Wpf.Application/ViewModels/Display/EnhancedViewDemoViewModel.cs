using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.Wpf.Application.Events;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.DataAccessLayer.Wpf.Infrastructure;
using Autofac;
using Caliburn.Micro;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class EnhancedViewDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        #region Mock data

        private static readonly string _testDataPath = Path.Combine(AppContext.BaseDirectory, "Data");
        private static readonly string _usfmTestProjectPath = Path.Combine(_testDataPath, "usfm", "Tes");
        private static IEnumerable<TokensTextRow>? _mockCorpus;
        private static IEnumerable<TokensTextRow>? MockCorpus => _mockCorpus ??= new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, _usfmTestProjectPath)
                                                                                                        .Tokenize<LatinWordTokenizer>()
                                                                                                        .Transform<IntoTokensTextRowProcessor>()
                                                                                                        .Transform<SetTrainingBySurfaceTokensTextRowProcessor>()
                                                                                                        .Cast<TokensTextRow>();
        // ReSharper disable once InconsistentNaming
        private static TokensTextRow? MockVerseTextRow(int BBBCCCVVV)
        {
            return MockCorpus?.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
        }

        private static readonly List<string> _mockOogaWords = new() { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foodie", "fingle", "boing", "la" };
        private static int _mockTranslationWordIndexer;
        private static string MockTranslationWord
        {
            get
            {
                var result = _mockOogaWords[_mockTranslationWordIndexer++];
                if (_mockTranslationWordIndexer == _mockOogaWords.Count) _mockTranslationWordIndexer = 0;
                return result;
            }
        }

        private static string MockTranslationStatus => new Random().Next(3) switch
                                                            {
                                                                0 => "FromTranslationModel",
                                                                1 => "FromOther",
                                                                _ => "Assigned"
                                                            };

        private static ObservableCollection<Label> MockLabelSuggestions => new()
                                                            {
                                                                new Label { Text = "alfa" },
                                                                new Label { Text = "bravo" },
                                                                new Label { Text = "charlie" },
                                                                new Label { Text = "delta" },
                                                                new Label { Text = "echo" }
                                                            };

        #endregion

        private ObservableCollection<Label> GetLabelSuggestions()
        {
            return MockLabelSuggestions;
        }

        private IEnumerable<(Token token, string paddingBefore, string paddingAfter)>? GetPaddedTokens(TokensTextRow textRow)
        {
            var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
            return detokenizer.Detokenize(textRow.Tokens);
        }

        private IEnumerable<(Token token, string paddingBefore, string paddingAfter)>? GetPaddedTokens(IEnumerable<TokensTextRow>? corpus, int BBBCCCVVV)
        {
            var textRow = corpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
            if (textRow != null)
            {
                var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
                return detokenizer.Detokenize(textRow.Tokens);
            }

            return null;
        }

        private async Task LoadFiles()
        {
            var corpus = MockCorpus;

            VerseTokens = GetTokenDisplays(corpus, 40001001);
            NotifyOfPropertyChange(nameof(VerseTokens));

            var note1 = new Note
            {
                Text = "This is a note",
                //NoteId = new NoteId(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new UserId(Guid.NewGuid())),
                //Labels = GetLabels()
            };

            var note2 = new Note
            {
                Text = "Here's another note",
                //NoteId = new NoteId(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, new UserId(Guid.NewGuid())),
                //Labels = GetLabels()
            };

            VerseTokens.First().Notes.Add(note1);
            VerseTokens.First().Notes.Add(note2);

            //NotifyOfPropertyChange(nameof(CurrentNote));
        }

        private Visibility _translationControlVisibility = Visibility.Collapsed;


        public IEnumerable<TokenDisplayViewModel>? VerseTokens { get; set; }
        public IEnumerable<TranslationOption> TranslationOptions { get; set; }
        public IEnumerable<Label> LabelSuggestions { get; set; }
        public TokenDisplayViewModel CurrentToken { get; set; }
        public TranslationOption CurrentTranslationOption { get; set; }

        public string? Message { get; set; }

        public Visibility NoteControlVisibility { get; set; } = Visibility.Collapsed;

        public Visibility TranslationControlVisibility
        {
            get => _translationControlVisibility;
            set => _translationControlVisibility = value;
        }


        // ReSharper disable UnusedMember.Global
        public EnhancedViewDemoViewModel()
        {
        }

        public EnhancedViewDemoViewModel(INavigationService navigationService, ILogger<EnhancedViewDemoViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
        }

        #region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenDoubleClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            if (e.TokenDisplayViewModel.HasNote)
            {
                DisplayNotePane(e.TokenDisplayViewModel);
            }

            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseWheel(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplayViewModel?.SurfaceText}' token ({e.TokenDisplayViewModel?.Token.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationClicked(TranslationEventArgs e)
        {
            DisplayTranslationPane(e);

            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationDoubleClicked(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationRightButtonDown(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseEnter(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseLeave(TranslationEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationMouseWheel(TranslationEventArgs e)
        {
            Message = $"'{e.Translation.TargetTranslationText}' translation for token ({e.Translation.SourceToken.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteLeftButtonDown(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteDoubleClicked(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteRightButtonDown(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseLeave(NoteEventArgs e)
        {
            //Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseWheel(NoteEventArgs e)
        {
            //Message = $"'{e.Note.Text}' note for token ({e.EntityId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteCreate(NoteEventArgs e)
        {
            //DisplayNote(e.TokenDisplayViewModel);
        }

        public void TranslationApplied(TranslationEventArgs e)
        {
            Message = $"Translation '{e.Translation.TargetTranslationText}' ({e.TranslationActionType}) applied to token '{e.TokenDisplayViewModel.SurfaceText}' ({e.TokenDisplayViewModel.Token.TokenId})";
            NotifyOfPropertyChange(nameof(Message));

            TranslationControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
        }

        public void TranslationCancelled(RoutedEventArgs e)
        {
            Message = "Translation cancelled.";
            NotifyOfPropertyChange(nameof(Message));

            TranslationControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
        }        
        
        public void NoteAdded(NoteEventArgs e)
        {
            Message = $"Note '{e.Note.Text}' added to token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void NoteUpdated(NoteEventArgs e)
        {
            Message = $"Note '{e.Note.Text}' updated on token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void NoteDeleted(NoteEventArgs e)
        {
            Message = $"Note '{e.Note.Text}' deleted from token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void LabelSelected(LabelEventArgs e)
        {
            Message = $"Label '{e.Label.Text}' selected for token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void LabelAdded(LabelEventArgs e)
        {
            Message = $"Label '{e.Label.Text}' added for token ({e.EntityId})";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void CloseRequested(RoutedEventArgs args)
        {
            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            LabelSuggestions = GetLabelSuggestions();
            GetLabels();

            await base.OnActivateAsync(cancellationToken);
            await LoadFiles();
        }


        // ReSharper restore UnusedMember.Global

        #endregion

        private ObservableCollection<Label> GetLabels()
        {
            var labelSuggestions = LabelSuggestions.ToList();
            var labelTexts = new List<string>();
            var numLabels = new Random().Next(4);

            for (var i = 0; i <= numLabels; i++)
            {
                var labelIndex = new Random().Next(LabelSuggestions.Count());
                if (!labelTexts.Contains(labelSuggestions[labelIndex].Text))
                {
                    labelTexts.Add(labelSuggestions[labelIndex].Text);
                }
            }

            return new ObservableCollection<Label>(labelTexts.OrderBy(lt => lt).Select(lt => new Label {Text = lt}));
        }        
        
        private Translation GetTranslation(Token token)
        {
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? MockTranslationWord
                : String.Empty;
            var translation = new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationOriginatedFrom: MockTranslationStatus);
            
            return translation;
        }
        
        private IEnumerable<TranslationOption> GetMockTranslationOptions(string sourceTranslation)
        {
            var result = new List<TranslationOption>();

            var random = new Random();
            var optionCount = random.Next(4) + 2;     // 2-5 options
            var remainingPercentage = 100d;

            var basePercentage = random.NextDouble() * remainingPercentage;
            result.Add(new TranslationOption {Word = sourceTranslation, Probability = basePercentage});
            remainingPercentage -= basePercentage;

            for (var i = 1; i < optionCount - 1; i++)
            {
                var percentage = random.NextDouble() * remainingPercentage;
                result.Add(new TranslationOption { Word = MockTranslationWord, Probability = percentage });
                remainingPercentage -= percentage;
            }

            result.Add(new TranslationOption { Word = MockTranslationWord, Probability = remainingPercentage });

            return result.OrderByDescending(to => to.Probability);
        }

        private List<TokenDisplayViewModel> GetTokenDisplays(IEnumerable<TokensTextRow>? corpus, int BBBCCCVVV)
        {
            var tokenDisplays = new List<TokenDisplayViewModel>();

            var tokens = GetPaddedTokens(corpus, BBBCCCVVV);
            if (tokens != null)
            {
                tokenDisplays.AddRange(from token in tokens 
                    let translation = GetTranslation(token.token) 
                    select new TokenDisplayViewModel { Token = token.token, PaddingBefore = token.paddingBefore, PaddingAfter = token.paddingAfter, Translation = translation });
            }

            return tokenDisplays;
        }

        private void SetCurrentToken(TokenDisplayViewModel viewModel)
        {
            CurrentToken = viewModel;
            NotifyOfPropertyChange(nameof(CurrentToken));
        }

        private void DisplayTranslationPane(TranslationEventArgs e)
        {
            SetCurrentToken(e.TokenDisplayViewModel);

            TranslationControlVisibility = Visibility.Visible;
            NotifyOfPropertyChange(nameof(TranslationControlVisibility));

            TranslationOptions = GetMockTranslationOptions(e.Translation.TargetTranslationText);
            NotifyOfPropertyChange(nameof(TranslationOptions));

            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText) ?? null;
            NotifyOfPropertyChange(nameof(CurrentTranslationOption));

        }

        private void DisplayNotePane(TokenDisplayViewModel tokenDisplayViewModel)
        {
            NoteControlVisibility = Visibility.Visible;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));

            SetCurrentToken(tokenDisplayViewModel);
        }
    }
}
