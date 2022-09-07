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
using ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.Wpf.Application.UserControls;
using ClearDashboard.Wpf.Application.ViewModels.Infrastructure;
using MediatR;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using CorpusClass = ClearDashboard.DAL.Alignment.Corpora.Corpus;
using EngineToken = ClearBible.Engine.Corpora.Token;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace ClearDashboard.Wpf.Application.ViewModels.Display
{
    public class AlignmentSampleViewModel : DashboardApplicationScreen, IMainWindowViewModel
    {
        private readonly IMediator _mediator;
        public List<string> EnglishWords { get; set; } = new() { "alfa", "bravo", "charlie", "delta", "echo", "foxtrot", "golf", "hotel", "india", "juliet", "kilo", "lima", "mike" };
        public List<string> GreekLowercase { get; set; } = "α β γ δ ε ζ η θ ι κ λ μ ν ξ ο π ρ σ τ υ φ χ ψ ω".Split(' ').ToList();
        public List<string> GreekUppercase { get; set; } = "Α Β Γ Δ Ε Ζ Η Θ Ι Κ Λ Μ Ν Ξ Ο Π Ρ Σ Τ Υ Φ Χ Ψ Ω".Split(' ').ToList();
        public List<string> HebrewPsalm { get; set; } = "כִּֽי־אַ֭תָּה תָּאִ֣יר נֵרִ֑י יְהוָ֥ה אֱ֝לֹהַ֗י יַגִּ֥יהַּ חָשְׁכִּֽי׃".Split(' ').ToList();
        public List<string> GreekPsalm { get; set; } = "χι αθθα θαειρ νηρι YHWH ελωαι αγι οσχι".Split(' ').ToList();

        public List<string> Paragraph { get; set; } =
            "In the beginning God created the heavens and the earth. Now the earth was formless and empty, darkness was over the surface of the deep, and the Spirit of God was hovering over the waters.".Split(' ').ToList();

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

        private async Task<CorpusClass> GetSampleCorpusAsync(ITextCorpus textCorpus, string language)
        {
            var corpus = await CorpusClass.Create(Mediator!, true, "NameX", "LanguageX", "LanguageType");
            var tokenizedTextCorpus = await textCorpus.Create(Mediator!, corpus.CorpusId, "boo", ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()");

            //var corpus = new Corpus
            //{
            //    IsRtl = false,
            //    Name = "Sample",
            //    Language = language,
            //    CorpusType = CorpusType.Standard,
            //    Metadata =
            //    {
            //        ["TokenizationQueryString"] = ".Tokenize<LatinWordTokenizer>().Transform<IntoTokensTextRowProcessor>()"
            //    }
            //};

            //var tokenizedCorpus = new TokenizedCorpus();
            //textCorpus.Cast<TokensTextRow>().ToList().ForEach(tokensTextRow =>
            //    tokenizedCorpus.Tokens.AddRange(tokensTextRow.Tokens.Select(engineToken => new Token
            //    {
            //        BookNumber = engineToken.TokenId.BookNumber,
            //        ChapterNumber = engineToken.TokenId.ChapterNumber,
            //        VerseNumber = engineToken.TokenId.VerseNumber,
            //        WordNumber = engineToken.TokenId.WordNumber,
            //        SubwordNumber = engineToken.TokenId.SubWordNumber,
            //        SurfaceText = engineToken.SurfaceText,
            //        TrainingText = engineToken.TrainingText
            //    }))
            //);
            //corpus.TokenizedCorpora.Add(tokenizedCorpus);
            return corpus;
        }

        //private static IEnumerable<string> GetBookAbbreviations(TokenizedCorpus tokenizedCorpus)
        //{
        //    var bookNumbers = tokenizedCorpus.Tokens.GroupBy(token => token.BookNumber).Select(g => g.Key);
        //    var bookIdsToAbbreviations = FileGetBookIds.BookIds.ToDictionary(x => int.Parse(x.silCannonBookNum), x => x.silCannonBookAbbrev);

        //    var bookAbbreviations = new List<string>();
        //    foreach (var bookNumber in bookNumbers)
        //    {
        //        if (bookIdsToAbbreviations.TryGetValue(bookNumber != null ? bookNumber : -1, out string bookAbbreviation))
        //        {
        //            bookAbbreviations.Add(bookAbbreviation);
        //        }
        //    }

        //    return bookAbbreviations;
        //}

        //private CorpusClass _englishCorpus;
        //private CorpusClass EnglishCorpus => _englishCorpus ??= awaitGetSampleCorpusAsync(GetSampleEnglishTextCorpus(), "English");
        //private TokenizedCorpus EnglishTokenizedCorpus => EnglishCorpus.TokenizedCorpora.First();
        //public List<string> EnglishFile { get; set; } = new();
        //private Corpus _greekCorpus;
        //private Corpus GreekCorpus => _greekCorpus ??= GetSampleCorpusAsync(GetSampleGreekTextCorpus(), "Greek");
        //private TokenizedCorpus GreekTokenizedCorpus => GreekCorpus.TokenizedCorpora.First();
        public List<string> GreekVerse1 { get; set; } = new();
        public string Message { get; set; }
        private VerseTokens DatabaseVerseTokens { get; set; }

        public List<string> DatabaseVerseTokensText => DatabaseVerseTokens != null ? DatabaseVerseTokens.Tokens.Select(t => t.SurfaceText).ToList() : new List<string>();

        public TokensTextRow TextRow { get; set; }
        public IEnumerable<(EngineToken token, string paddingBefore, string paddingAfter)> Tokens { get; set; }

        public string DatabaseVerseDetokenized
        {
            get
            {
                if (DatabaseVerseTokens != null)
                {
                    var detokenizer = new LatinWordDetokenizer();
                    return detokenizer.Detokenize(DatabaseVerseTokensText);
                }
                return string.Empty;
            }
        }
        public List<string> DatabaseVerseWords => DatabaseVerseDetokenized.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // ReSharper disable UnusedMember.Global
        public AlignmentSampleViewModel()
        {
        }

        public AlignmentSampleViewModel(INavigationService navigationService,
            ILogger<AlignmentSampleViewModel> logger, DashboardProjectManager projectManager, IEventAggregator eventAggregator,
            IMediator mediator, ILifetimeScope? lifetimeScope)
            : base(projectManager, navigationService, logger, eventAggregator, mediator, lifetimeScope)
        {
            _mediator = mediator;
            //LoadFiles();
        }

        public void TokenClicked(TokenEventArgs e)
        {
            Message = $"'{e.SurfaceText}' clicked";
            NotifyOfPropertyChange(nameof(Message));
        }

        public void LoadTokens()
        {
            LoadFiles();
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

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            //LoadFiles();
            //await MockProjectAndUser();
            //await RetrieveTokensViaQuery(cancellationToken);
            //await RetrieveTokensViaCorpusClass();
            await base.OnActivateAsync(cancellationToken);
        }

        private void LoadFiles()
        {
            var corpus = GetSampleEnglishTextCorpus();
            TextRow = corpus.Cast<TokensTextRow>().FirstOrDefault();
            NotifyOfPropertyChange(nameof(TextRow));

            if (TextRow != null)
            {
                var detokenizer = new EngineStringDetokenizer(new LatinWordDetokenizer());
                Tokens = detokenizer.Detokenize(TextRow.Tokens);
            }
            NotifyOfPropertyChange(nameof(Tokens));

            //EnglishFile = EnglishTokenizedCorpus.Tokens.Where(t => t.BookNumber == 40 && t.ChapterNumber == 1 && t.VerseNumber == 1).Select(t => t.SurfaceText).ToList();
            //NotifyOfPropertyChange(nameof(EnglishFile));

            //GreekVerse1 = GreekTokenizedCorpus.Tokens.Where(t => t.ChapterNumber == 1 && t.VerseNumber == 1).Select(t => t.SurfaceText).ToList();
            //NotifyOfPropertyChange(nameof(GreekVerse1));
        }

        private async Task MockProjectAndUser()
        {
            ProjectManager.CurrentProject = new DataAccessLayer.Models.Project
            {
                Id = Guid.Parse("13A06172-71F1-44AD-97EF-BB473A7B84BD"),
                ProjectName = "Alignment"
            };
            ProjectManager.CurrentUser = new User
            {
                Id = Guid.Parse("75413790-4A32-482B-9A11-36BFBBC0AF9C"),
                FirstName = "Test",
                LastName = "User"
            };
            await ProjectManager.CreateNewProject("Alignment");
        }

        public async Task RetrieveTokensViaQuery(CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetTokensByTokenizedCorpusIdAndBookIdQuery(new TokenizedTextCorpusId(Guid.Parse("1C641B25-DE5E-4F37-B0EE-3EE43AC79E10")), "MAT");
                var result = await ExecuteRequest(query, cancellationToken);

                if (result.Success && result.Data != null)
                {
                    DatabaseVerseTokens = result.Data.FirstOrDefault(v => v.Chapter == "1" && v.Verse == "1");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
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
