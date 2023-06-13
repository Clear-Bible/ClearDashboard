using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Lexeme
    {
        public LexemeId? LexemeId
        {
            get;
#if DEBUG
            set;
#else 
            // RELEASE MODIFIED
            //private set;
            set;
#endif
        }
        public string? Lemma { get; set; }
        public string? Language { get; set; }
        public string? Type { get; set; }

#if DEBUG
        private ObservableCollection<Meaning> meanings_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Meaning> meanings_;
        private ObservableCollection<Meaning> meanings_;
#endif

        public ObservableCollection<Meaning> Meanings
        {
            get { return meanings_; }
#if DEBUG
            set { meanings_ = value; }
#else
            // RELEASE MODIFIED
            //set { meanings_ = value; }
            set { meanings_ = value; }
#endif
        }

#if DEBUG
        private ObservableCollection<Form> forms_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Form> forms_;
        private ObservableCollection<Form> forms_;
#endif

        public ObservableCollection<Form> Forms
        {
            get { return forms_; }
#if DEBUG
            set { forms_ = value; }
#else
            // RELEASE MODIFIED
            //set { forms_ = value; }
            set { forms_ = value; }
#endif
        }

        public Lexeme()
        {
            meanings_ = new ObservableCollection<Meaning>();
            forms_ = new ObservableCollection<Form>();
        }
        internal Lexeme(LexemeId lexemeId, string lemma, string? language, string? type, ICollection<Meaning> meanings, ICollection<Form> forms)
        {
            LexemeId = lexemeId;
            Lemma = lemma;
            Language = language;
            Type = type;
            meanings_ = new ObservableCollection<Meaning>(meanings.DistinctBy(s => s.MeaningId)); ;
            forms_ = new ObservableCollection<Form>(forms.DistinctBy(f => f.FormId)); ;
        }

        public async Task<Lexeme> Create(IMediator mediator, CancellationToken token = default)
        {
            var command = new CreateOrUpdateLexemeCommand(null, Lemma ?? string.Empty, Language, Type);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            LexemeId = result.Data!;
            return this;
        }

        public async Task PutMeaning(IMediator mediator, Meaning meaning, CancellationToken token = default)
        {
            if (LexemeId is null)
            {
                throw new MediatorErrorEngineException("Create Lexeme before associating with given Meaning");
            }

            var result = await mediator.Send(new PutMeaningCommand(LexemeId, meaning), token);
            result.ThrowIfCanceledOrFailed();

            if (meaning.MeaningId is not null &&
                meanings_.Any(s => s.MeaningId == meaning.MeaningId))
            {
                return;
            }

            meaning.MeaningId = result.Data!;
            meanings_.Add(meaning);
        }

        public async Task PutForm(IMediator mediator, Form form, CancellationToken token = default)
        {
            if (form.FormId is not null &&
                forms_.Any(f => f.FormId == form.FormId))
            {
                return;
            }

            if (LexemeId is null)
            {
                throw new MediatorErrorEngineException("Create Lexeme before associating with given Form");
            }

            var result = await mediator.Send(new PutFormCommand(LexemeId, form), token);
            result.ThrowIfCanceledOrFailed();

            form.FormId = result.Data!;

            forms_.Add(form);
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (LexemeId == null)
            {
                return;
            }

            var command = new DeleteLexemeAndDependentsCommand(LexemeId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        [Obsolete("This method is deprecated, use GetByLemmaOrForm instead.")]
        public static async Task<Lexeme?> Get(
            IMediator mediator,
            string lemma,
            string? language,
            string? meaningLanguage,
            CancellationToken token = default)
        {
            var command = new GetLexemeByTextQuery(lemma, language, meaningLanguage);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }

        public static async Task<IEnumerable<Lexeme>> GetByLemmaOrForm(
            IMediator mediator,
            string lemmaOrForm,
            string? language,
            string? meaningLanguage,
            CancellationToken token = default)
        {
            var command = new GetLexemesByLemmaOrFormQuery(lemmaOrForm, language, meaningLanguage);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }
    }
}
