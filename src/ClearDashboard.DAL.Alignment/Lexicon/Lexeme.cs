using System.Collections.ObjectModel;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Lexeme : IEquatable<Lexeme>
    {
        public LexemeId LexemeId
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

        private string? lemma_;
        public string? Lemma 
        {
            get => lemma_;
            set
            {
                lemma_ = value;
                IsDirty = true;
            }
        }

        private string? language_;
        public string? Language 
        {
            get => language_;
            set
            {
                language_ = value;
                IsDirty = true;
            }
        }

        private string? type_;
        public string? Type 
        { 
            get => type_;
            set
            {
                type_ = value;
                IsDirty = true;
            }
        }

        private readonly List<MeaningId> meaningIdsInDatabase_;
        internal IEnumerable<MeaningId> MeaningIdsToDelete => meaningIdsInDatabase_.ExceptBy(meanings_.Select(e => e.MeaningId.Id), e => e.Id);

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

        private readonly List<FormId> formIdsInDatabase_;
        internal IEnumerable<FormId> FormIdsToDelete => formIdsInDatabase_.ExceptBy(forms_.Select(e => e.FormId.Id), e => e.Id);

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

        public bool IsDirty { get; internal set; } = false;
        public bool IsInDatabase { get => LexemeId.Created is not null; }

        public IEnumerable<string> LemmaPlusFormTexts
        {
            get => (string.IsNullOrEmpty(lemma_))
                ? forms_
                    .Where(e => !string.IsNullOrEmpty(e.Text))
                    .Select(e => e.Text!)
                : new List<string>() { lemma_ }.Union(forms_
                    .Where(e => !string.IsNullOrEmpty(e.Text))
                    .Select(e => e.Text!));
        }

        public Lexeme()
        {
            LexemeId = LexemeId.Create(Guid.NewGuid());

            meanings_ = new ObservableCollection<Meaning>();
            meaningIdsInDatabase_ = new();

            forms_ = new ObservableCollection<Form>();
            formIdsInDatabase_ = new();
        }
        internal Lexeme(LexemeId lexemeId, string lemma, string? language, string? type, ICollection<Meaning> meanings, ICollection<Form> forms)
        {
            LexemeId = lexemeId;

            lemma_ = lemma;
            language_ = language;
            type_ = type;

            meanings_ = new ObservableCollection<Meaning>(meanings.DistinctBy(s => s.MeaningId));
            meaningIdsInDatabase_ = new(meanings.Select(f => f.MeaningId));

            forms_ = new ObservableCollection<Form>(forms.DistinctBy(f => f.FormId));
            formIdsInDatabase_ = new(forms.Select(f => f.FormId));
        }

        public async Task<Lexeme> Create(IMediator mediator, CancellationToken token = default)
        {
            var command = new CreateOrUpdateLexemeCommand(LexemeId, Lemma ?? string.Empty, Language, Type);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            PostSave(result.Data!);
            return this;
        }

        public async Task PutMeaning(IMediator mediator, Meaning meaning, CancellationToken token = default)
        {
            if (!IsInDatabase)
            {
                throw new MediatorErrorEngineException("Create Lexeme before associating with given Meaning");
            }

            if (meaning.IsInDatabase && !meaning.IsDirty)
            {
                return;
            }

            var result = await mediator.Send(new PutMeaningCommand(LexemeId, meaning), token);
            result.ThrowIfCanceledOrFailed();

            meaning.PostSave(result.Data!);

            if (!meanings_.Contains(meaning))
            {
                meanings_.Add(meaning);
            }

            if (!meaningIdsInDatabase_.Contains(meaning.MeaningId, new IIdEqualityComparer()))
            {
                meaningIdsInDatabase_.Add(meaning.MeaningId);
            }
        }

        public async Task DeleteMeaning(IMediator mediator, Meaning meaning, CancellationToken token = default)
        {
            if (!meaning.IsInDatabase)
            {
                return;
            }

            // If there are any SemanticDomains associated with the meaning being deleted,
            // the SemanticDomainMeaningAssociation in the DB should automatically get deleted.
            var command = new DeleteMeaningCommand(meaning.MeaningId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            meaning.PostSave(SimpleSynchronizableTimestampedEntityId<MeaningId>.Create(meaning.MeaningId.Id));

            meanings_.Remove(meaning);
            meaningIdsInDatabase_.RemoveAll(e => e.Id == meaning.MeaningId.Id);
        }

        public async Task PutForm(IMediator mediator, Form form, CancellationToken token = default)
        {
            if (!IsInDatabase)
            {
                throw new MediatorErrorEngineException("Create Lexeme before associating with given Form");
            }

            if (form.IsInDatabase && !form.IsDirty)
            {
                return;
            }

            var result = await mediator.Send(new PutFormCommand(LexemeId, form), token);
            result.ThrowIfCanceledOrFailed();

            form.PostSave(result.Data!);

            if (!forms_.Contains(form))
            {
                forms_.Add(form);
            }
          
            if (!formIdsInDatabase_.Contains(form.FormId, new IIdEqualityComparer()))
            {
                formIdsInDatabase_.Add(form.FormId);
            }
        }

        public async Task DeleteForm(IMediator mediator, Form form, CancellationToken token = default)
        {
            if (!form.IsInDatabase)
            {
                return;
            }

            var command = new DeleteFormCommand(form.FormId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            form.PostSave(SimpleSynchronizableTimestampedEntityId<FormId>.Create(form.FormId.Id));

            forms_.Remove(form);
            formIdsInDatabase_.RemoveAll(e => e.Id == form.FormId.Id);
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (!IsInDatabase)
            {
                return;
            }

            var command = new DeleteLexemeAndDependentsCommand(LexemeId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            LexemeId = SimpleSynchronizableTimestampedEntityId<LexemeId>.Create(LexemeId.Id);
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

        internal void PostSave(LexemeId? lexemeId)
        {
            LexemeId = lexemeId ?? LexemeId;
            IsDirty = false;
        }

        internal void PostSaveAll(IDictionary<Guid, IId> createdIIdsByGuid)
        {
            createdIIdsByGuid.TryGetValue(LexemeId.Id, out var lexemeId);
            PostSave((LexemeId?)lexemeId);

            formIdsInDatabase_.Clear();
            formIdsInDatabase_.AddRange(forms_.Select(e => e.FormId));

            foreach (var form in forms_)
            {
                form.PostSaveAll(createdIIdsByGuid);
            }

            meaningIdsInDatabase_.Clear();
            meaningIdsInDatabase_.AddRange(meanings_.Select(e => e.MeaningId));

            foreach (var meaning in meanings_)
            {
                meaning.PostSaveAll(createdIIdsByGuid);
            }
        }

        public override bool Equals(object? obj) => Equals(obj as Lexeme);
        public bool Equals(Lexeme? other)
        {
            if (other is null) return false;
            if (!LexemeId.Id.Equals(other.LexemeId.Id)) return false;

            return true;
        }
        public override int GetHashCode() => LexemeId.Id.GetHashCode();
        public static bool operator ==(Lexeme? e1, Lexeme? e2) => Equals(e1, e2);
        public static bool operator !=(Lexeme? e1, Lexeme? e2) => !(e1 == e2);
    }
}
