using System;
using System.Collections.Generic;
using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
using ClearDashboard.DAL.Alignment.Features.Notes;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
//using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;
using CorpusClass = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using EngineToken = ClearBible.Engine.Corpora.Token;
using ClearDashboard.Wpf.Application.Events;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class EnhancedViewDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        private static readonly string _testDataPath = Path.Combine(AppContext.BaseDirectory, "Data");
        private static readonly string _usfmTestProjectPath = Path.Combine(_testDataPath, "usfm", "Tes");
        private static readonly string _greekNtUsfmTestProjectPath = Path.Combine(_testDataPath, "usfm", "nestle1904");

        private static ITextCorpus GetSampleEnglishTextCorpus()
        {
            return new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, _usfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Transform<SetTrainingBySurfaceTokensTextRowProcessor>();
        }

        private static ITextCorpus GetSampleGreekTextCorpus()
        {
            return new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, _greekNtUsfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>();
        }

        public string? Message { get; set; }
        public IEnumerable<TokenDisplay>? Verse1 { get; set; }
        public TokenDisplay CurrentTokenDisplay { get; set; }
        public IEnumerable<TranslationOption> TranslationOptions { get; set; }
        public TranslationOption CurrentTranslationOption { get; set; }

        public IEnumerable<Label> SampleLabels { get; set; }

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
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenDoubleClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            if (e.TokenDisplay.HasNote)
            {
                DisplayNote(e.TokenDisplay);
            }

            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseWheel(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay?.SurfaceText}' token ({e.TokenDisplay?.Token.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TranslationClicked(TranslationEventArgs e)
        {
            DisplayTranslation(e);

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
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteDoubleClicked(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteRightButtonDown(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseEnter(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) hovered";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseLeave(NoteEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteMouseWheel(NoteEventArgs e)
        {
            Message = $"'{e.TokenDisplay.Note}' note for token ({e.TokenDisplay.Token.TokenId}) mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void NoteCreate(NoteEventArgs e)
        {
            DisplayNote(e.TokenDisplay);
        }

        public void TranslationApplied(TranslationEventArgs e)
        {
            Message = $"Translation '{e.Translation.TargetTranslationText}' ({e.TranslationActionType}) applied to token '{e.TokenDisplay.SurfaceText}' ({e.TokenDisplay.Token.TokenId})";
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
        
        public void NoteApplied(NoteEventArgs e)
        {
            Message = $"Note '{e.TokenDisplay.Note}' applied to token '{e.TokenDisplay.SurfaceText}' ({e.TokenDisplay.Token.TokenId})";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }

        public void NoteCancelled(RoutedEventArgs e)
        {
            Message = "Note cancelled.";
            NotifyOfPropertyChange(nameof(Message));

            NoteControlVisibility = Visibility.Hidden;
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
        }
        // ReSharper restore UnusedMember.Global

        #endregion


        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            CreateLabels();

            await base.OnActivateAsync(cancellationToken);
            await LoadFiles();
            //await MockProjectAndUser();
            //await RetrieveTokensViaCorpusClass();
        }

        private void CreateLabels()
        {
            var labels = new List<Label>();
            for (var i = 1; i <= 5; i++)
            {
                labels.Add(new Label { Text = $"Label{i}" });
            }

            SampleLabels = labels;
        }


        private readonly List<string> MockOogaWords = new() { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foo", "foodie", "fingle", "boing", "la" };

        private string RandomTranslationOriginatedFrom()
        {
            switch (new Random().Next(3))
            {
                case 0: return "FromTranslationModel";
                case 1: return "FromOther";
                default: return "Assigned";
            }
        }

        private static int mockOogaWordsIndexer_;

        private string GetMockOogaWord()
        {
            var result = MockOogaWords[mockOogaWordsIndexer_++];
            if (mockOogaWordsIndexer_ == MockOogaWords.Count) mockOogaWordsIndexer_ = 0;
            return result;
        }

        private Translation GetTranslation(Token token)
        {
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? GetMockOogaWord()
                : String.Empty;
            var translation = new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationOriginatedFrom: RandomTranslationOriginatedFrom());
            
            return translation;
        }
        
        private IEnumerable<(EngineToken token, string paddingBefore, string paddingAfter)>? GetTokens(IEnumerable<TokensTextRow> corpus, int BBBCCCVVV)
        {
            var textRow = corpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
            if (textRow != null)
            {
                var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
                return detokenizer.Detokenize(textRow.Tokens);
            }

            return null;
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
                result.Add(new TranslationOption { Word = GetMockOogaWord(), Probability = percentage });
                remainingPercentage -= percentage;
            }

            result.Add(new TranslationOption { Word = GetMockOogaWord(), Probability = remainingPercentage });

            return result.OrderByDescending(to => to.Probability);
        }

        private List<TokenDisplay> GetTokenDisplays(IEnumerable<TokensTextRow> corpus, int BBBCCCVVV)
        {
            var tokenDisplays = new List<TokenDisplay>();

            var tokens = GetTokens(corpus, BBBCCCVVV);
            if (tokens != null)
            {
                tokenDisplays.AddRange(from token in tokens 
                    let translation = GetTranslation(token.token) 
                    select new TokenDisplay { Token = token.token, PaddingBefore = token.paddingBefore, PaddingAfter = token.paddingAfter, Translation = translation });
            }

            return tokenDisplays;
        }

        //private List<Note> GetNotesForEntity(TokenId id)
        //{
        //    var noteIds = GetAllNoteDomainEntityAssociationsQuery.Where()
        //}

        //private void Foo()
        //{
        //    Dictionary<NoteId, Note> dictionary_;
            
        //    List<>
        //}

        public Visibility TranslationControlVisibility { get; set; } = Visibility.Collapsed;

        private void DisplayTranslation(TranslationEventArgs e)
        {
            TranslationControlVisibility = Visibility.Visible;

            CurrentTokenDisplay = e.TokenDisplay;
            TranslationOptions = GetMockTranslationOptions(e.Translation.TargetTranslationText);
            CurrentTranslationOption = TranslationOptions.FirstOrDefault(to => to.Word == e.Translation.TargetTranslationText);

            NotifyOfPropertyChange(nameof(TranslationControlVisibility));
            NotifyOfPropertyChange(nameof(CurrentTokenDisplay));
            NotifyOfPropertyChange(nameof(TranslationOptions));
            NotifyOfPropertyChange(nameof(CurrentTranslationOption));
        }

        public Visibility NoteControlVisibility { get; set; } = Visibility.Collapsed;
        private void DisplayNote(TokenDisplay tokenDisplay)
        {
            NoteControlVisibility = Visibility.Visible;
            CurrentTokenDisplay = tokenDisplay;
        
            NotifyOfPropertyChange(nameof(NoteControlVisibility));
            NotifyOfPropertyChange(nameof(CurrentTokenDisplay));
        }

        private async Task LoadFiles()
        {
            var corpus = GetSampleEnglishTextCorpus().Cast<TokensTextRow>();

            Verse1 = GetTokenDisplays(corpus, 40001001);
            Verse1.First().Note = "This is a note";
            Verse1.Skip(3).First().Note = "Here's another note.";
            
            NotifyOfPropertyChange(nameof(Verse1));
        }

        private async Task MockProjectAndUser()
        {
            //ProjectManager.CurrentProject = new DataAccessLayer.Models.Project
            //{
            //    Id = Guid.Parse("13A06172-71F1-44AD-97EF-BB473A7B84BD"),
            //    ProjectName = "Alignment"
            //};
            //ProjectManager.CurrentUser = new User
            //{
            //    Id = Guid.Parse("75413790-4A32-482B-9A11-36BFBBC0AF9C"),
            //    FirstName = "Test",
            //    LastName = "User"
            //};
            //await ProjectManager.CreateNewProject("Alignment");
        }

        public async Task RetrieveTokensViaCorpusClass()
        {
            //try
            //{
            //    var corpus = await TokenizedTextCorpus.Get(_mediator, new TokenizedCorpusId(Guid.Parse("1C641B25-DE5E-4F37-B0EE-3EE43AC79E10")));
            //    var book = corpus.Where(row => ((VerseRef)row.Ref).BookNum == 40);
            //    var chapter = book.Where(row => ((VerseRef)row.Ref).ChapterNum == 1);
            //    var verse = chapter.First(row => ((VerseRef)row.Ref).VerseNum == 1) as TokensTextRow;
            //    DatabaseVerseTokens = new VerseTokens("40", "1", verse.Tokens.Where(t => t.TokenId.BookNumber == 40), true);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}
        }
    }
}
