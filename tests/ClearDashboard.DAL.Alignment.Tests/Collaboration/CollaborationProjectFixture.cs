using SIL.Machine.Utils;
using SIL.Scripture;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using ClearDashboard.Collaboration.Builder;
using ClearDashboard.Collaboration.Features;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.Collaboration.Services;
using ClearDashboard.Collaboration;
using Models = ClearDashboard.DataAccessLayer.Models;
using System.Text.RegularExpressions;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DataAccessLayer;
using System.Xml.Linq;
using ClearBible.Engine.Corpora;

namespace ClearDashboard.DAL.Alignment.Tests.Collaboration
{
    public class CollaborationProjectFixture : TestBase, IDisposable
    {
        public const string ShaBase = "shasha";
        private bool disposedValue;

        public List<Models.User> Users { get; private set; } = new();
        public List<Models.Corpus> Corpora { get; private set; } = new();
        public List<Models.TokenizedCorpus> TokenizedCorpora { get; private set; } = new();
        public List<Models.ParallelCorpus> ParallelCorpora { get; private set; } = new();
        public List<Models.TokenComposite> TokenComposites { get; private set; } = new();
        public List<Models.AlignmentSet> AlignmentSets { get; private set; } = new();
        public List<Models.Alignment> Alignments { get; private set; } = new();
        public List<Models.TranslationSet> TranslationSets { get; private set; } = new();
        public List<Models.Translation> Translations { get; private set; } = new();
        public List<Models.Note> Notes { get; private set; } = new();
        public Dictionary<Guid, IEnumerable<Models.Note>> RepliesByThread { get; private set; } = new();
        public List<(Models.NoteDomainEntityAssociation, NoteModelRef)> NoteAssociations { get; private set; } = new();
        public List<Models.Label> Labels { get; private set; } = new();
        public List<Models.LabelNoteAssociation> LabelNoteAssociations { get; private set; } = new();

        public ProjectSnapshot? ProjectSnapshotLastMerged { get; private set; } = null;

        public CollaborationProjectFixture(IMessageSink diagnosticMessageSink) : base(new TestOutputMessageSinkAdapter(diagnosticMessageSink))
        {
            BuildInitialEntities();
        }

        public void BuildInitialEntities()
        {
            var testUserId1 = Guid.NewGuid();
            var testUserId2 = Guid.NewGuid();
            Users.Clear();
            Users.Add(new Models.User() { Id = testUserId1, FirstName = "tester", LastName = "one"});
            Users.Add(new Models.User() { Id = testUserId2, FirstName = "tester", LastName = "two"});

            var testCorpus1 = BuildTestCorpus(Guid.NewGuid(), "test corpus 1", "language one", Models.CorpusType.Standard, testUserId1);
            Corpora.Clear();
            Corpora.Add(testCorpus1);

            var testTokenizedCorpus1 = BuildTestTokenizedCorpus(Guid.NewGuid(), testCorpus1, "test tokenized corpus one", "LatinWordTokenizer", ScrVers.English, testUserId1);

            // This should result in 23 TokenComponents.  If any of the VerseRow OriginalText data is changed below,
            // the final Assert in the constructor will need to be changed accordingly
            testTokenizedCorpus1.VerseRows = new List<Models.VerseRow>
            {
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpus1.Id, "001001001", "yes, the worms did eat my food because they were thirsty", testUserId1),
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpus1.Id, "001001002", "my hovercraft is full of eels", testUserId1),
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpus1.Id, "001001003", "boo!", testUserId1),
                BuildTestVerseRow(Guid.NewGuid(), testTokenizedCorpus1.Id, "002001001", "chapter two stuff", testUserId1)
            };
            TokenizedCorpora.Clear();
            TokenizedCorpora.Add(testTokenizedCorpus1);

            TokenComposites.Clear();
            TokenComposites.AddRange(BuildTestTokenComposites(testTokenizedCorpus1, null, new List<string> 
            { 
                "001001001002001-001001001003001",
                "001001001005001-001001001006001-001001001008001"
            }));
        }

        public ProjectSnapshot ToProjectSnapshot()
        {
            var testProject = ProjectDbContext.Projects.First();
            var builderContext = new BuilderContext(ProjectDbContext);

            var projectSnapshot = new ProjectSnapshot(ProjectBuilder.BuildModelSnapshot(testProject));
            projectSnapshot.AddGeneralModelList(ToUserBuilder(Users).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToCorpusBuilder(Corpora).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToTokenizedCorpusBuilder(TokenizedCorpora, TokenComposites).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToParallelCorpusBuilder(ParallelCorpora, TokenComposites).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToAlignmentSetBuilder(AlignmentSets, Alignments).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToTranslationSetBuilder(TranslationSets, Translations).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToNoteBuilder(Notes, RepliesByThread, NoteAssociations).BuildModelSnapshots(builderContext));
            projectSnapshot.AddGeneralModelList(ToLabelBuilder(Labels, LabelNoteAssociations).BuildModelSnapshots(builderContext));

            return projectSnapshot;
        }

        public async Task MergeIntoDatabase(string commitShaToMerge, ProjectSnapshot snapshotLastMerged, ProjectSnapshot snapshotToMerge, IProgress<ProgressStatus> progress)
        {
            var command = new MergeProjectSnapshotCommand(
                commitShaToMerge,
                snapshotLastMerged,
                snapshotToMerge,
                MergeMode.RemoteOverridesLocal,
                false,
                progress);
            var result = await Mediator.Send(command, CancellationToken.None);
            result.ThrowIfCanceledOrFailed();

            Assert.NotNull(result);
            Assert.True(result.Success);

            ProjectDbContext.ChangeTracker.Clear();
            Assert.Equal(commitShaToMerge, ProjectDbContext.Projects.First().LastMergedCommitSha);

            ProjectSnapshotLastMerged = snapshotToMerge;
        }

        public async Task<ProjectSnapshot> GetDatabaseProjectSnapshot()
        {
            var command = new GetProjectSnapshotQuery();
            var result = await Mediator.Send(command, CancellationToken.None);
            result.ThrowIfCanceledOrFailed();

            Assert.NotNull(result);
            Assert.True(result.Success);

            return result.Data!;
        }

        public void Report(ProgressStatus status)
        {
            var message = Regex.Replace(status.Message ?? string.Empty, "{PercentCompleted(:.*)?}", "{0$1}");
            var description = Regex.IsMatch(message, "{0(:.*)?}") ?
                string.Format(message, status.PercentCompleted) :
                message;

            Output.WriteLine(description);
        }

        public static Models.Corpus BuildTestCorpus(Guid id, string name, string language, Models.CorpusType corpusType, Guid userId)
        {
            var corpus = new Models.Corpus()
            {
                Id = id,
                IsRtl = true,
                FontFamily = "test font family",
                Name = name,
                DisplayName = name,
                Language = language,
                ParatextGuid = "2d2be644c2f6107a5b911a5df8c63dc69fa4ef6f",  // zz_SUR
                CorpusType = corpusType,
                Created = DateTimeOffset.UtcNow,
                UserId = userId
            };

            return corpus;
        }

        public static (Models.Corpus, Models.TokenizedCorpus) BuildTestManuscriptHebrew(Guid userId)
        {
            var corpus = new Models.Corpus()
            {
                Id = Corpus.FixedCorpusIdsByCorpusType[Models.CorpusType.ManuscriptHebrew],
                IsRtl = true,
                FontFamily = FontNames.HebrewFontFamily,
                Name = "Macula Hebrew",
                Language = "Hebrew",
                ParatextGuid = ManuscriptIds.HebrewManuscriptId,
                CorpusType = Models.CorpusType.ManuscriptHebrew,
                Created = DateTimeOffset.UtcNow,
                UserId = userId
            };

            var tokenizedCorpus = new Models.TokenizedCorpus
            {
                Id = TokenizedTextCorpus.FixedTokenizedCorpusIdsByCorpusType[Models.CorpusType.ManuscriptHebrew],
                CorpusId = corpus.Id,
                Corpus = corpus,
                DisplayName = "Macula Hebrew",
                TokenizationFunction = "WhitespaceTokenizer",
                Metadata = new(),
                LastTokenized = DateTimeOffset.UtcNow,
                Created = DateTimeOffset.UtcNow,
                UserId = userId
            };

            SetVersification(tokenizedCorpus, ScrVers.Original);

            return (corpus, tokenizedCorpus);
        }

        public static (Models.Corpus, Models.TokenizedCorpus) BuildTestManuscriptGreek(Guid userId)
        {
            var corpus = new Models.Corpus
            {
                Id = Corpus.FixedCorpusIdsByCorpusType[Models.CorpusType.ManuscriptGreek],
                IsRtl = false,
                FontFamily = FontNames.GreekFontFamily,
                Name = "Macula Greek",
                Language = "Greek",
                ParatextGuid = ManuscriptIds.GreekManuscriptId,
                CorpusType = Models.CorpusType.ManuscriptGreek,
                Created = DateTimeOffset.UtcNow,
                UserId = userId
            };

            var tokenizedCorpus = new Models.TokenizedCorpus
            {
                Id = TokenizedTextCorpus.FixedTokenizedCorpusIdsByCorpusType[Models.CorpusType.ManuscriptGreek],
                CorpusId = corpus.Id,
                Corpus = corpus,
                DisplayName = "Macula Greek",
                TokenizationFunction = "WhitespaceTokenizer",
                Metadata = new(),
                LastTokenized = DateTimeOffset.UtcNow,
                Created = DateTimeOffset.UtcNow,
                UserId = userId
            };

            SetVersification(tokenizedCorpus, ScrVers.Original);

            return (corpus, tokenizedCorpus);
        }

        public static Models.TokenizedCorpus BuildTestTokenizedCorpus(Guid id, Models.Corpus corpus, string? displayName, string tokenizationFunction, ScrVers versification, Guid userId)
        {
            var tokenizedCorpus = new Models.TokenizedCorpus()
            {
                Id = id,
                CorpusId = corpus.Id,
                Corpus = corpus,
                DisplayName = displayName,
                TokenizationFunction = tokenizationFunction,
                Metadata = new(),
                LastTokenized = DateTimeOffset.UtcNow,
                Created = DateTimeOffset.UtcNow,
                UserId = userId
            };

            SetVersification(tokenizedCorpus, versification);

            return tokenizedCorpus;
        }

        public static void SetVersification(Models.TokenizedCorpus tokenizedCorpus, ScrVers versification)
        {
            if (versification.IsCustomized)
            {
                tokenizedCorpus.ScrVersType = (int)versification.BaseVersification.Type;
                using (var writer = new StringWriter())
                {
                    versification.Save(writer);
                    tokenizedCorpus.CustomVersData = writer.ToString();
                }
            }
            else
            {
                tokenizedCorpus.ScrVersType = (int)versification.Type;
                tokenizedCorpus.CustomVersData = null;
            }
        }

        public static ScrVers BuildCustomVersification(ScrVers baseVersification, int mapValue = 3)
        {
            string customVersificationAddition = $"&MAT 1:2 = MAT 1:1\nMAT 1:{mapValue} = MAT 1:2\nMAT 1:1 = MAT 1:{mapValue}\n";
            using (var reader = new StringReader(customVersificationAddition))
            {
                Versification.Table.Implementation.RemoveAllUnknownVersifications();
                ScrVers versification = Versification.Table.Implementation.Load(reader, "not a file", baseVersification, "custom");

                return versification;
            }
        }

        public static Models.VerseRow BuildTestVerseRow(Guid id, Guid tokenizedCorpusId, string bookChapterVerse, string originalText, Guid userId)
        {
            return new Models.VerseRow
            {
                Id = id,
                BookChapterVerse = bookChapterVerse,
                OriginalText = originalText,
                IsSentenceStart = true,
                IsInRange = false,
                IsRangeStart = false,
                IsEmpty = false,
                TokenizedCorpusId = tokenizedCorpusId,
                Created = DateTimeOffset.UtcNow,
                Modified = null,
                UserId = userId
            };
        }

        public static IEnumerable<Models.TokenComposite> BuildTestTokenComposites(Models.TokenizedCorpus tokenizedCorpus, Models.ParallelCorpus? parallelCorpus, IEnumerable<string> engineTokenIds)
        {
            var tokenComposites = new List<Models.TokenComposite>();

            foreach (var engineTokenId in engineTokenIds.Where(e => e.Contains('-')))
            {
                var tokenComposite = new Models.TokenComposite
                {
                    Id = Guid.NewGuid(),
                    TokenizedCorpus = tokenizedCorpus,
                    TokenizedCorpusId = tokenizedCorpus.Id,
                    ParallelCorpus = parallelCorpus,
                    ParallelCorpusId = parallelCorpus?.Id,
                    EngineTokenId = engineTokenId
                };

                var tokenLocations = engineTokenId.Split('-');
                tokenComposite.TokenCompositeTokenAssociations = tokenLocations.Select(e => BuildTestTokenCompositeTokenAssociation(tokenComposite, e)).ToList();
                tokenComposite.Tokens = tokenComposite.TokenCompositeTokenAssociations.Select(e => e.Token!).ToList();

                tokenComposites.Add(tokenComposite);
            }

            return tokenComposites;
        }

        protected static Models.TokenCompositeTokenAssociation BuildTestTokenCompositeTokenAssociation(Models.TokenComposite tokenComposite, string engineTokenId)
        {
            var token = BuildTestToken(tokenComposite.TokenizedCorpus!, engineTokenId);
            var association =  new Models.TokenCompositeTokenAssociation
            {
                Id = Guid.NewGuid(),
                Token = token,
                TokenId = token.Id,
                TokenComposite = tokenComposite,
                TokenCompositeId = tokenComposite.Id
            };

            token.TokenCompositeTokenAssociations.Add(association);
            token.TokenComposites.Add(tokenComposite);

            return association;
        }

        public static Models.Token BuildTestToken(Models.TokenizedCorpus tokenizedCorpus, string engineTokenId)
        {
            return new Models.Token
            {
                Id = Guid.NewGuid(),
                TokenizedCorpus = tokenizedCorpus,
                TokenizedCorpusId = tokenizedCorpus.Id,
                EngineTokenId = engineTokenId,
                BookNumber = int.Parse(engineTokenId.Substring(0, 3)),
                ChapterNumber = int.Parse(engineTokenId.Substring(3, 3)),
                VerseNumber = int.Parse(engineTokenId.Substring(6, 3)),
                WordNumber = int.Parse(engineTokenId.Substring(9, 3)),
                SubwordNumber = int.Parse(engineTokenId.Substring(12, 3))
            };
        }

        public static Models.ParallelCorpus BuildTestParallelCorpus(Models.TokenizedCorpus testTokenizedCorpus1, Models.TokenizedCorpus testTokenizedCorpus2, Guid testUserId)
        {
            return new Models.ParallelCorpus
            {
                Id = Guid.NewGuid(),
                DisplayName = testTokenizedCorpus1.DisplayName + " - " + testTokenizedCorpus2.DisplayName,
                SourceTokenizedCorpus = testTokenizedCorpus1,
                TargetTokenizedCorpus = testTokenizedCorpus2,
                SourceTokenizedCorpusId = testTokenizedCorpus1.Id,
                TargetTokenizedCorpusId = testTokenizedCorpus2.Id,
                Created = DateTimeOffset.UtcNow,
                UserId = testUserId
            };
        }

        public static Models.AlignmentSet BuildTestAlignmentSet(Models.ParallelCorpus testParallelCorpus, Guid testUserId)
        {
            return new Models.AlignmentSet
            {
                Id = Guid.NewGuid(),
                ParallelCorpus = testParallelCorpus,
                ParallelCorpusId = testParallelCorpus.Id,
                DisplayName = "alignment set for " + testParallelCorpus.DisplayName,
                SmtModel = "FastAlign",
                IsSyntaxTreeAlignerRefined = false,
                IsSymmetrized = true,
                Created = DateTimeOffset.UtcNow,
                UserId = testUserId
            };
        }

        public static Models.TranslationSet BuildTestTranslationSet(Models.AlignmentSet testAlignmentSet, Guid testUserId)
        {
            return new Models.TranslationSet
            {
                Id = Guid.NewGuid(),
                ParallelCorpus = testAlignmentSet.ParallelCorpus,
                ParallelCorpusId = testAlignmentSet.ParallelCorpus!.Id,
                DisplayName = "translation set for " + testAlignmentSet.ParallelCorpus!.DisplayName,
                AlignmentSet = testAlignmentSet,
                AlignmentSetId = testAlignmentSet.Id,
                Created = DateTimeOffset.UtcNow,
                UserId = testUserId
            };
        }

        public static IEnumerable<Models.Alignment> BuildTestAlignments(
            Models.AlignmentSet alignmentSet, 
            Models.AlignmentVerification verification, 
            Models.AlignmentOriginatedFrom originatedFrom, 
            double score,
            Guid userId,
            IEnumerable<(Models.TokenComponent sourceTokenComponent, Models.TokenComponent targetTokenComponent)> tokenComponentPairs)
        {
            var alignments = new List<Models.Alignment>();

            foreach (var (sourceTokenComponent, targetTokenComponent) in tokenComponentPairs)
            {
                var alignment = new Models.Alignment()
                {
                    Id = Guid.NewGuid(),
                    AlignmentSet = alignmentSet,
                    AlignmentSetId = alignmentSet.Id,
                    SourceTokenComponent = sourceTokenComponent,
                    SourceTokenComponentId = sourceTokenComponent.Id,
                    TargetTokenComponent = targetTokenComponent,
                    TargetTokenComponentId = targetTokenComponent.Id,
                    AlignmentVerification = verification,
                    AlignmentOriginatedFrom = originatedFrom,
                    Score = score,
                    Created = DateTimeOffset.UtcNow,
                    UserId = userId
                };

                alignments.Add(alignment);
            }

            return alignments;
        }

        public static IEnumerable<Models.Translation> BuildTestTranslations(
            Models.TranslationSet translationSet,
            Models.TranslationOriginatedFrom originatedFrom,
            Guid userId,
            IEnumerable<(Models.TokenComponent sourceTokenComponent, string targetText)> tokenComponentText)
        {
            var translations = new List<Models.Translation>();

            foreach (var (sourceTokenComponent, targetText) in tokenComponentText)
            {
                var translation = new Models.Translation()
                {
                    Id = Guid.NewGuid(),
                    TranslationSet = translationSet,
                    TranslationSetId = translationSet.Id,
                    SourceTokenComponent = sourceTokenComponent,
                    SourceTokenComponentId = sourceTokenComponent.Id,
                    TargetText = targetText,
                    TranslationState = originatedFrom,
                    Created = DateTimeOffset.UtcNow,
                    UserId = userId
                };

                translations.Add(translation);
            }

            return translations;
        }


        public static Models.Note BuildTestNote(Guid id, string text, Models.NoteStatus noteStatus, Guid userId)
        {
            return new Models.Note
            {
                Id = id,
                Text = text,
                AbbreviatedText = null,
                ThreadId = null,
                NoteStatus = noteStatus,
                UserId = userId
            };
        }

        public static (Models.NoteDomainEntityAssociation, NoteModelRef) BuildTestNoteTokenAssociation(Guid noteId, Guid tokenizedCorpusId, string tokenLocation, BuilderContext builderContext)
        {
            var tokenIdType = (tokenLocation.Contains('-')) ? "CompositeTokenId" : "TokenId";

            var nd = new Models.NoteDomainEntityAssociation
            {
                Id = Guid.NewGuid(),
                NoteId = noteId,
                DomainEntityIdGuid = Guid.NewGuid(),
                DomainEntityIdName = $"ClearBible.Engine.Utils.EntityId`1[[ClearBible.Engine.Corpora.{tokenIdType}, ClearBible.Engine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], ClearBible.Engine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
            };

            var noteModelRef = new NoteModelRef<TokenRef>
            (
                nd.Id,
                nd.NoteId,
                TokenBuilder.BuildTokenRef(new Models.Token { TokenizedCorpusId = tokenizedCorpusId, EngineTokenId = tokenLocation }, builderContext)
            );

            if (tokenIdType == "CompositeTokenId")
            {
                ((TokenRef)noteModelRef.ModelRef).IsComposite = true;
            }

            return (nd, noteModelRef);
        }

        public static UserBuilder ToUserBuilder(IEnumerable<Models.User> users)
        {
            return new UserBuilder { GetUsers = (projectDbContext) => users };
        }

        public static CorpusBuilder ToCorpusBuilder(IEnumerable<Models.Corpus> corpora)
        {
            return new CorpusBuilder { GetCorpora = (projectDbContext) => corpora };
        }

        public static TokenizedCorpusBuilder ToTokenizedCorpusBuilder(
            IEnumerable<Models.TokenizedCorpus> tokenizedCorpora,
            IEnumerable<Models.TokenComposite> tokenComposites)
        {
            var tokenizedCorpusBuilder = new TokenizedCorpusBuilder
            {
                GetTokenizedCorpora = (projectDbContext) =>
                {
                    return tokenizedCorpora;
                },
                GetBookNumbers = (projectDbContext, tokenizedCorpusId) =>
                {
                    var tokenizedCorpus = tokenizedCorpora.Where(e => e.Id == tokenizedCorpusId).FirstOrDefault();
                    return (tokenizedCorpus != null)
                        ? tokenizedCorpus.VerseRows
                            .Select(e => e.BookChapterVerse!.Substring(0, 3))
                            .Distinct()
                            .OrderBy(e => e)
                            .ToGeneralListModel()
                        : new GeneralListModel<string>();
                },
                VerseRowBuilder = new VerseRowBuilder
                {
                    GetVerseRows = (projectDbContext, tokenizedCorpusId) =>
                    {
                        var tokenizedCorpus = tokenizedCorpora.Where(e => e.Id == tokenizedCorpusId).FirstOrDefault();
                        return (tokenizedCorpus != null)
                            ? tokenizedCorpus.VerseRows
                            : Enumerable.Empty<Models.VerseRow>();
                    }
                },
                TokenBuilder = new TokenBuilder()
                {
                    GetTokenizedCorpusCompositeTokens = (projectDbContext, tokenizedCorpusId) =>
                    {
                        return tokenComposites
                            .Where(e => e.TokenizedCorpusId == tokenizedCorpusId)
                            .Where(e => e.ParallelCorpusId == null)
                            .Select(e => (e, e.Tokens.AsEnumerable()));
                    },
                    GetParallelCorpusCompositeTokens = (projectDbContext, parallelCorpusId) =>
                    {
                        return Enumerable.Empty<(Models.TokenComposite, IEnumerable<Models.Token>)>();
                    }
                }
            };

            return tokenizedCorpusBuilder;
        }

        public static ParallelCorpusBuilder ToParallelCorpusBuilder(
            IEnumerable<Models.ParallelCorpus> parallelCorpora,
            IEnumerable<Models.TokenComposite> tokenComposites)
        {
            var parallelCorpusBuilder = new ParallelCorpusBuilder
            {
                GetParallelCorpora = (projectDbContext) =>
                {
                    return parallelCorpora;
                },
                TokenBuilder = new TokenBuilder()
                {
                    GetTokenizedCorpusCompositeTokens = (projectDbContext, tokenizedCorpusId) =>
                    {
                        return Enumerable.Empty<(Models.TokenComposite, IEnumerable<Models.Token>)>();
                    },
                    GetParallelCorpusCompositeTokens = (projectDbContext, parallelCorpusId) =>
                    {
                        return tokenComposites
                            .Where(e => e.ParallelCorpusId == parallelCorpusId)
                            .Select(e => (e, e.Tokens.AsEnumerable()));
                    }
                }
            };

            return parallelCorpusBuilder;
        }

        public static AlignmentSetBuilder ToAlignmentSetBuilder(
            IEnumerable<Models.AlignmentSet> alignmentSets,
            IEnumerable<Models.Alignment> alignments)
        {
            var alignmentSetBuilder = new AlignmentSetBuilder
            {
                GetAlignmentSets = (projectDbContext) =>
                {
                    return alignmentSets;
                },
                AlignmentBuilder = new AlignmentBuilder()
                {
                    GetAlignments = (projectDbContext, alignmentSetId) =>
                    {
                        return alignments
                            .Where(e => e.AlignmentSetId == alignmentSetId)
                            .Select(e => (
                                alignment: e,
                                leadingToken: e.SourceTokenComponent as Models.Token ??
                                             (e.SourceTokenComponent as Models.TokenComposite)!.Tokens.First()
                            ));
                    }
                }
            };

            return alignmentSetBuilder;
        }
        public static TranslationSetBuilder ToTranslationSetBuilder(
            IEnumerable<Models.TranslationSet> translationSets,
            IEnumerable<Models.Translation> translations)
        {
            var translationSetBuilder = new TranslationSetBuilder
            {
                GetTranslationSets = (projectDbContext) =>
                {
                    return translationSets;
                },
                TranslationBuilder = new TranslationBuilder()
                {
                    GetTranslations = (projectDbContext, translationSetId) =>
                    {
                        return translations
                            .Where(e => e.TranslationSetId == translationSetId)
                            .Select(e => (
                                translation: e,
                                leadingToken: e.SourceTokenComponent as Models.Token ??
                                             (e.SourceTokenComponent as Models.TokenComposite)!.Tokens.First()
                            ));
                    }
                }
            };

            return translationSetBuilder;
        }

        public static NoteBuilder ToNoteBuilder(
            IEnumerable<Models.Note> notes,
            Dictionary<Guid, IEnumerable<Models.Note>> repliesByThread,
            IEnumerable<(Models.NoteDomainEntityAssociation nd, NoteModelRef noteModelRef)> noteAssociations)
        {
            var noteBuilder = new NoteBuilder
            {
                GetNotes = (projectDbContext) => notes,
                GetRepliesByThreadId = (projectDbContext) => repliesByThread,
                GetNoteDomainEntityAssociationsByNoteId = (projectDbContext) =>
                {
                    return noteAssociations
                        .Select(e => e.nd)
                        .GroupBy(e => e.NoteId)
                        .Select(g => new
                        {
                            NoteId = g.Key,
                            DomainEntityTypes = g.ToList()
                                .GroupBy(gg => gg.DomainEntityIdName!)
                                .ToDictionary(gg => gg.Key, gg => gg.Select(e => e))
                        })
                        .ToDictionary(g => g.NoteId, g => g.DomainEntityTypes);
                },
                ExtractNoteModelRefs = (nda, builderContext) =>
                {
                    var ndaIds = nda.Values.SelectMany(nds => nds.Select(nd => nd.Id));
                    return new GeneralListModel<NoteModelRef>(noteAssociations.Where(e => ndaIds.Contains(e.nd.Id)).Select(e => e.noteModelRef));
                }
            };

            return noteBuilder;
        }

        public static LabelBuilder ToLabelBuilder(
            IEnumerable<Models.Label> labels,
            IEnumerable<Models.LabelNoteAssociation> labelNoteAssociations)
        {
            return new LabelBuilder
            {
                GetLabels = (projectDbContext) => labels,
                GetLabelNoteAssociationsByLabelId = (projectDbContext) =>
                {
                    return labelNoteAssociations
                        .GroupBy(e => e.LabelId)
                        .ToDictionary(g => g.Key, g => g.Select(e => e));
                }
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DeleteDatabaseContext().Wait();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
