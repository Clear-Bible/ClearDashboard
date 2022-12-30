using System.Linq;
using MediatR;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;

namespace ClearDashboard.Wpf.Application.Services
{
    public sealed class SelectionManager : PropertyChangedBase
    {
        private IEventAggregator EventAggregator { get; }
        private ILogger<VerseManager> Logger { get; }
        private IMediator Mediator { get; }

        private TokenDisplayViewModelCollection _selectedTokens = new();
        private TokenDisplayViewModelCollection SelectedTokens
        {
            get => _selectedTokens;
            set
            {
                Set(ref _selectedTokens, value);
                NotifyOfPropertyChange(nameof(SelectedEntityIds));
                NotifyOfPropertyChange(nameof(SelectedNoteIds));
            }
        }

        public EntityIdCollection SelectedEntityIds => SelectedTokens.EntityIds;
        public NoteIdCollection SelectedNoteIds => SelectedTokens.NoteIds;
        public bool AnySelectedNotes => SelectedTokens.Any(t => t.HasNote);

        private void SelectionUpdated()
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

                if (!token.IsTokenSelected)
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

        public SelectionManager(IEventAggregator eventAggregator, ILogger<VerseManager> logger, IMediator mediator)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
        }
    }
}
