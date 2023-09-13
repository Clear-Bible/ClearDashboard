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
