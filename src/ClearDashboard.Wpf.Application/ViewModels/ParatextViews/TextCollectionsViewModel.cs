using Caliburn.Micro;
using ClearDashboard.DataAccessLayer.Wpf;
using ClearDashboard.ParatextPlugin.CQRS.Features.TextCollections;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ClearDashboard.Wpf.Application.Helpers;
using ClearDashboard.Wpf.Application.Views.ParatextViews;
using Autofac;

namespace ClearDashboard.Wpf.Application.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TextCollectionsViewModel : ToolViewModel, IHandle<TextCollectionChangedMessage>,
        IHandle<VerseChangedMessage>
    {

        #region Member Variables

        private string _currentVerse = "";

        #endregion //Member Variables

        #region Public Properties


        #endregion //Public Properties

        #region Observable Properties

        private ObservableCollection<TextCollectionList> _textCollectionLists = new();

        public ObservableCollection<TextCollectionList> TextCollectionLists
        {
            get { return _textCollectionLists; }
            set
            {
                _textCollectionLists = value;
                NotifyOfPropertyChange(() => TextCollectionLists);
            }
        }


        #endregion //Observable Properties

        #region Constructor
        // ReSharper disable once UnusedMember.Global
        public TextCollectionsViewModel()
        {
            // no-op this is here for the XAML design time
        }

        public TextCollectionsViewModel(INavigationService navigationService, ILogger<TextCollectionsViewModel> logger,
            DashboardProjectManager projectManager, IEventAggregator eventAggregator, IMediator mediator, ILifetimeScope lifetimeScope) : base(
            navigationService, logger, projectManager, eventAggregator, mediator, lifetimeScope)
        {
            this.Title = "🗐 TEXT COLLECTION";
            this.ContentId = "TEXTCOLLECTION";
        }

        protected override async void OnViewAttached(object view, object context)
        {
            await CallGetTextCollections().ConfigureAwait(false);
            base.OnViewAttached(view, context);
        }

        #endregion //Constructor

        #region Methods

        private async Task CallGetTextCollections()
        {
            try
            {
                var result = await ExecuteRequest(new GetTextCollectionsQuery(), CancellationToken.None)
                    .ConfigureAwait(false);
                await EventAggregator.PublishOnUIThreadAsync(
                    new LogActivityMessage($"{this.DisplayName}: TextCollections read"));


                if (result.Success)
                {
                    OnUIThread(() =>
                    {
                        TextCollectionLists.Clear();
                        var data = result.Data;

                        foreach (var textCollection in data)
                        {
                            TextCollectionList tc = new();

                            var endPart = textCollection.Data;
                            var startPart = textCollection.ReferenceShort;

                            tc.Inlines.Insert(0, new Run(endPart) { FontWeight = FontWeights.Normal });
                            tc.Inlines.Insert(0, new Run(startPart + ":  ") { FontWeight = FontWeights.Bold, Foreground = Brushes.Cyan });

                            TextCollectionLists.Add(tc);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"BiblicalTermsViewModel Deserialize BiblicalTerms: {e.Message}");
            }
        }

        public void LaunchMirrorView(double actualWidth, double actualHeight)
        {
            LaunchMirrorView<TextCollectionsView>.Show(this, actualWidth, actualHeight);
        }

        public Task HandleAsync(TextCollectionChangedMessage message, CancellationToken cancellationToken)
        {
            // TODO
            return Task.CompletedTask;
        }

        public async Task HandleAsync(VerseChangedMessage message, CancellationToken cancellationToken)
        {
            if (_currentVerse != message.Verse.PadLeft(9, '0'))
            {
                _currentVerse = message.Verse.PadLeft(9, '0');
                await CallGetTextCollections().ConfigureAwait(false);
            }
        }

        #endregion // Methods


    }
}
