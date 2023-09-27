using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClearBible.Engine.Corpora;
using ClearBible.Engine.Tokenization;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Notes;
using ClearDashboard.DAL.Alignment.Translation;
using ClearDashboard.Wpf.Application.Collections.Notes;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using SIL.Scripture;

namespace ClearDashboard.Wpf.Application.ViewModels.EnhancedView
{
    public class MockVerseDisplayViewModel : VerseDisplayViewModel
    {
        private static readonly string _testDataPath = Path.Combine(AppContext.BaseDirectory, "Data");
        private static readonly string _usfmTestProjectPath = Path.Combine(_testDataPath, "usfm", "Tes");
        private static IEnumerable<TokensTextRow>? _mockCorpus;

        private static IEnumerable<TokensTextRow> MockCorpus => _mockCorpus ??=
            new UsfmFileTextCorpus("usfm.sty", Encoding.UTF8, _usfmTestProjectPath)
                .Tokenize<LatinWordTokenizer>()
                .Transform<IntoTokensTextRowProcessor>()
                .Transform<SetTrainingBySurfaceLowercase>()
                .Cast<TokensTextRow>();

        // ReSharper disable once InconsistentNaming
        private static TokensTextRow? GetMockVerseTextRow(int BBBCCCVVV)
        {
            return MockCorpus.FirstOrDefault(row => ((VerseRef)row.Ref).BBBCCCVVV == BBBCCCVVV);
        }

        // ReSharper disable StringLiteralTypo
        // ReSharper disable once IdentifierTypo
        private static readonly List<string> _mockOogaWords = new()
            { "Ooga", "booga", "bong", "biddle", "foo", "boi", "foodie", "fingle", "boing", "la" };
        // ReSharper restore StringLiteralTypo

        private static int _mockTranslationWordIndexer;

        private static string GetMockTranslationWord()
        {
            var result = _mockOogaWords[_mockTranslationWordIndexer++];
            if (_mockTranslationWordIndexer == _mockOogaWords.Count) _mockTranslationWordIndexer = 0;
            return result;
        }

        private static string GetMockTranslationStatus()
        {
            return new Random().Next(3) switch
            {
                0 => Translation.OriginatedFromValues.FromTranslationModel,
                1 => Translation.OriginatedFromValues.FromOther,
                _ => Translation.OriginatedFromValues.Assigned
            };
        }

        private static IEnumerable<TranslationOption> GetMockTranslationOptions(string sourceTranslation)
        {
            var result = new List<TranslationOption>();

            var random = new Random();
            var optionCount = random.Next(4) + 2; // 2-5 options
            var remainingPercentage = 100d;

            var basePercentage = random.NextDouble() * remainingPercentage;
            result.Add(new TranslationOption { Word = sourceTranslation, Count = basePercentage });
            remainingPercentage -= basePercentage;

            for (var i = 1; i < optionCount - 1; i++)
            {
                var percentage = random.NextDouble() * remainingPercentage;
                result.Add(new TranslationOption { Word = GetMockTranslationWord(), Count = percentage });
                remainingPercentage -= percentage;
            }

            result.Add(new TranslationOption { Word = GetMockTranslationWord(), Count = remainingPercentage });

            return result.OrderByDescending(to => to.Count);
        }

        private static NoteId GetMockNoteId()
        {
            return new NoteId(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow,
                new UserId(Guid.NewGuid(), "Joe Schmoe"));

        }

        private static ObservableCollection<Label> GetMockLabels()
        {
            var mockLabelSuggestions = MockLabelSuggestions.ToList();
            var mockLabels = new List<string>();
            var numMockLabels = new Random().Next(4);

            for (var i = 0; i <= numMockLabels; i++)
            {
                var mockLabelIndex = new Random().Next(mockLabelSuggestions.Count);
                var mockLabel = mockLabelSuggestions[mockLabelIndex].Text;
                if (mockLabel != null && !mockLabels.Contains(mockLabel))
                {
                    mockLabels.Add(mockLabel);
                }
            }

            return new ObservableCollection<Label>(mockLabels.OrderBy(lt => lt)
                .Select(lt => new Label { Text = lt }));
        }

        private static NoteCollection GetMockNotes()
        {
            var result = new NoteCollection();
            var random = new Random().NextDouble();
            if (random < 0.2)
            {
                result.Add(new Note
                {
                    Text = "This is a note",
                    NoteId = GetMockNoteId(),
                    Labels = GetMockLabels()
                });
            }

            if (random < 0.1)
            {
                result.Add(new Note
                {
                    Text = "Here's another note",
                    NoteId = GetMockNoteId(),
                    Labels = GetMockLabels()
                });
            }

            return result;
        }

        // ReSharper disable StringLiteralTypo
        private static ObservableCollection<Label> MockLabelSuggestions => new()
        {
            new Label { Text = "alfa" },
            new Label { Text = "bravo" },
            new Label { Text = "charlie" },
            new Label { Text = "delta" },
            new Label { Text = "echo" }
        };
        // ReSharper restore StringLiteralTypo

        private static Translation? GetTranslationForToken(Token token)
        {
            var translationText = (token.SurfaceText != "." && token.SurfaceText != ",")
                ? GetMockTranslationWord()
                : String.Empty;
            return new Translation(sourceToken: token, targetTranslationText: translationText,
                originatedFrom: GetMockTranslationStatus());
        }

        private async Task<IEnumerable<Translation>> GetTranslations(TranslationSet translationSet,
            IEnumerable<TokenId> tokens)
        {
            return new List<Translation>();
        }

        private async Task<Dictionary<IId, IEnumerable<Note>>?> GetAllNotes()
        {
            return new Dictionary<IId, IEnumerable<Note>>();
        }

        private async Task<LabelCollection> GetLabelSuggestions()
        {
            return new LabelCollection(MockLabelSuggestions);
        }

        /// <summary>
        /// Gets a collection of <see cref="TranslationOption"/>s for a given translation.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> for which to provide options.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> containing a <see cref="IEnumerable{T}"/> of <see cref="TranslationOption"/>s.</returns>
        public async Task<IEnumerable<TranslationOption>> GetTranslationOptionsAsync(Translation translation)
        {
            return GetMockTranslationOptions(translation.TargetTranslationText);
        }

        /// <summary>
        /// Saves a selected translation for a token to the database.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> to save to the database.</param>
        /// <param name="translationActionType"></param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task PutTranslationAsync(Translation translation, string translationActionType)
        {
            return;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task BindMockVerseAsync(int BBBCCCVVV = 40001001)
        {
            var row = GetMockVerseTextRow(BBBCCCVVV);
            //await BindAsync(row);
        }

        public MockVerseDisplayViewModel() : base(null, null, null, null, null)
        {
            
        }
    }
}
