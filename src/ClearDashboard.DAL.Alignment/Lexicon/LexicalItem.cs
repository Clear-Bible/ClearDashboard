using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class LexicalItem
    {
        public LexicalItemId? LexicalItemId
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

#if DEBUG
        private ObservableCollection<Definition> definitions_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Definition> definitions_;
        private ObservableCollection<Definition> definitions_;
#endif

        public ObservableCollection<Definition> Definitions
        {
            get { return definitions_; }
#if DEBUG
            set { definitions_ = value; }
#else
            // RELEASE MODIFIED
            //set { definitions_ = value; }
            set { definitions_ = value; }
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

        public LexicalItem()
        {
            definitions_ = new ObservableCollection<Definition>();
            forms_ = new ObservableCollection<Form>();
        }
        internal LexicalItem(LexicalItemId lexicalItemId, string lemma, string? language, ICollection<Definition> definitions, ICollection<Form> forms)
        {
            LexicalItemId = lexicalItemId;
            Lemma = lemma;
            Language = language;
            definitions_ = new ObservableCollection<Definition>(definitions.DistinctBy(l => l.DefinitionId)); ;
            forms_ = new ObservableCollection<Form>(forms.DistinctBy(l => l.FormId)); ;
        }

        public async Task<LexicalItem> Create(IMediator mediator, CancellationToken token = default)
        {
            var command = new CreateOrUpdateLexicalItemCommand(null, Lemma ?? string.Empty, Language);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            LexicalItemId = result.Data!;
            return this;
        }

        public async Task PutDefinition(IMediator mediator, Definition definition, CancellationToken token = default)
        {
            if (definition.DefinitionId is not null && 
                definitions_.Any(l => l.DefinitionId == definition.DefinitionId))
            {
                return;
            }

            if (LexicalItemId is null)
            {
                throw new MediatorErrorEngineException("Create LexicalItem before associating with given Definition");
            }

            var result = await mediator.Send(new PutDefinitionCommand(LexicalItemId, definition), token);
            result.ThrowIfCanceledOrFailed();

            definition.DefinitionId = result.Data!;

            definitions_.Add(definition);
        }

        public async Task PutForm(IMediator mediator, Form form, CancellationToken token = default)
        {
            if (form.FormId is not null &&
                forms_.Any(f => f.FormId == form.FormId))
            {
                return;
            }

            if (LexicalItemId is null)
            {
                throw new MediatorErrorEngineException("Create LexicalItem before associating with given Form");
            }

            var result = await mediator.Send(new PutFormCommand(LexicalItemId, form), token);
            result.ThrowIfCanceledOrFailed();

            form.FormId = result.Data!;

            forms_.Add(form);
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (LexicalItemId == null)
            {
                return;
            }

            var command = new DeleteLexicalItemAndDependentsCommand(LexicalItemId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public static async Task<LexicalItem?> Get(
            IMediator mediator,
            string lemma,
            string? language,
            string? definitionLanguage,
            CancellationToken token = default)
        {
            var command = new GetLexicalItemByTextQuery(lemma, language, definitionLanguage);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            return result.Data!;
        }
    }
}
