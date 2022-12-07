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
        public string? TrainingText { get; set; }
        public string? Language { get; set; }

#if DEBUG
        private ObservableCollection<LexicalItemDefinition> lexicalItemDefinitions_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<LexicalItemDefinition> lexicalItemDefinitions_;
        private ObservableCollection<LexicalItemDefinition> lexicalItemDefinitions_;
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

#if DEBUG
        private ObservableCollection<LexicalItemSurfaceText> lexicalItemSurfaceTexts_;
#else
        // RELEASE MODIFIED
        //private readonly ObservableCollection<LexicalItemSurfaceText> lexicalItemSurfaceTexts_;
        private ObservableCollection<LexicalItemSurfaceText> lexicalItemSurfaceTexts_;
#endif

        public ObservableCollection<LexicalItemSurfaceText> LexicalItemSurfaceTexts
        {
            get { return lexicalItemSurfaceTexts_; }
#if DEBUG
            set { lexicalItemSurfaceTexts_ = value; }
#else
            // RELEASE MODIFIED
            //set { lexicalItemSurfaceTexts_ = value; }
            set { lexicalItemSurfaceTexts_ = value; }
#endif
        }

        public LexicalItem()
        {
            lexicalItemDefinitions_ = new ObservableCollection<LexicalItemDefinition>();
            lexicalItemSurfaceTexts_ = new ObservableCollection<LexicalItemSurfaceText>();
        }
        internal LexicalItem(LexicalItemId lexicalItemId, string trainingText, string? language, ICollection<LexicalItemDefinition> lexicalItemDefinitions, ICollection<LexicalItemSurfaceText> lexicalItemSurfaceTexts)
        {
            LexicalItemId = lexicalItemId;
            TrainingText = trainingText;
            Language = language;
            lexicalItemDefinitions_ = new ObservableCollection<LexicalItemDefinition>(lexicalItemDefinitions.DistinctBy(l => l.LexicalItemDefinitionId)); ;
            lexicalItemSurfaceTexts_ = new ObservableCollection<LexicalItemSurfaceText>(lexicalItemSurfaceTexts.DistinctBy(l => l.LexicalItemSurfaceTextId)); ;
        }

        public async Task<LexicalItem> Create(IMediator mediator, CancellationToken token = default)
        {
            var command = new CreateOrUpdateLexicalItemCommand(null, TrainingText ?? string.Empty, Language);

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

        public async Task PutLexicalItemSurfaceText(IMediator mediator, LexicalItemSurfaceText lexicalItemSurfaceText, CancellationToken token = default)
        {
            if (lexicalItemSurfaceText.LexicalItemSurfaceTextId is not null &&
                lexicalItemSurfaceTexts_.Any(l => l.LexicalItemSurfaceTextId == lexicalItemSurfaceText.LexicalItemSurfaceTextId))
            {
                return;
            }

            if (LexicalItemId is null)
            {
                throw new MediatorErrorEngineException("Create LexicalItem before associating with given LexicalItemSurfaceText");
            }

            var result = await mediator.Send(new PutLexicalItemSurfaceTextCommand(LexicalItemId, lexicalItemSurfaceText), token);
            result.ThrowIfCanceledOrFailed();

            lexicalItemSurfaceText.LexicalItemSurfaceTextId = result.Data!;

            lexicalItemSurfaceTexts_.Add(lexicalItemSurfaceText);
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
            string trainingText,
            string language,
            string? definitionLanguage,
            CancellationToken token = default)
        {
            var command = new GetLexicalItemByTextQuery(trainingText, language, definitionLanguage);

            var result = await mediator.Send(command, token);
            result.ThrowIfCanceledOrFailed(true);

            return result.Data!;
        }
    }
}
