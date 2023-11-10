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
using System.Linq;
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
            var result = await Mediator.Send(new GetExternalLexiconQuery(projectId));
            if (result.Success && result.HasData)
            {
                return result.Data;
            }
            else
            {
                Logger.LogError($"An unexpected error occurred while getting the lexicon for the Paratext project with id '{projectId}'");
                return null;
            }
            
        }


        public List<LexiconImportViewModel> LexiconImportViewModels { get; private set; }

        public ObservableCollection<Lexeme>? ManagedLexemes { get; private set; }

        public (IEnumerable<Lexeme> Lexemes, IEnumerable<Guid> LemmaOrFormMatchesOnly, IEnumerable<Guid> TranslationMatchesOnly) ManagedLexicon { get; private set; }

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
            LexiconImportViewModels = new List<LexiconImportViewModel>();
            var externalLexicon = await GetLexiconForProject(projectId);
            if (externalLexicon != null)
            {
                ManagedLexicon = await GetExternalLexiconNotInInternal(externalLexicon, cancellationToken);
                ManagedLexemes = new ObservableCollection<Lexeme>(ManagedLexicon.Lexemes);

                foreach (var lexeme in ManagedLexemes)
                {
                   
                    foreach (var meaning in lexeme.Meanings)
                    {
                        //TODO:  fix me!
                        var lemmaOrFormMatch = true; //ManagedLexicon.LemmaOrFormMatchesOnly.Contains(lexeme.LexemeId.Id);
                        var translationMatch = true; // ManagedLexicon.TranslationMatchesOnly.Contains(lexeme.LexemeId.Id);

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
                                IsSelected = !lemmaOrFormMatch && !translationMatch,
                                ShowAddAsFormButton = lemmaOrFormMatch,
                                ShowAddTargetAsTranslationButton = translationMatch
                            };
                            LexiconImportViewModels.Add(vm);
                        }
                    }
                }
            }

            return LexiconImportViewModels;
        }

        private async Task<(IEnumerable<Lexeme> Lexemes, IEnumerable<Guid> LemmaOrFormMatchesOnly, IEnumerable<Guid> TranslationMatchesOnly)> GetExternalLexiconNotInInternal(Lexicon externalLexicon, CancellationToken cancellationToken)
        {
            var internalLexicon = await Lexicon.GetInternalLexicon(Mediator, cancellationToken);
            return GetExternalLexiconNotInInternal(externalLexicon, internalLexicon);
        }

        private static (IEnumerable<Lexeme> Lexemes, IEnumerable<Guid> LemmaOrFormMatchesOnly, IEnumerable<Guid> TranslationMatchesOnly) GetExternalLexiconNotInInternal(Lexicon externalLexicon, Lexicon internalLexicon)
        {
            // Include each external Lexeme for which there are not any internal lexemes for which:
            //  Any of the external lemma/forms match any of the internal lemma/forms AND
            //  Any of the external translation texts match any of the internal translation texts

            //var lexemesExternalExceptInternal = externalLexicon.Lexemes
            //    .Where(el =>
            //        !internalLexicon.Lexemes.Any(il =>
            //            (il.Lemma == el.Lemma || 
            //             el.Forms.Select(e => e.Text).Contains(il.Lemma)) &&
            //             il.Meanings.SelectMany(m => m.Translations.Select(t => t.Text)).Intersect(
            //             el.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))).Any()));

            //var lexemesExternalExceptInternal = externalLexicon.Lexemes
            //    .Where(el =>
            //        !internalLexicon.Lexemes.Any(il => 
            //            il.LemmaPlusFormTexts.Intersect(el.LemmaPlusFormTexts).Any() &&
            //            il.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))
            //                .Intersect(
            //            el.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))
            //                ).Any()
            //        ));

            return externalLexicon.Lexemes
                .ExceptByLexemeTranslationMatch(internalLexicon.Lexemes);

           
        }

        public async Task ProcessLexiconToImport()
        {
            if (ManagedLexemes != null)
            {

                var selectedLexemesToImportIds = LexiconImportViewModels.Where(vm => vm.IsSelected).Select(vm=>vm.LexemeId).Distinct(); 
                var selectedLexemesToImport = ManagedLexemes.IntersectBy(selectedLexemesToImportIds, l => l.LexemeId.Id);
                var lexicon = new Lexicon
                {
                    Lexemes = new ObservableCollection<Lexeme>(selectedLexemesToImport)
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
