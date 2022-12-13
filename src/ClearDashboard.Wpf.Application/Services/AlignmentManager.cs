using MediatR;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using ClearBible.Engine.Corpora;
using ClearDashboard.DAL.Alignment.Translation;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClearDashboard.Wpf.Application.Collections;
using Autofac;
using System.Diagnostics;
using System;

namespace ClearDashboard.Wpf.Application.Services
{
    /// <summary>
    /// A class that manages the alignments for a specified <see cref="EngineParallelTextRow"/> and <see cref="AlignmentSet"/>.
    /// </summary>
    public sealed class AlignmentManager : PropertyChangedBase
    {
        private EngineParallelTextRow ParallelTextRow { get; }
        private AlignmentSetId AlignmentSetId { get; }
        private AlignmentSet? AlignmentSet { get; set; }

        private IEventAggregator EventAggregator { get; }
        private ILogger<AlignmentManager> Logger { get; }
        private IMediator Mediator { get; }

        private async Task GetAlignmentSetAsync()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                AlignmentSet = await AlignmentSet.Get(AlignmentSetId, Mediator);

                stopwatch.Stop();
                Logger.LogInformation($"Retrieved alignment set {AlignmentSetId} in {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        private async Task GetAlignmentsAsync()
        {
            try
            {
                if (AlignmentSet != null)
                {
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    Alignments = new AlignmentCollection(await AlignmentSet.GetAlignments(new List<EngineParallelTextRow> { ParallelTextRow }));
                    
                    stopwatch.Stop();
                    Logger.LogInformation($"Retrieved alignments in {stopwatch.ElapsedMilliseconds} ms");
                }
                else
                {
                    Logger.LogCritical("Could not retrieve alignments without a valid alignment set.");
                }
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets the sequence of <see cref="Alignment"/>s for the specified row and alignment set.
        /// </summary>
        public AlignmentCollection? Alignments { get; private set; }

        /// <summary>
        /// Initializes the view model with the alignments for the row.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task InitializeAsync()
        {
            await GetAlignmentSetAsync();
            await GetAlignmentsAsync();
        }

        public AlignmentManager(EngineParallelTextRow parallelTextRow, 
                                AlignmentSetId alignmentSetId, 
                                IEventAggregator eventAggregator, 
                                ILogger<AlignmentManager> logger, 
                                IMediator mediator)
        {
            ParallelTextRow = parallelTextRow;
            AlignmentSetId = alignmentSetId;

            EventAggregator = eventAggregator;
            Logger = logger;
            Mediator = mediator;
        }

        /// <summary>
        /// Creates an <see cref="AlignmentManager"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> containing the tokens to align.</param>
        /// <param name="alignmentSetId">The ID of the alignment set to use for aligning the tokens.</param>
        /// <returns>The constructed AlignmentManager.</returns>
        public static async Task<AlignmentManager> CreateAsync(IComponentContext componentContext,
                                                                EngineParallelTextRow parallelTextRow, 
                                                                AlignmentSetId alignmentSetId)
        {
            var manager = componentContext.Resolve<AlignmentManager>(new NamedParameter("parallelTextRow", parallelTextRow),
                                                                    new NamedParameter("alignmentSetId", alignmentSetId));
            await manager.InitializeAsync();
            return manager;
        }
    }
}
