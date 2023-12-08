using System.Collections.ObjectModel;
using ClearBible.Engine.Utils;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;
using Microsoft.SqlServer.Server;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Meaning : IEquatable<Meaning>
    {
        public MeaningId MeaningId
        {
            get;
#if DEBUG
            set;
#else 
            // RELEASE MODIFIED
            //internal set;
            set;
#endif
        }

        private string? text_;
        public string? Text
        {
            get => text_;
            set
            {
                text_ = value;
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

        public bool HasAnythingToSave =>
            IsDirty ||
            TranslationIdsToAdd.Any() ||
            TranslationIdsToDelete.Any() ||
            translations_.IntersectBy(translationIdsInDatabase_.Select(e => e.Id), e => e.TranslationId.Id).Any(e => e.HasAnythingToSave);

        private readonly List<TranslationId> translationIdsInDatabase_;
        internal IEnumerable<TranslationId> TranslationIdsToDelete => translationIdsInDatabase_.ExceptBy(translations_.Select(e => e.TranslationId.Id), e => e.Id);
        internal IEnumerable<TranslationId> TranslationIdsToAdd => translations_.Where(e => !e.ExcludeFromSave).Select(e => e.TranslationId).ExceptBy(translationIdsInDatabase_.Select(e => e.Id), e => e.Id);

#if DEBUG
        private ObservableCollection<Translation> translations_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<Translation> translations_;
        private ObservableCollection<Translation> translations_;
#endif

        public ObservableCollection<Translation> Translations
        {
            get { return translations_; }
#if DEBUG
            set { translations_ = value; }
#else
            // RELEASE MODIFIED
            //set { translations_ = value; }
            set { translations_ = value; }
#endif
        }

#if DEBUG
        private ObservableCollection<SemanticDomain> semanticDomains_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<SemanticDomain> semanticDomains_;
        private ObservableCollection<SemanticDomain> semanticDomains_;
#endif

        public ObservableCollection<SemanticDomain> SemanticDomains
        {
            get { return semanticDomains_; }
#if DEBUG
            set { semanticDomains_ = value; }
#else
            // RELEASE MODIFIED
            //set { semanticDomains_ = value; }
            set { semanticDomains_ = value; }
#endif
        }

        public bool IsDirty { get; internal set; } = false;
        public bool IsInDatabase { get => MeaningId.Created is not null; }

        public Meaning()
        {
            MeaningId = MeaningId.Create(Guid.NewGuid());

            translations_ = new ObservableCollection<Translation>();
            translationIdsInDatabase_ = new();

            semanticDomains_ = new ObservableCollection<SemanticDomain>();
        }
        internal Meaning(MeaningId meaningId, string text, string? language, ICollection<Translation> translations, ICollection<SemanticDomain> semanticDomains)
        {
            MeaningId = meaningId;

            text_ = text;
            language_ = language;

            translations_ = new ObservableCollection<Translation>(translations.DistinctBy(t => t.TranslationId)); ;
            translationIdsInDatabase_ = new(translations.Select(f => f.TranslationId));

            semanticDomains_ = new ObservableCollection<SemanticDomain>(semanticDomains.DistinctBy(sd => sd.SemanticDomainId)); ;
        }

        public async Task PutTranslation(IMediator mediator, Translation translation, CancellationToken token = default)
        {
            if (!IsInDatabase)
            {
                throw new MediatorErrorEngineException("Create Meaning before associating with given Translation");
            }

            if (translation.IsInDatabase && !translation.IsDirty)
            {
                return;
            }

            var result = await mediator.Send(new PutTranslationCommand(MeaningId, translation), token);
            result.ThrowIfCanceledOrFailed();

            translation.PostSave(result.Data!);

            if (!translations_.Contains(translation))
            {
                translations_.Add(translation);
            }

            if (!translationIdsInDatabase_.Contains(translation.TranslationId, new IIdEqualityComparer()))
            {
                translationIdsInDatabase_.Add(translation.TranslationId);
            }
        }

        public void DeleteTranslations(IEnumerable<Translation> translations)
        {
            foreach (var translation in translations)
            {
               translations_.Remove(translation);
            }
        }

        public async Task DeleteTranslation(IMediator mediator, Translation translation, CancellationToken token = default)
        {
            if (!translation.IsInDatabase)
            {
                return;
            }

            var command = new DeleteTranslationCommand(translation.TranslationId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            translation.PostSave(SimpleSynchronizableTimestampedEntityId<TranslationId>.Create(translation.TranslationId.Id));

            translations_.Remove(translation);
            translationIdsInDatabase_.RemoveAll(e => e.Id == translation.TranslationId.Id);
        }

        public async Task<SemanticDomain> CreateAssociateSenanticDomain(IMediator mediator, string text, CancellationToken token = default)
        {
            if (!IsInDatabase)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Meaning before associating with SemanticDomain");
            }

            var semanticDomain = await new SemanticDomain { Text = text }.Create(mediator, token);
            await AssociateSemanticDomain(mediator, semanticDomain, token);

            return semanticDomain;
        }

        /// <summary>
        /// NOTE:  this method alters a the "SemanticDomains" ObservableCollection that was created in the Meaning 
        /// constructor.  For each UI thread that is going to access this method (really,the Labels ObservableCollection in general), 
        /// a WPF-layer caller should establish a “lock” object, tell WPF about it 
        /// (using EnableCollectionSynchronization(meaning.SemanticDomains, theLockObject)), and use it in a “lock” 
        /// statement every time those methods are called.  Summarized from:
        /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.data.bindingoperations.enablecollectionsynchronization
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="semanticDomain"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public async Task AssociateSemanticDomain(IMediator mediator, SemanticDomain semanticDomain, CancellationToken token = default)
        {
            if (semanticDomains_.Any(l => l.SemanticDomainId == semanticDomain.SemanticDomainId))
            {
                return;
            }

            if (!IsInDatabase)
            {
                throw new MediatorErrorEngineException("Create Meaning before associating with given SemanticDomain");
            }
            if (semanticDomain.SemanticDomainId is null)
            {
                throw new MediatorErrorEngineException("Create given SemanticDomain before associating with Meaning");
            }

            var command = new CreateSemanticDomainMeaningAssociationCommand(semanticDomain.SemanticDomainId, MeaningId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            semanticDomains_.Add(semanticDomain);
        }

        /// <summary>
        /// NOTE:  this method alters a the "SemanticDomains" ObservableCollection that was created in the Meaning 
        /// constructor.  For each UI thread that is going to access this method (really,the Labels ObservableCollection in general), 
        /// a WPF-layer caller should establish a “lock” object, tell WPF about it 
        /// (using EnableCollectionSynchronization(meaning.SemanticDomains, theLockObject)), and use it in a “lock” 
        /// statement every time those methods are called.  Summarized from:
        /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.data.bindingoperations.enablecollectionsynchronization
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="semanticDomain"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="MediatorErrorEngineException"></exception>
        public async Task DetachSemanticDomain(IMediator mediator, SemanticDomain semanticDomain, CancellationToken token = default)
        {
            var semanticDomainMatch = semanticDomains_.FirstOrDefault(l => l.SemanticDomainId == semanticDomain.SemanticDomainId);
            if (semanticDomainMatch is null)
            {
                return;
            }

            if (!IsInDatabase)
            {
                throw new MediatorErrorEngineException("Cannot detach semantic domain before Meaning has been created");
            }
            if (semanticDomain.SemanticDomainId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach label before it has been created/attached");
            }

            var command = new DeleteSemanticDomainMeaningAssociationCommand(semanticDomain.SemanticDomainId, MeaningId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            semanticDomains_.Remove(semanticDomainMatch);
        }

        internal void PostSave(MeaningId? meaningId)
        {
            MeaningId = meaningId ?? MeaningId;
            IsDirty = false;
        }

        internal void PostSaveAll(IDictionary<Guid, IId> createdIIdsByGuid)
        {
            createdIIdsByGuid.TryGetValue(MeaningId.Id, out var meaningId);
            PostSave((MeaningId?)meaningId);

            translationIdsInDatabase_.Clear();
            translationIdsInDatabase_.AddRange(translations_.Select(e => e.TranslationId));

            foreach (var translation in translations_)
            {
                translation.PostSaveAll(createdIIdsByGuid);
            }
        }

        public override bool Equals(object? obj) => Equals(obj as Meaning);
        public bool Equals(Meaning? other)
        {
            if (other is null) return false;
            if (!MeaningId.Id.Equals(other.MeaningId.Id)) return false;

            return true;
        }
        public override int GetHashCode() => MeaningId.Id.GetHashCode();
        public static bool operator ==(Meaning? e1, Meaning? e2) => Equals(e1, e2);
        public static bool operator !=(Meaning? e1, Meaning? e2) => !(e1 == e2);
    }
}
