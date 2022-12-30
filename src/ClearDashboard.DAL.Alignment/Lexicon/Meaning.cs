using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;
using Microsoft.SqlServer.Server;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Meaning
    {
        public MeaningId? MeaningId
        {
            get;
#if DEBUG
            internal set;
#else 
            // RELEASE MODIFIED
            //internal set;
            set;
#endif
        }
        public string? Text { get; set; }
        public string? Language { get; set; }

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

        public Meaning()
        {
            translations_ = new ObservableCollection<Translation>();
            semanticDomains_ = new ObservableCollection<SemanticDomain>();
        }
        internal Meaning(MeaningId meaningId, string text, string? language, ICollection<Translation> translations, ICollection<SemanticDomain> semanticDomains)
        {
            MeaningId = meaningId;
            Text = text;
            Language = language;
            translations_ = new ObservableCollection<Translation>(translations.DistinctBy(t => t.TranslationId)); ;
            semanticDomains_ = new ObservableCollection<SemanticDomain>(semanticDomains.DistinctBy(sd => sd.SemanticDomainId)); ;
        }

        public async Task PutTranslation(IMediator mediator, Translation translation, CancellationToken token = default)
        {
            if (translation.TranslationId is not null &&
                translations_.Any(t => t.TranslationId == translation.TranslationId))
            {
                return;
            }

            if (MeaningId is null)
            {
                throw new MediatorErrorEngineException("Create Meaning before associating with given Translation");
            }

            var result = await mediator.Send(new PutTranslationCommand(MeaningId, translation), token);
            result.ThrowIfCanceledOrFailed();

            translation.TranslationId = result.Data!;

            translations_.Add(translation);
        }

        public async Task<SemanticDomain> CreateAssociateSenanticDomain(IMediator mediator, string text, CancellationToken token = default)
        {
            if (MeaningId is null)
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

            if (MeaningId is null)
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

            if (MeaningId is null)
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

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (MeaningId == null)
            {
                return;
            }

            var command = new DeleteMeaningCommand(MeaningId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }
    }
}
