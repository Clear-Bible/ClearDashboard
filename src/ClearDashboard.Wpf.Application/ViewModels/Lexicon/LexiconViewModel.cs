using Autofac;
using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Panes;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClearDashboard.Wpf.Application.ViewModels.Lexicon
{
    public class LexiconViewModel : ToolViewModel
    {

        private LexiconManager LexiconManager { get; }

        private Visibility _progressBarVisibility = Visibility.Hidden;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => Set(ref _progressBarVisibility, value);
        }
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
            await Task.Run(async () =>
            {
                ProgressBarVisibility = Visibility.Visible;
                var list = new List<LexiconImportViewModel>();
                var stopWatch = Stopwatch.StartNew();
                try
                {
                   
                    var projectLexicon = await LexiconManager.GetLexiconForProject(null);
                    if (projectLexicon != null)
                    {
                        var externalLexicon = await LexiconManager.GetExternalLexiconNotInInternal(projectLexicon, cancellationToken);

                        //var intersectedLexemes =
                        //    externalLexicon.Lexemes.IntersectIdsByLexemeTranslationMatch(projectLexicon.Lexemes);

                        foreach (var lexeme in projectLexicon.Lexemes)
                        {
                            var externalLexeme =
                                externalLexicon.Lexemes.FirstOrDefault(l => l.LexemeId.Id == lexeme.LexemeId.Id);
                            var showAddTargetAsTranslationButton = externalLexeme != null &&
                                                                  lexeme.Forms.Any(f => f.Text == externalLexeme.Lemma);

                            foreach (var meaning in lexeme.Meanings)
                            {
                                foreach (var translation in meaning.Translations)
                                {
                                    var vm = new LexiconImportViewModel
                                    {
                                        SourceLanguage = lexeme.Language,
                                        SourceWord = lexeme.Lemma,
                                        SourceType = lexeme.Type,
                                        TargetLanguage = meaning.Language,
                                        TargetWord = translation.Text,
                                        ShowAddAsFormButton = externalLexeme != null && externalLexeme.Meanings.ToImmutableArray().Any(m => m.Translations.Contains(translation)),
                                        ShowAddTargetAsTranslationButton = showAddTargetAsTranslationButton
                                    };
                                    list.Add(vm);
                                }
                            }
                        }
                    }
                    Execute.OnUIThread(() =>
                    {
                        LexiconImports.AddRange(list);
                        ProgressBarVisibility = Visibility.Hidden;
                    });
                }
                finally
                {
                    stopWatch.Stop();
                    Logger!.LogInformation($"Loaded lexicon data in {stopWatch.ElapsedMilliseconds} milliseconds.");
                }
            });
            await base.OnActivateAsync(cancellationToken);
        }


        //protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        //{

        //    await Task.Run(async () =>
        //    {
        //        var stopWatch = Stopwatch.StartNew();
        //        try
        //        {
        //            var projectLexicon = await LexiconManager.GetLexiconForProject(null);
        //            if (projectLexicon != null)
        //            {
        //                var externalLexicon = await LexiconManager.GetExternalLexiconNotInInternal(projectLexicon, cancellationToken);

        //                //var intersectedLexemes =
        //                //    externalLexicon.Lexemes.IntersectIdsByLexemeTranslationMatch(projectLexicon.Lexemes);

        //                foreach (var vm in from lexeme in projectLexicon.Lexemes.ToImmutableArray()
        //                                       //let externalLexeme =
        //                                       //    externalLexicon.Lexemes.FirstOrDefault(l => l.LexemeId.Id == lexeme.LexemeId.Id)
        //                                       //let showAddTargetAsTranslationButton = externalLexeme != null &&
        //                                       //                                       lexeme.Forms.Any(f =>
        //                                       //                                           f.Text == externalLexeme.Lemma)
        //                                   from vm in lexeme.Meanings.ToImmutableArray()
        //                                       .SelectMany(meaning => meaning.Translations.ToImmutableArray().Select(
        //                                           translation => new LexiconImportViewModel
        //                                           {
        //                                               SourceLanguage = lexeme.Language,
        //                                               SourceWord = lexeme.Lemma,
        //                                               SourceType = lexeme.Type,
        //                                               TargetLanguage = meaning.Language,
        //                                               TargetWord = translation.Text,
        //                                               //ShowAddAsFormButton = externalLexeme != null && externalLexeme.Meanings
        //                                               //    .ToImmutableArray().Any(m => m.Translations.Contains(translation)),
        //                                               //ShowAddTargetAsTranslationButton = showAddTargetAsTranslationButton
        //                                           }))
        //                                   select vm)
        //                {
        //                    Execute.OnUIThread(() => LexiconImports.Add(vm));
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            stopWatch.Stop();
        //            Logger.LogInformation($"Loaded lexicon data in {stopWatch.ElapsedMilliseconds} milliseconds.");
        //        }


        //    });


        //    await base.OnActivateAsync(cancellationToken);
        //}

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
            if (checkBox != null && LexiconImports is { Count: > 0 })
            {
                foreach (var lexicon in LexiconImports)
                {
                    lexicon.IsSelected = checkBox.IsChecked ?? false;
                }
            }
        }
    }
}
