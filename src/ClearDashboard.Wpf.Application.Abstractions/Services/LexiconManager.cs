//#define DEMO

using Autofac;
using Caliburn.Micro;
using ClearDashboard.DAL.Alignment.Features.Lexicon;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.Messages.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lexeme = ClearDashboard.DAL.Alignment.Lexicon.Lexeme;
using Lexicon = ClearDashboard.DAL.Alignment.Lexicon.Lexicon;
using Translation = ClearDashboard.DAL.Alignment.Lexicon.Translation;

namespace ClearDashboard.Wpf.Application.Services
{
    /// <summary>
    /// A class that bridges the lexicon API with the user interface.
    /// </summary>
    public sealed class LexiconManager : PropertyChangedBase
    {
        private Lexicon _internalLexicon;
        private IEventAggregator EventAggregator { get; }
        private ILogger<LexiconManager> Logger { get; }
        private IMediator Mediator { get; }

        public async Task<LexemeViewModel> CreateLexemeAsync(string lemma = "", string? language = null, string? type = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var lexeme = new Lexeme
                {
                    Lemma = lemma,
                    Language = language,
                    Type = type
                };
#if !DEMO
                var result = await lexeme.Create(Mediator);
#else
                var result = lexeme;
#endif
                stopwatch.Stop();
                Logger.LogInformation($"Created lexeme for lemma {lemma} in {stopwatch.ElapsedMilliseconds} ms");

                var resultViewModel = new LexemeViewModel(result);
                await EventAggregator.PublishOnUIThreadAsync(new LexemeAddedMessage(resultViewModel));

                return resultViewModel;
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        [Obsolete("This method is deprecated; use GetLexemesAsync instead.")]
        public async Task<LexemeViewModel?> GetLexemeAsync(string lemma = "", string? language = null, string? meaningLanguage = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                var result = await Lexeme.Get(Mediator, lemma, language, meaningLanguage);
#else
                var result = new Lexeme { Lemma = lemma, Language = language };
#endif
                stopwatch.Stop();

                if (result == null)
                {
                    Logger.LogInformation($"Could not find lexeme for {lemma} in {stopwatch.ElapsedMilliseconds} ms");
                    return null;
                }
                Logger.LogInformation($"Retrieved lexeme for {lemma} in {stopwatch.ElapsedMilliseconds} ms");

                return new LexemeViewModel(result);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<LexemeViewModelCollection> GetLexemesAsync(string lemma = "", string? language = null, string? meaningLanguage = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                var results = (await Lexeme.GetByLemmaOrForm(Mediator, lemma, language, meaningLanguage)).ToList();
#else
                var results = new List<Lexeme>(new Lexeme { Lemma = lemma, Language = language });
#endif
                stopwatch.Stop();

                Logger.LogInformation(results.Any() ? $"Retrieved {results.Count} lexeme(s) for {lemma} in {stopwatch.ElapsedMilliseconds} ms"
                                                    : $"Could not find lexeme for {lemma} in {stopwatch.ElapsedMilliseconds} ms");
                return new LexemeViewModelCollection(results);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public bool LexemeExists(string lemma = "", string? language = null, string? meaningLanguage = null)
        {
            var result = Task.Run(() => LexemeExistsAsync(lemma, language, meaningLanguage)).GetAwaiter().GetResult();
            return result;
        }

        public async Task<bool> LexemeExistsAsync(string lemma = "", string? language = null, string? meaningLanguage = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                var results = (await Lexeme.GetByLemmaOrForm(Mediator, lemma, language, meaningLanguage)).ToList();
#else
                var results = new List<Lexeme>(new Lexeme { Lemma = lemma, Language = language });
#endif
                stopwatch.Stop();

                Logger.LogInformation(results.Any() ? $"Retrieved {results.Count} lexeme(s) for {lemma} in {stopwatch.ElapsedMilliseconds} ms"
                                                    : $"Could not find lexeme for {lemma} in {stopwatch.ElapsedMilliseconds} ms");

                return results.Any(l => l.Lemma == lemma);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task DeleteLexemeAsync(LexemeViewModel lexeme)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await lexeme.Entity.Delete(Mediator);
#endif
                stopwatch.Stop();

                Logger.LogInformation($"Deleted lexeme for {lexeme.Lemma} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task AddLexemeFormAsync(LexemeViewModel lexeme, Form form)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await lexeme.Entity.PutForm(Mediator, form);
#endif
                stopwatch.Stop();

                Logger.LogInformation($"Added form {form.Text} to lexeme for {lexeme.Lemma} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task DeleteLexemeFormAsync(LexemeViewModel lexeme, Form form)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await lexeme.Entity.DeleteForm(Mediator, form);
#endif
                stopwatch.Stop();

                Logger.LogInformation($"Deleted form {form.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task AddMeaningAsync(LexemeViewModel lexeme, MeaningViewModel meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await lexeme.Entity.PutMeaning(Mediator, meaning.Entity);
#endif
                stopwatch.Stop();

                Logger.LogInformation($"Added meaning {meaning.Text} to lexeme for {lexeme.Lemma} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public static void AddMeaningDeferred(LexemeViewModel lexeme, MeaningViewModel meaning)
        {
            lexeme.Meanings.Add(meaning);
        }

        public async Task UpdateMeaningAsync(LexemeViewModel lexeme, MeaningViewModel meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await lexeme.Entity.PutMeaning(Mediator, meaning.Entity);
#endif
                stopwatch.Stop();

                Logger.LogInformation($"Updated meaning {meaning.Text} to lexeme for {lexeme.Lemma} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task DeleteMeaningAsync(LexemeViewModel lexeme, MeaningViewModel meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await lexeme.Entity.DeleteMeaning(Mediator, meaning.Entity);
#endif
                stopwatch.Stop();

                Logger.LogInformation($"Deleted meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<SemanticDomainCollection> GetAllSemanticDomainsAsync()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                var result = await SemanticDomain.GetAll(Mediator);
#else
                var result = new SemanticDomainCollection();
#endif
                stopwatch.Stop();
                Logger.LogInformation($"Retrieved semantic domains in {stopwatch.ElapsedMilliseconds} ms");

                return new SemanticDomainCollection(result);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<SemanticDomain> AddNewSemanticDomainAsync(MeaningViewModel meaning, string semanticDomainText)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                var result = await meaning.Entity.CreateAssociateSenanticDomain(Mediator, semanticDomainText);
#else
                var result = new SemanticDomain { Text = semanticDomainText };
                meaning.SemanticDomains.Add(result);
#endif
                stopwatch.Stop();
                Logger.LogInformation($"Added semantic domain {semanticDomainText} to meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");

                return result;
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task AddExistingSemanticDomainAsync(MeaningViewModel meaning, SemanticDomain semanticDomain)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await meaning.Entity.AssociateSemanticDomain(Mediator, semanticDomain);
#endif
                stopwatch.Stop();
                Logger.LogInformation($"Associated semantic domains {semanticDomain.Text} to meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task RemoveSemanticDomainAsync(MeaningViewModel meaning, SemanticDomain semanticDomain)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#if !DEMO
                await meaning.Entity.DetachSemanticDomain(Mediator, semanticDomain);
#endif
                stopwatch.Stop();
                Logger.LogInformation($"Detached semantic domains {semanticDomain.Text} from meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task AddTranslationAsync(LexiconTranslationViewModel translation, MeaningViewModel meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

#if !DEMO
                await meaning.Entity.PutTranslation(Mediator, translation.Entity);
#endif
                var newTranslation = new LexiconTranslationViewModel(translation.Entity)
                {
                    Count = translation.Count,
                    Meaning = meaning,
                    IsSelected = translation.IsSelected
                };
                await EventAggregator.PublishOnUIThreadAsync(new LexiconTranslationAddedMessage(newTranslation, meaning));

                stopwatch.Stop();
                Logger.LogInformation($"Added translation {translation.Text} to meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task MoveTranslationAsync(MeaningViewModel targetMeaning, LexiconTranslationViewModel sourceTranslation)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var sourceMeaning = sourceTranslation.Meaning;
                if (sourceMeaning != null)
                {
#if !DEMO
                    await sourceMeaning.Entity.DeleteTranslation(Mediator, sourceTranslation.Entity);
#endif
                }

                var targetTranslationEntity = new Translation { Text = sourceTranslation.Text };
#if !DEMO
                await targetMeaning.Entity.PutTranslation(Mediator, targetTranslationEntity);
#endif
                var targetTranslation = new LexiconTranslationViewModel(targetTranslationEntity)
                {
                    Count = sourceTranslation.Count,
                    Meaning = targetMeaning,
                    IsSelected = sourceTranslation.IsSelected
                };
                await EventAggregator.PublishOnUIThreadAsync(new LexiconTranslationMovedMessage(
                    sourceTranslation,
                    sourceMeaning,
                    targetTranslation,
                    targetMeaning));

                stopwatch.Stop();
                Logger.LogInformation($"Moved translation {sourceTranslation.Text} to meaning {targetMeaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task DeleteTranslationAsync(MeaningViewModel meaning, LexiconTranslationViewModel translation)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await meaning.Entity.DeleteTranslation(Mediator, translation.Entity);

                stopwatch.Stop();

                Logger.LogInformation($"Deleted translation {translation.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }
        
        private async Task<Lexicon?> GetLexiconForProject(string? projectId)
        {

            var data = await TryLoadExternalLexiconFromTempFile<Lexicon?>();
            if (data != null)
            {
                return data;
            }

            var result = await Mediator.Send(new GetExternalLexiconQuery(projectId));
            if (result is { Success: true, HasData: true })
            {

                await SaveExternalLexiconToTempFile(result.Data);

                return result.Data;
            }
            else
            {
                Logger.LogError($"An unexpected error occurred while getting the lexicon for the Paratext project with id '{projectId}'");
                return null;
            }

        }

        private async Task SaveExternalLexiconToTempFile(Lexicon resultData, string fileName = "externalLexicon.json")
        {
            var path = Path.Combine(Path.GetTempPath(), fileName);
            if (File.Exists(path))
            {
                return;
            }

            Logger.LogInformation($"Saving temporary external lexicon data file: '{path}'.");
            var json = JsonSerializer.Serialize(resultData);

            await System.IO.File.WriteAllTextAsync(path, json);
        }

        public async Task DeleteTemporaryExternalLexiconFile(string fileName = "externalLexicon.json")
        {
            var path = Path.Combine(Path.GetTempPath(), fileName);
            if (File.Exists(path))
            { Logger.LogInformation($"Deleting temporary external lexicon data file '{path}'.");
               File.Delete(path);
            }

            await Task.CompletedTask;
        }

        private async Task<T?> TryLoadExternalLexiconFromTempFile<T>(string fileName = "externalLexicon.json")
        {
            var path = Path.Combine(Path.GetTempPath(), fileName);
            if (!File.Exists(path))
            {
                return default;
            }
            Logger.LogInformation($"Loading temporary external lexicon data file: '{path}'.");
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json);
        }


        public List<LexiconImportViewModel> LexiconImportViewModels { get; private set; }

        public ObservableCollection<Lexeme>? ManagedLexemes { get; private set; }

        public (Lexicon Lexicon, IEnumerable<Guid> TranslationMatchTranslationIds, IEnumerable<Guid> LemmaOrFormMatchTranslationIds) ManagedLexicon { get; private set; }

        public List<LexiconImportViewModel> ImportedLexiconViewModels { get; private set; }

        /// <summary>
        /// This method is used purely for a sanity check on the data imported into the Project database,
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<LexiconImportViewModel>> GetImportedLexiconViewModels(string? projectId, CancellationToken cancellationToken)
        {
            ImportedLexiconViewModels = new List<LexiconImportViewModel>();
            var internalLexicon = await Lexicon.GetInternalLexicon(Mediator, cancellationToken);

            foreach (var lexeme in internalLexicon.Lexemes)
            {
                foreach (var meaning in lexeme.Meanings)
                {
                    foreach (var translation in meaning.Translations)
                    {
                        var vm = new LexiconImportViewModel
                        {
                            LexemeId = lexeme.LexemeId.Id,
                            SourceLanguage = lexeme.Language,
                            SourceWord = lexeme.Lemma,
                            SourceType = lexeme.Type,
                            TargetLanguage = meaning.Language,
                            TargetWord = translation.Text,
                        };
                        ImportedLexiconViewModels.Add(vm);
                    }
                }
            }
            return ImportedLexiconViewModels;
        }

        public async Task<List<LexiconImportViewModel>> GetLexiconImportViewModels(string? projectId, CancellationToken cancellationToken)
        {
            try
            {
                LexiconImportViewModels = new List<LexiconImportViewModel>();
                var externalLexicon = await GetLexiconForProject(projectId);
                if (externalLexicon != null)
                {
                    ManagedLexicon = await GetExternalLexiconMergedIntoInternal(externalLexicon, cancellationToken);
                    ManagedLexemes = new ObservableCollection<Lexeme>(ManagedLexicon.Lexicon.Lexemes);

                    foreach (var lexeme in ManagedLexemes)
                    {
                        foreach (var meaning in lexeme.Meanings)
                        {
                            foreach (var translation in meaning.Translations.Where(e => !e.IsInDatabase))
                            {
                                var translationMatch =
                                    ManagedLexicon.TranslationMatchTranslationIds.Contains(translation.TranslationId
                                        .Id);
                                var lemmaOrFormMatch =
                                    ManagedLexicon.LemmaOrFormMatchTranslationIds.Contains(translation.TranslationId
                                        .Id);

                                var vm = new LexiconImportViewModel
                                {
                                    LexemeId = lexeme.LexemeId.Id,
                                    MeaningId = meaning.MeaningId.Id,
                                    TranslationId = translation.TranslationId.Id,
                                    SourceLanguage = lexeme.Language,
                                    SourceWord = lexeme.Lemma,
                                    SourceType = lexeme.Type,
                                    TargetLanguage = meaning.Language,
                                    TargetWord = translation.Text,
                                    IsSelected = !translationMatch && !lemmaOrFormMatch,
                                    ShowAddAsFormButton = translationMatch,
                                    ShowAddTargetAsTranslationButton = lemmaOrFormMatch
                                };
                                LexiconImportViewModels.Add(vm);
                            }
                        }
                    }

                    //// Some Lexeme "filter" examples:
                    //var eg1 = ManagedLexemes.FilterByLexemeAndTranslationText(
                    //    "wur",
                    //    true,
                    //    "sur",
                    //    "Word",
                    //    "en",
                    //    "him").ToList();
                    //var eg2 = ManagedLexemes.FilterByLexemeText("ɗi", false, null, null).ToList();
                    //var eg3 = ManagedLexemes
                    //    .FilterByLexemeText(new string[] { "kɨ̀", "ɗiihai", "mishkagham" }, false, null, null).ToList();
                    //var eg4 = ManagedLexemes
                    //    .FilterByLexemeText(new string[] { "kɨ̀", "ɗii", "wuri" }, true, null, "Word").ToList();
                    //var eg5 = ManagedLexemes.FilterByTranslationText("sur", "en", "another").ToList();

                    //// 
                    //var eg6 = ManagedLexemes
                    //    .Where(LexiconExtensions.LexemeMatchPredicate("sur", "Word", null))
                    //    .SelectMany(l => l.Meanings
                    //        .Where(LexiconExtensions.MeaningMatchPredicate(null, "en", null))
                    //        .SelectMany(m =>
                    //            m.Translations.Where(LexiconExtensions.TranslationMatchPredicate("him", true))))
                    //    .ToList();
                }

                // AnyPartialMatch example:
                //var matchStrings = new string[] { "lemma1", "form1", "form2" };
                //lexemes.Where(e => matchStrings.AnyPartialMatch(e.LemmaPlusFormTexts)).Select(e => ...);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                throw;
            }

            return LexiconImportViewModels;
        }

        public Lexicon InternalLexicon
        {
            get => _internalLexicon;
            private set => Set(ref _internalLexicon, value);
        }

        private async Task<(Lexicon Lexicon, IEnumerable<Guid> TranslationMatchTranslationIds, IEnumerable<Guid> LemmaOrFormMatchTranslationIds)> GetExternalLexiconMergedIntoInternal(Lexicon externalLexicon, CancellationToken cancellationToken)
        {
            InternalLexicon = await Lexicon.GetInternalLexicon(Mediator, cancellationToken);
            return GetExternalLexiconMergedIntoInternal(externalLexicon, InternalLexicon);
        }

        private static (Lexicon Lexicon, IEnumerable<Guid> TranslationMatchTranslationIds, IEnumerable<Guid> LemmaOrFormMatchTranslationIds) GetExternalLexiconMergedIntoInternal(Lexicon externalLexicon, Lexicon internalLexicon)
        {
            var mergedLexicon = Lexicon.MergeFirstIntoSecond(externalLexicon, internalLexicon);

            var translationMatchTranslationIds = mergedLexicon.Lexemes.GetTranslationMatchTranslationIds();
            var lemmaOrFormMatchTranslationIds = mergedLexicon.Lexemes.GetLemmaOrFormMatchTranslationIds();

            return (mergedLexicon, translationMatchTranslationIds, lemmaOrFormMatchTranslationIds);
        }

        public async Task ProcessLexiconToImport()
        {
            if (ManagedLexemes != null)
            {
                var selectedLexemesToImportIds = LexiconImportViewModels
                    .Where(vm => vm.IsSelected)
                    .Select(vm => vm.LexemeId)
                    .Distinct();

                var selectedLexemesToImport = ManagedLexemes
                    .IntersectBy(selectedLexemesToImportIds, l => l.LexemeId.Id)
                    .ToDictionary(l => l.LexemeId.Id, l => l);

                LexiconImportViewModels
                    .Where(vm => !vm.IsSelected)
                    .Where(vm => selectedLexemesToImport.ContainsKey(vm.LexemeId))
                    .ToList()
                    .ForEach(vm =>
                    {
                        var lexeme = selectedLexemesToImport[vm.LexemeId];
                        lexeme.Meanings
                            .Where(m => m.Language == vm.TargetLanguage)
                            .SelectMany(m => m.Translations
                                .Where(t => t.Text == vm.TargetWord))
                            .ToList()
                            .ForEach(t =>
                            {
                                t.ExcludeFromSave = true;
                            });
                    });

                var lexicon = new Lexicon
                {
                    Lexemes = new ObservableCollection<Lexeme>(selectedLexemesToImport.Values)
                };

                await lexicon.SaveAsync(Mediator);
            }

        }


        /// <summary>
        /// Creates an <see cref="LexiconManager"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <returns>The constructed LexiconManager.</returns>
        public static async Task<LexiconManager> CreateAsync(IComponentContext componentContext)
        {
            await Task.CompletedTask;
            var manager = componentContext.Resolve<LexiconManager>();
            return manager;
        }

        public LexiconManager(IEventAggregator eventAggregator,
                              ILogger<LexiconManager> logger,
                              IMediator mediator)
        {
            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;

        }
    }
}
