using System.Linq;
using MediatR;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using System.Threading.Tasks;
using System.Threading;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class SelectionManager : PropertyChangedBase, IHandle<TokensJoinedMessage>, IHandle<TokenUnjoinedMessage>
    {
        private IEventAggregator EventAggregator { get; }
        private ILogger<SelectionManager> Logger { get; }
        private IMediator Mediator { get; }

        private TokenDisplayViewModelCollection _selectedTokens = new();
        public TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set
            {
                Set(ref _selectedTokens, value);
                NotifyOfPropertyChange(nameof(SelectedEntityIds));
                NotifyOfPropertyChange(nameof(SelectedNoteIds));
            }
        }

        public bool AnySourceTokens => SelectedTokens.Any(t => t.IsSource && (t.IsTokenSelected || t.IsTranslationSelected));
        public bool AnyTargetTokens => SelectedTokens.Any(t => t.IsTarget && (t.IsTokenSelected || t.IsTranslationSelected));

        public TokenDisplayViewModelCollection SelectedSourceTokens => new(SelectedTokens.Where(t => t.IsSource && (t.IsTokenSelected || t.IsTranslationSelected)));
        public TokenDisplayViewModelCollection SelectedTargetTokens => new(SelectedTokens.Where(t => t.IsTarget && (t.IsTokenSelected || t.IsTranslationSelected)));

        public EntityIdCollection SelectedEntityIds => SelectedTokens.EntityIds;
        public NoteIdCollection SelectedNoteIds => SelectedTokens.NoteIds;
        public bool AnySelectedNotes => SelectedTokens.Any(t => t.HasNote);

        public void SelectionUpdated()
        {
            EventAggregator.PublishOnUIThreadAsync(new SelectionUpdatedMessage(SelectedTokens));
        }

        public void UpdateSelection(TokenDisplayViewModel token, TokenDisplayViewModelCollection selectedTokens, bool addToSelection)
        {
            if (addToSelection)
            {
                foreach (var selectedToken in selectedTokens)
                {
                    if (!SelectedTokens.Contains(selectedToken))
                    {
                        SelectedTokens.Add(selectedToken);
                    }
                }

                if (!token.IsTokenSelected && !token.IsTranslationSelected)
                {
                    SelectedTokens.Remove(token);
                }
                NotifyOfPropertyChange(nameof(SelectedEntityIds));
                NotifyOfPropertyChange(nameof(SelectedNoteIds));
            }
            else
            {
                SelectedTokens = selectedTokens;
            }
            SelectionUpdated();
        }

        public void UpdateRightClickSelection(TokenDisplayViewModel token)
        {
            if (!SelectedTokens.Contains(token))
            {
                SelectedTokens = new TokenDisplayViewModelCollection(token);
                SelectionUpdated();
            }
        }

        public async Task HandleAsync(TokensJoinedMessage message, CancellationToken cancellationToken)
        {
            SelectedTokens = new TokenDisplayViewModelCollection();
            SelectionUpdated();
            await Task.CompletedTask;
        }

        public async Task HandleAsync(TokenUnjoinedMessage message, CancellationToken cancellationToken)
        {
            SelectedTokens = new TokenDisplayViewModelCollection();
            SelectionUpdated();
            await Task.CompletedTask;
        }

        public SelectionManager(IEventAggregator eventAggregator, ILogger<SelectionManager> logger, IMediator mediator)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;

            EventAggregator.SubscribeOnUIThread(this);
        }
    }
}
