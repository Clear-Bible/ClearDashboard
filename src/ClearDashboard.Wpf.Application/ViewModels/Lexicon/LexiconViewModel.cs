using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autofac;
using Caliburn.Micro;
using ClearBible.Engine.SyntaxTree.Aligner.Legacy;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.ParatextPlugin.CQRS.Features.Lexicon;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Lexicon = ClearDashboard.DAL.Alignment.Lexicon.Lexicon;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconViewModel : ToolViewModel
    {

        private LexiconManager LexiconManager { get; }
        public LexiconViewModel()
        {
            
        }
        public LexiconViewModel(INavigationService navigationService, 
            ILogger<LexiconViewModel> logger, 
            DashboardProjectManager dashboardProjectManager, 
            IEventAggregator eventAggregator, 
            IMediator mediator, 
            ILifetimeScope lifetimeScope, 
            ILocalizationService localizationService,
            LexiconManager lexiconManager) :
            base(navigationService, logger, dashboardProjectManager, eventAggregator, mediator, lifetimeScope, localizationService)
        {
            LexiconManager = lexiconManager;
        }


        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            return base.OnInitializeAsync(cancellationToken);
        }

        public BindableCollection<LexiconImportViewModel> LexiconImports { get; private set; } = new BindableCollection<LexiconImportViewModel>();

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            var projectLexicon = await LexiconManager.GetLexiconForProject(null);
            if (projectLexicon != null)
            {
                var externalLexicon = await LexiconManager.GetExternalLexiconNotInInternal(projectLexicon, cancellationToken);

                //Logger.LogDebug("************************************************");
                //Logger.LogDebug(JsonConvert.SerializeObject(projectLexicon, Formatting.Indented));
                //Logger.LogDebug("************************************************");

                LexiconImports = new BindableCollection<LexiconImportViewModel>(projectLexicon.Lexemes.Select(lexeme => new LexiconImportViewModel {SourceLanguage = lexeme.Language}));
            }
            await base.OnActivateAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, context);
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
        }

        protected override void OnViewReady(object view)
        {
            base.OnViewReady(view);
        }


        public void OnToggleAllChecked(CheckBox? checkBox) 
        {
            //if (checkBox != null && BulkAlignments is { Count: > 0 })
            //{
            //    foreach (var bulkAlignment in BulkAlignments)
            //    {
            //        bulkAlignment.IsSelected = checkBox.IsChecked ?? false;
            //    }
            //}
        }
    }
}
