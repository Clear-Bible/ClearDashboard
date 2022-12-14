using System.Collections.ObjectModel;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment.Features;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using MediatR;
using Microsoft.SqlServer.Server;

namespace ClearDashboard.DAL.Alignment.Lexicon
{
    public class Definition
    {
        public DefinitionId? DefinitionId
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

        public Definition()
        {
            translations_ = new ObservableCollection<Translation>();
            semanticDomains_ = new ObservableCollection<SemanticDomain>();
        }
        internal Definition(DefinitionId definitionId, string text, string? language, ICollection<Translation> translations, ICollection<SemanticDomain> semanticDomains)
        {
            DefinitionId = definitionId;
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

            if (DefinitionId is null)
            {
                throw new MediatorErrorEngineException("Create Definition before associating with given Translation");
            }

            var result = await mediator.Send(new PutTranslationCommand(DefinitionId, translation), token);
            result.ThrowIfCanceledOrFailed();

            translation.TranslationId = result.Data!;

            translations_.Add(translation);
        }

        public async Task<SemanticDomain> CreateAssociateSenanticDomain(IMediator mediator, string text, CancellationToken token = default)
        {
            if (DefinitionId is null)
            {
                throw new MediatorErrorEngineException("'CreateOrUpdate Definition before associating with SemanticDomain");
            }

            var semanticDomain = await new SemanticDomain { Text = text }.Create(mediator, token);
            await AssociateSemanticDomain(mediator, semanticDomain, token);

            return semanticDomain;
        }

        /// <summary>
        /// NOTE:  this method alters a the "SemanticDomains" ObservableCollection that was created in the Definition 
        /// constructor.  For each UI thread that is going to access this method (really,the Labels ObservableCollection in general), 
        /// a WPF-layer caller should establish a “lock” object, tell WPF about it 
        /// (using EnableCollectionSynchronization(definition.SemanticDomains, theLockObject)), and use it in a “lock” 
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

            if (DefinitionId is null)
            {
                throw new MediatorErrorEngineException("Create Definition before associating with given SemanticDomain");
            }
            if (semanticDomain.SemanticDomainId is null)
            {
                throw new MediatorErrorEngineException("Create given SemanticDomain before associating with Definition");
            }

            var command = new CreateSemanticDomainDefinitionAssociationCommand(semanticDomain.SemanticDomainId, DefinitionId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            semanticDomains_.Add(semanticDomain);
        }

        /// <summary>
        /// NOTE:  this method alters a the "SemanticDomains" ObservableCollection that was created in the Definition 
        /// constructor.  For each UI thread that is going to access this method (really,the Labels ObservableCollection in general), 
        /// a WPF-layer caller should establish a “lock” object, tell WPF about it 
        /// (using EnableCollectionSynchronization(definition.SemanticDomains, theLockObject)), and use it in a “lock” 
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

            if (DefinitionId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach semantic domain before Definition has been created");
            }
            if (semanticDomain.SemanticDomainId is null)
            {
                throw new MediatorErrorEngineException("Cannot detach label before it has been created/attached");
            }

            var command = new DeleteSemanticDomainDefinitionAssociationCommand(semanticDomain.SemanticDomainId, DefinitionId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();

            semanticDomains_.Remove(semanticDomainMatch);
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (DefinitionId == null)
            {
                return;
            }

            var command = new DeleteDefinitionCommand(DefinitionId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }
    }
}
