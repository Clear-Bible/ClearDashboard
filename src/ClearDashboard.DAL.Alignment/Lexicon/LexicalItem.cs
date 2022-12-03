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
        public string? Text { get; set; }
        public string? Language { get; set; }
        public string? Type { get; set; }

#if DEBUG
        private ObservableCollection<LexicalItemDefinition> lexicalItemDefinitions_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<SemanticDomain> semanticDomains_;
        private ObservableCollection<SemanticDomain> semanticDomains_;
#endif

        public ObservableCollection<LexicalItemDefinition> LexicalItemDefinitions
        {
            get { return lexicalItemDefinitions_; }
#if DEBUG
            set { lexicalItemDefinitions_ = value; }
#else
            // RELEASE MODIFIED
            //set { lexicalItemDefinitions_ = value; }
            set { lexicalItemDefinitions_ = value; }
#endif
        }

        public LexicalItem()
        {
            lexicalItemDefinitions_ = new ObservableCollection<LexicalItemDefinition>();
        }
        internal LexicalItem(LexicalItemId lexicalItemId, string text, string? language, string? type, ICollection<LexicalItemDefinition> lexicalItemDefinitions)
        {
            LexicalItemId = lexicalItemId;
            Text = text;
            Language = language;
            Type = type;
            lexicalItemDefinitions_ = new ObservableCollection<LexicalItemDefinition>(lexicalItemDefinitions.DistinctBy(l => l.LexicalItemDefinitionId)); ;
        }

        public async Task<LexicalItem> Create(IMediator mediator, CancellationToken token = default)
        {
            var command = new CreateLexicalItemCommand(Text ?? string.Empty, Language, Type);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            LexicalItemId = result.Data!;
            return this;
        }

        public async Task PutLexicalItemDefinition(IMediator mediator, LexicalItemDefinition lexicalItemDefinition, CancellationToken token = default)
        {
            if (lexicalItemDefinition.LexicalItemDefinitionId is not null && 
                lexicalItemDefinitions_.Any(l => l.LexicalItemDefinitionId == lexicalItemDefinition.LexicalItemDefinitionId))
            {
                return;
            }

            if (LexicalItemId is null)
            {
                throw new MediatorErrorEngineException("Create LexicalItem before associating with given LexicalItemDefinition");
            }

            var result = await mediator.Send(new PutLexicalItemDefinitionCommand(LexicalItemId, lexicalItemDefinition), token);
            result.ThrowIfCanceledOrFailed();

            lexicalItemDefinition.LexicalItemDefinitionId = result.Data!;

            lexicalItemDefinitions_.Add(lexicalItemDefinition);
        }

        public async Task Delete(IMediator mediator, CancellationToken token = default)
        {
            if (LexicalItemId == null)
            {
                return;
            }

            var command = new DeleteLexicalItemAndDefinitionsCommand(LexicalItemId);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed();
        }

        public static async Task<LexicalItem?> Get(
            IMediator mediator,
            string text,
            CancellationToken token = default)
        {
            var command = new GetLexicalItemByTextQuery(text);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
    }
}
