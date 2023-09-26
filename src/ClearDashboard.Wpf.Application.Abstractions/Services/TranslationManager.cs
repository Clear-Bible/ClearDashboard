using System.Threading.Tasks;
using Autofac;
using MediatR;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using ClearDashboard.Wpf.Application.Collections;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using SIL.Extensions;
using Translation = ClearDashboard.DAL.Alignment.Translation.Translation;
using TranslationCollection = ClearDashboard.Wpf.Application.Collections.TranslationCollection;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;

namespace ClearDashboard.Wpf.Application.Services
{
    /// <summary>
    /// A class that manages the translations for a specified <see cref="EngineParallelTextRow"/> and <see cref="TranslationSet"/>.
    /// </summary>
    public sealed class TranslationManager : PropertyChangedBase
    {
        private List<TokenId> SourceTokenIds { get; }
        private TranslationSetId TranslationSetId { get; }
        private TranslationSet? TranslationSet { get; set; }
        private TranslationCollection? Translations { get; set; } 

        private IEventAggregator EventAggregator { get; }
        private ILogger<TranslationManager> Logger { get; }
        private IMediator Mediator { get; }

        public string? SourceLanguage => TranslationSet?.ParallelCorpusId.SourceTokenizedCorpusId?.CorpusId?.Language;
        public string? TargetLanguage => TranslationSet?.ParallelCorpusId.TargetTokenizedCorpusId?.CorpusId?.Language;

        private async Task GetTranslationSetAsync()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                TranslationSet = await TranslationSet.Get(TranslationSetId, Mediator);

                stopwatch.Stop();
                Logger.LogInformation($"Retrieved translation set {TranslationSetId.DisplayName} ({TranslationSetId.Id}) in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task GetTranslationsAsync()
        {
            try
            {
                if (TranslationSet != null)
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Translations = new TranslationCollection(await TranslationSet.GetTranslations(SourceTokenIds));

                    stopwatch.Stop();
                    Logger.LogInformation($"Retrieved {Translations.Count} translations from translation set {TranslationSetId.DisplayName} ({TranslationSetId.Id}) in {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger.LogCritical("Could not retrieve translations without a valid translation set.");
                }
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Adds a translation for a token to the current set of translations.
        /// </summary>
        /// <param name="tokenId">The token ID to add.</param>
        /// <returns>The translation for the additional token.</returns>
        public async Task<Translation?> AddTranslationAsync(TokenId tokenId)
        {
            try
            {
                if (TranslationSet != null)
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var newTranslations = (await TranslationSet.GetTranslations(tokenId.ToEnumerable())).ToList();
                    if (Translations != null)
                    {
                        Translations.AddRange(newTranslations);
                    }
                    else
                    {
                        Translations = new TranslationCollection(newTranslations);
                    }

                    stopwatch.Stop();
                    Logger.LogInformation($"Retrieved {newTranslations.Count()} translations from translation set {TranslationSetId.DisplayName} ({TranslationSetId.Id}) in {stopwatch.ElapsedMilliseconds} ms");

                    return newTranslations.FirstOrDefault();
                }
                else
                {
                    Logger.LogCritical("Could not retrieve translations without a valid translation set.");
                }
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
            return null;
        }

        /// <summary>
        /// Retrieves the translation for a specified token.
        /// </summary>
        /// <remarks>
        /// If <see paramref="returnPlaceholder"/> is true, then if a token is not found in the translation collection, a default translation is returned with a placeholder text.
        /// </remarks>
        /// <param name="token">The <see cref="Token"/> for which to obtain a translation.</param>
        /// <param name="returnPlaceholder">If true, then if a translation is not found then return a default placeholder; otherwise, return null.</param>
        /// <returns>The translation for the token if found; otherwise, a default placeholder or null, depending on the value of the <paramref name="returnPlaceholder"/> parameter.</returns>
        private Translation? GetTranslationForToken(Token token, bool returnPlaceholder = true)
        {
            var translation = Translations?.FirstOrDefault(t => t.SourceToken.TokenId.Id == token.TokenId.Id);
            if (translation != null && !String.IsNullOrWhiteSpace(translation.TargetTranslationText))
            {
                return translation;
            }

            return (returnPlaceholder ? new Translation(token) : null);
        }

        /// <summary>
        /// Retrieves the translation for a specified token, optionally within a parent <see cref="CompositeToken"/>.
        /// </summary>
        /// <remarks>
        /// If the token is part of a composite token, then this return the parent's translation for the first child token and null for other tokens.
        ///
        /// If <see paramref="returnPlaceholder"/> is true, then if a token is not found in the translation collection, a default translation is returned with a placeholder text.
        /// </remarks>
        /// <param name="token">The <see cref="Token"/> for which to obtain a translation.</param>
        /// <param name="compositeToken">An optional parent <see cref="CompositeToken"/> that the token is part of.</param>
        /// <param name="returnPlaceholder">If true, then if a translation is not found then return a default placeholder; otherwise, return null.</param>
        /// <returns>The translation for the token if found; otherwise, a default placeholder or null, depending on whether the token is part of a CompositeToken.</returns>
        public Translation? GetTranslationForToken(Token token, CompositeToken? compositeToken, bool returnPlaceholder = true)
        {
            return compositeToken != null
                ? new TokenCollection(compositeToken.Tokens).IsFirst(token) ? GetTranslationForToken(compositeToken) : null
                : GetTranslationForToken(token, returnPlaceholder);
        }

        /// <summary>
        /// Gets a collection of <see cref="TranslationOption"/>s for a given translation.
        /// </summary>
        /// <param name="token">The <see cref="Token"/> for which to provide translation options.</param>
        /// <param name="count">The number of options to include in the resulting collection.</param>
        /// <returns>An awaitable <see cref="Task{T}"/> containing a <see cref="TranslationOptionCollection"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the manager does not contain a valid translation set.</exception>
        public async Task<TranslationOptionCollection> GetTranslationOptionsAsync(Token token, int count = 4)
        {
            try
            {
                if (TranslationSet == null) throw new InvalidOperationException("Cannot retrieve translation options for token '{token.SurfaceText}' ({token.TokenId.Id}) without a valid translation set.");
                
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var translationModelEntry = await TranslationSet.GetTranslationModelEntryForToken(token);
                if (translationModelEntry == null)
                {
                    stopwatch.Stop();
                    Logger.LogInformation($"No translation options found for token '{token.SurfaceText}' ({token.TokenId.Id}) in {stopwatch.ElapsedMilliseconds} ms");
                    return new TranslationOptionCollection();
                }

                stopwatch.Stop();
                Logger.LogInformation($"Retrieved translation options for token '{token.SurfaceText}' ({token.TokenId.Id}) in {stopwatch.ElapsedMilliseconds} ms");

                return new TranslationOptionCollection(translationModelEntry.OrderByDescending(option => option.Value)
                    .Select(option => new TranslationOption { Word = option.Key, Count = option.Value })
                    .Take(count));
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Saves a selected translation for a token to the database.
        /// </summary>
        /// <param name="translation">The <see cref="Translation"/> to save to the database.</param>
        /// <param name="translationActionType">A <see cref="TranslationActionTypes"/> value indicating whether the translation should propagate to other tokens.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the manager does not contain a valid translation set.</exception>
        public async Task PutTranslationAsync(Translation translation, string translationActionType)
        {
            try
            {
                if (TranslationSet == null) throw new InvalidOperationException("Cannot save translation without a valid translation set.");

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await TranslationSet.PutTranslation(translation, translationActionType);
                stopwatch.Stop();
                
                Logger.LogInformation($"Saved translation for {translation.SourceTokenSurfaceText} in {stopwatch.ElapsedMilliseconds} ms");

                if (!SourceTokenIds.Contains(translation.SourceToken.TokenId))
                {
                    SourceTokenIds.Add(translation.SourceToken.TokenId);
                }

                await GetTranslationsAsync();
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Initializes the manager with the translations for the row.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task InitializeAsync()
        {
            await GetTranslationSetAsync();
            await GetTranslationsAsync();
        }

        /// <summary>
        /// Creates an <see cref="TranslationManager"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="sourceTokenIds">The <see cref="TokenId"/>s of the tokens to gloss.</param>
        /// <param name="translationSetId">The ID of the translation set to use for the token translations.</param>
        /// <returns>The constructed AlignmentManager.</returns>
        public static async Task<TranslationManager> CreateAsync(IComponentContext componentContext,
                                                                 List<TokenId> sourceTokenIds,
                                                                 TranslationSetId translationSetId)
        {
            var manager = componentContext.Resolve<TranslationManager>(new NamedParameter("sourceTokenIds", sourceTokenIds),
                                                                        new NamedParameter("translationSetId", translationSetId));
            await manager.InitializeAsync();
            return manager;
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <remarks>
        /// This is for use by the DI container; use <see cref="CreateAsync"/> instead to create and initialize an instance.
        /// </remarks>
        public TranslationManager(List<TokenId> sourceTokenIds,
                                    TranslationSetId translationSetId,
                                    IEventAggregator eventAggregator,
                                    ILogger<TranslationManager> logger,
                                    IMediator mediator)
        {
            SourceTokenIds = sourceTokenIds;
            TranslationSetId = translationSetId;

            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
        }
    }
}
