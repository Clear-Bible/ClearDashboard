//#define DEMO

using System.Threading.Tasks;
using Autofac;
using MediatR;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Linq;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;
using ClearDashboard.Wpf.Application.Messages.Lexicon;
using ClearDashboard.DataAccessLayer.Models;
using Lexicon = ClearDashboard.DAL.Alignment.Lexicon.Lexicon;
using Lexeme = ClearDashboard.DAL.Alignment.Lexicon.Lexeme;
using Translation = ClearDashboard.DAL.Alignment.Lexicon.Translation;
using System.Collections.ObjectModel;

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

        public static Lexicon GetExternalLexiconNotInInternal(Lexicon externalLexicon, Lexicon internalLexicon)
        {
            var lexemesExternalExceptInternal = externalLexicon.Lexemes
                .Where(el =>
                    !internalLexicon.Lexemes.Any(il => il.Lemma == el.Lemma) &&
                    !internalLexicon.Lexemes.Any(il => el.Forms.Select(e => e.Text).Contains(il.Lemma)) &&
                    !internalLexicon.Lexemes.Any(il =>
                        il.Meanings.SelectMany(m => m.Translations.Select(t => t.Text)).Intersect(
                        el.Meanings.SelectMany(m => m.Translations.Select(t => t.Text))).Any())
                    );

            return new Lexicon
            {
                Lexemes = new ObservableCollection<Lexeme>(lexemesExternalExceptInternal)
            };
        }

        public static void Save(Lexicon lexicon)
        {

        }

        /// <summary>
        /// Creates an <see cref="LexiconManager"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <returns>The constructed LexiconManager.</returns>
        public static async Task<LexiconManager> CreateAsync(IComponentContext componentContext)
        {
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
