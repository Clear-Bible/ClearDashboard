using System.Threading.Tasks;
using Autofac;
using MediatR;
using Caliburn.Micro;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System;
using ClearDashboard.DAL.Alignment.Lexicon;
using ClearDashboard.Wpf.Application.Collections.Lexicon;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Lexicon;

namespace ClearDashboard.Wpf.Application.Services
{
    /// <summary>
    /// A class that manages the lexicons for a specified <see cref="EngineParallelTextRow"/> and <see cref="TranslationSet"/>.
    /// </summary>
    public sealed class LexiconManager : PropertyChangedBase
    {
        //private EngineParallelTextRow? _parallelTextRow;
        //private EngineParallelTextRow? ParallelTextRow
        //{
        //    get => _parallelTextRow;
        //    set
        //    {
        //        _parallelTextRow = value;
        //        if (_parallelTextRow is { SourceTokens: { } })
        //        {
        //            SourceTokenIds = _parallelTextRow.SourceTokens.Select(t => t.TokenId).ToList();
        //        }
        //    }
        //}
        //private List<TokenId> SourceTokenIds { get; set; } = new();

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
                var result = await lexeme.Create(Mediator);

                stopwatch.Stop();
                Logger.LogInformation($"Created lexeme for lemma {lemma} in {stopwatch.ElapsedMilliseconds} ms");

                return new LexemeViewModel(result);
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<LexemeViewModel?> GetLexemeAsync(string lemma = "", string? language = null, string? meaningLanguage = null)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await Lexeme.Get(Mediator, lemma, language, meaningLanguage);

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

        public async Task DeleteLexemeAsync(Lexeme lexeme)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await lexeme.Delete(Mediator);

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

                await lexeme.Entity.PutForm(Mediator, form);

                stopwatch.Stop();

                Logger.LogInformation($"Added form {form.Text} to lexeme for {lexeme.Lemma} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task DeleteLexemeFormAsync(Form form)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await form.Delete(Mediator);

                stopwatch.Stop();

                Logger.LogInformation($"Deleted form {form.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task AddMeaningAsync(LexemeViewModel lexeme, Meaning meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await lexeme.Entity.PutMeaning(Mediator, meaning);

                stopwatch.Stop();

                Logger.LogInformation($"Added meaning {meaning.Text} to lexeme for {lexeme.Lemma} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task DeleteMeaningAsync(Meaning meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                await meaning.Delete(Mediator);

                stopwatch.Stop();

                Logger.LogInformation($"Deleted meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task<SemanticDomainCollection> GetAllSemanticDomainsAsync(Meaning meaning)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var result = await SemanticDomain.GetAll(Mediator);

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

        public async Task<SemanticDomain> AddNewSemanticDomainAsync(Meaning meaning, string semanticDomainText)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                var result = await meaning.CreateAssociateSenanticDomain(Mediator, semanticDomainText);

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

        public async Task AddExistingSemanticDomainAsync(Meaning meaning, SemanticDomain semanticDomain)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                await meaning.AssociateSemanticDomain(Mediator, semanticDomain);

                stopwatch.Stop();
                Logger.LogInformation($"Associated semantic domains {semanticDomain.Text} to meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        public async Task RemoveSemanticDomainAsync(Meaning meaning, SemanticDomain semanticDomain)
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                await meaning.DetachSemanticDomain(Mediator, semanticDomain);

                stopwatch.Stop();
                Logger.LogInformation($"Detached semantic domains {semanticDomain.Text} from meaning {meaning.Text} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Initializes the manager with the lexicon data for the row.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task InitializeAsync()
        {
        }

        /// <summary>
        /// Creates an <see cref="LexiconManager"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> containing the tokens to align.</param>
        /// <returns>The constructed LexiconManager.</returns>
        public static async Task<LexiconManager> CreateAsync(IComponentContext componentContext)
//                                                             EngineParallelTextRow parallelTextRow)
        {
            var manager = componentContext.Resolve<LexiconManager>();
            //var manager = componentContext.Resolve<LexiconManager>(new NamedParameter("parallelTextRow", parallelTextRow));
            await manager.InitializeAsync();
            return manager;
        }

        //public LexiconManager(EngineParallelTextRow parallelTextRow,
        //                            IEventAggregator eventAggregator,
        //                            ILogger<LexiconManager> logger,
        //                            IMediator mediator)
        //{
        //    ParallelTextRow = parallelTextRow;

        //    EventAggregator = eventAggregator;
        //    Logger = logger;
        //    Mediator = mediator;
        //}
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
