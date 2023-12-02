using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;
using System.Collections.ObjectModel;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Lexicon
    {
#if DEBUG
        private ObservableCollection<Lexeme> lexemes_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Lexeme> lexemes_;
        private ObservableCollection<Lexeme> lexemes_;
#endif

        private readonly List<LexemeId> lexemeIdsInDatabase_;
        public IEnumerable<LexemeId> LexemeIdsToDelete => lexemeIdsInDatabase_.ExceptBy(lexemes_.Select(e => e.LexemeId.Id), e => e.Id);

        public static async Task<Lexicon> MergeAndSaveAsync(IEnumerable<Lexeme> lexemes, Lexicon sourceLexicon, IMediator mediator)
        {
            //var sourceLexicon = new Lexicon(new ObservableCollection<Lexeme>(sourceLexemes));
            var lexemeCollection = new ObservableCollection<Lexeme>(lexemes.DistinctBy(e => e.LexemeId));
            var lexicon = new Lexicon(lexemeCollection);
            lexicon.SetLexemeIdsInDatabase(lexemeCollection.Select(e => e.LexemeId)
                            .IntersectBy(sourceLexicon.lexemeIdsInDatabase_.Select(e => e.Id),
                                                         e => e.Id).ToList());
            await lexicon.SaveAsync(mediator);
            return lexicon;
        }

        internal void SetLexemeIdsInDatabase(IEnumerable<LexemeId> lexemeIds)
        {
            lexemeIdsInDatabase_.Clear();
            lexemeIdsInDatabase_.AddRange(lexemeIds);
        }

        public ObservableCollection<Lexeme> Lexemes
        {
            get { return lexemes_; }
#if DEBUG
            set { lexemes_ = value; }
#else
            // RELEASE MODIFIED
            //set { lexemes_ = value; }
            set { lexemes_ = value; }
#endif
        }

        public Lexicon()
        {
            lexemes_ = new ObservableCollection<Lexeme>();
            lexemeIdsInDatabase_ = new();

        }
        internal Lexicon(ICollection<Lexeme> lexemes)
        {
            lexemes_ = new ObservableCollection<Lexeme>(lexemes.DistinctBy(e => e.LexemeId));
            lexemeIdsInDatabase_ = new(lexemes.Select(f => f.LexemeId));
        }
        internal Lexicon(IEnumerable<Lexeme> lexemes, List<LexemeId> lexemeIdsInDatabase)
        {
            lexemes_ = new ObservableCollection<Lexeme>(lexemes.DistinctBy(e => e.LexemeId));
            lexemeIdsInDatabase_ = lexemeIdsInDatabase;
        }

        public static Lexicon MergeFirstIntoSecond(Lexicon first, Lexicon second)
        {
            var combinedLexemes = first.Lexemes.MergeIntoSecond(second.Lexemes);

            // Find LexemeIdsInDatabase from first Lexicon that aren't already
            // in second Lexicon, but only ones for combined lexemes:
            var additionalLexemeIdsInDatabase = first.lexemeIdsInDatabase_
                .ExceptBy(second.lexemeIdsInDatabase_.Select(e => e.Id), e => e.Id)
                .IntersectBy(combinedLexemes.Select(e => e.LexemeId.Id), e => e.Id);

            var combinedLexemeIdsInDatabase = second.lexemeIdsInDatabase_
                .UnionBy(additionalLexemeIdsInDatabase, e => e.Id)
                .ToList();

            return new Lexicon(combinedLexemes, combinedLexemeIdsInDatabase);
        }

        public static async Task<Lexicon> MergeAndSaveAsync(Collection<Lexeme> lexemes, Lexicon sourceLexicon, IMediator mediator)
        {
            var lexicon = new Lexicon(lexemes, lexemes.Select(e => e.LexemeId).IntersectBy(
                sourceLexicon.lexemeIdsInDatabase_.Select(e => e.Id),
                e => e.Id).ToList());

            await lexicon.SaveAsync(mediator);
            return lexicon;
        }

        public async Task SaveAsync(IMediator mediator, CancellationToken token = default)
        {
            var result = await mediator.Send(new SaveLexiconCommand(this), token);
            result.ThrowIfCanceledOrFailed();

            var createdIIdsByGuid = result.Data!.ToDictionary(e => e.Id, e => e);

            lexemeIdsInDatabase_.Clear();
            lexemeIdsInDatabase_.AddRange(lexemes_.Select(e => e.LexemeId));

            foreach (var lexeme in lexemes_)
            {
                lexeme.PostSaveAll(createdIIdsByGuid);
            }
        }

        public static async Task<Lexicon> GetExternalLexicon(IMediator mediator, string? projectId, CancellationToken token = default)
        {
            var result = await mediator.Send(new GetExternalLexiconQuery(projectId), token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        public static async Task<Lexicon> GetInternalLexicon(IMediator mediator, CancellationToken token = default)
        {
            var result = await mediator.Send(new GetInternalLexiconQuery(), token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }
    }
}
