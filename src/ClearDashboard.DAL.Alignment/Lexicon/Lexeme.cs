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
        private ObservableCollection<Sense> senses_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Sense> senses_;
        private ObservableCollection<Sense> senses_;
#endif

        public ObservableCollection<Sense> Senses
        {
            get { return senses_; }
#if DEBUG
            set { senses_ = value; }
#else
            // RELEASE MODIFIED
            //set { senses_ = value; }
            set { senses_ = value; }
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
            senses_ = new ObservableCollection<Sense>();
            forms_ = new ObservableCollection<Form>();
        }
        internal Lexeme(LexemeId lexemeId, string lemma, string? language, string? type, ICollection<Sense> senses, ICollection<Form> forms)
        {
            LexemeId = lexemeId;
            Lemma = lemma;
            Language = language;
            Type = type;
            senses_ = new ObservableCollection<Sense>(senses.DistinctBy(s => s.SenseId)); ;
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

        public async Task PutSense(IMediator mediator, Sense sense, CancellationToken token = default)
        {
            if (sense.SenseId is not null && 
                senses_.Any(s => s.SenseId == sense.SenseId))
            {
                return;
            }

            if (LexemeId is null)
            {
                throw new MediatorErrorEngineException("Create Lexeme before associating with given Sense");
            }

            var result = await mediator.Send(new PutSenseCommand(LexemeId, sense), token);
            result.ThrowIfCanceledOrFailed();

            sense.SenseId = result.Data!;

            senses_.Add(sense);
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

        public static async Task<Lexeme?> Get(
            IMediator mediator,
            string lemma,
            string? language,
            string? senseLanguage,
            CancellationToken token = default)
        {
            var command = new GetLexemeByTextQuery(lemma, language, senseLanguage);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }
    }
}
