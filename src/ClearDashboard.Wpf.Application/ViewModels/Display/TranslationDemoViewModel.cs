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
using Autofac;
using ClearApplicationFoundation.ViewModels.Infrastructure;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features.Corpora;
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

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class TranslationDemoViewModel : DashboardApplicationScreen, IMainWindowViewModel
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
        public IEnumerable<TokenDisplay>? Verse1NoTranslations { get; set; }
        public IEnumerable<TokenDisplay>? Verse2 { get; set; }
        public IEnumerable<TokenDisplay>? Verse3 { get; set; }

        // ReSharper disable UnusedMember.Global
        public TranslationDemoViewModel()
        {
        }

        public TranslationDemoViewModel(INavigationService navigationService, ILogger<TranslationDemoViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
        }

        //public void LoadTokens()
        //{
        //    LoadFiles();
        //}

        #region Event Handlers

        public void TokenClicked(TokenEventArgs e)
        {
            Message = $"'{e.TokenDisplay.SurfaceText}' token ({e.TokenDisplay.Token.TokenId}) clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenDoubleClicked(TokenEventArgs e)
        {
            Message = $"'{e.SurfaceText}' double-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenRightButtonDown(TokenEventArgs e)
        {
            Message = $"'{e.SurfaceText}' right-clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseEnter(TokenEventArgs e)
        {
            Message = $"Entered '{e.SurfaceText}'";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseLeave(TokenEventArgs e)
        {
            Message = string.Empty;
            NotifyOfPropertyChange(nameof(Message));
        }

        public void TokenMouseWheel(TokenEventArgs e)
        {
            Message = $"'{e.SurfaceText}' mouse wheel";
            NotifyOfPropertyChange(nameof(Message));
        }
        // ReSharper restore UnusedMember.Global

        #endregion


        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);
            await LoadFiles();
            //await MockProjectAndUser();
            //await RetrieveTokensViaCorpusClass();
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
        private Translation GetTranslation(Token token)
        {
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? MockOogaWords[mockOogaWordsIndexer_++]
                : String.Empty;
            if (mockOogaWordsIndexer_ == MockOogaWords.Count) mockOogaWordsIndexer_ = 0;
            var translation = new Translation(SourceToken: token, TargetTranslationText: translationText, TranslationState: RandomTranslationOriginatedFrom());
            
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

        private async Task LoadFiles()
        {
            var corpus = GetSampleEnglishTextCorpus().Cast<TokensTextRow>();

            Verse1 = GetTokenDisplays(corpus, 40001001);
            Verse1NoTranslations = GetTokenDisplays(corpus, 40001001);
            Verse2 = GetTokenDisplays(corpus, 40001002);
            Verse3 = GetTokenDisplays(corpus, 40001003);
            
            // Show some without translations
            foreach (var tokenDisplay in Verse1NoTranslations)
            {
                tokenDisplay.Translation = null;
            }            
            foreach (var tokenDisplay in Verse3)
            {
                tokenDisplay.Translation = null;
            }

            Verse2.First().Note = "This is a note";
            Verse2.Skip(3).First().Note = "Here's another note.";
            
            NotifyOfPropertyChange(nameof(Verse1));
            NotifyOfPropertyChange(nameof(Verse2));
            NotifyOfPropertyChange(nameof(Verse3));
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
