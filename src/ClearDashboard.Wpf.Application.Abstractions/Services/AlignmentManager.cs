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
using System.Linq;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView;
using ClearDashboard.Wpf.Application.ViewModels.EnhancedView.Messages;
using ClearDashboard.Wpf.Application.ViewModels.Popups;

namespace ClearDashboard.Wpf.Application.Services
{
    /// <summary>
    /// A class that manages the alignments for a specified <see cref="EngineParallelTextRow"/> and <see cref="AlignmentSet"/>.
    /// </summary>
    public sealed class AlignmentManager : PropertyChangedBase
    {
        private List<EngineParallelTextRow> ParallelTextRows { get; }
        private AlignmentSetId AlignmentSetId { get; }
        private AlignmentSet? AlignmentSet { get; set; }

        private IEventAggregator EventAggregator { get; }
        private ILogger<AlignmentManager> Logger { get; }
        private IMediator Mediator { get; }
        private ILifetimeScope LifetimeScope { get; }
        private IWindowManager WindowManager { get; }
        private SelectionManager SelectionManager { get; }

        private async Task GetAlignmentSetAsync()
        {
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                AlignmentSet = await AlignmentSet.Get(AlignmentSetId, Mediator);

                stopwatch.Stop();
                Logger.LogInformation($"Retrieved alignment set {AlignmentSetId.DisplayName} ({AlignmentSetId.Id}) in {stopwatch.ElapsedMilliseconds} ms");
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

                    Alignments = new AlignmentCollection(await AlignmentSet.GetAlignments(ParallelTextRows));
                    
                    stopwatch.Stop();
                    Logger.LogInformation($"Retrieved {Alignments.Count} alignments from alignment set {AlignmentSetId.DisplayName} ({AlignmentSetId.Id}) in {stopwatch.ElapsedMilliseconds} ms");
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
        /// Initializes the manager with the alignments for the row.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        public async Task InitializeAsync()
        {
            await GetAlignmentSetAsync();
            await GetAlignmentsAsync();
        }

        public AlignmentManager(List<EngineParallelTextRow> parallelTextRows, 
                                AlignmentSetId alignmentSetId, 
                                IEventAggregator eventAggregator, 
                                ILogger<AlignmentManager> logger, 
                                ILifetimeScope lifetimeScope,
                                IMediator mediator, 
                                IWindowManager windowManager, SelectionManager selectionManager)
        {
            ParallelTextRows = parallelTextRows;
            AlignmentSetId = alignmentSetId;

            EventAggregator = eventAggregator;
            Logger = logger;
            LifetimeScope = lifetimeScope;
            Mediator = mediator;
            WindowManager = windowManager;
            SelectionManager = selectionManager;
        }

        /// <summary>
        /// Creates an <see cref="AlignmentManager"/> instance using the specified DI container.
        /// </summary>
        /// <param name="componentContext">A <see cref="IComponentContext"/> (i.e. LifetimeScope) with which to resolve dependencies.</param>
        /// <param name="parallelTextRow">The <see cref="EngineParallelTextRow"/> containing the tokens to align.</param>
        /// <param name="alignmentSetId">The ID of the alignment set to use for aligning the tokens.</param>
        /// <returns>The constructed AlignmentManager.</returns>
        public static async Task<AlignmentManager> CreateAsync(IComponentContext componentContext,
                                                                List<EngineParallelTextRow> parallelTextRows, 
                                                                AlignmentSetId alignmentSetId)
        {
            var manager = componentContext.Resolve<AlignmentManager>(new NamedParameter("parallelTextRows", parallelTextRows),
                                                                    new NamedParameter("alignmentSetId", alignmentSetId));
            await manager.InitializeAsync();
            return manager;
        }

        public async Task AddAlignment(TokenDisplayViewModel sourceTokenDisplay, TokenDisplayViewModel targetTokenDisplay)
        {
            var alignmentPopupViewModel = GetAlignmentPopupViewModel(SimpleMessagePopupMode.Add, targetTokenDisplay, sourceTokenDisplay);

            var result = await WindowManager.ShowDialogAsync(alignmentPopupViewModel, null,
                SimpleMessagePopupViewModel.CreateDialogSettings(alignmentPopupViewModel.Title));
            if (result == true)
            {
                var alignment = new Alignment(new AlignedTokenPairs(sourceTokenDisplay.CompositeToken ?? sourceTokenDisplay.Token, 
                                                                    targetTokenDisplay.CompositeToken ?? targetTokenDisplay.Token, 1d),
                                                                    alignmentPopupViewModel.Verification);
                try
                {
                    await AlignmentSet!.PutAlignment(alignment);

                    if (alignment.AlignmentId != null)
                    {
                        Alignments!.Add(alignment);
                        await EventAggregator.PublishOnUIThreadAsync(new AlignmentAddedMessage(alignment, sourceTokenDisplay, targetTokenDisplay));
                    }
                    else
                    {
                        Logger.LogError($"An unexpected error occurred while creating an alignment between {sourceTokenDisplay.Token.TokenId} and {targetTokenDisplay.Token.TokenId}.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"An unexpected error occurred while creating an alignment between {sourceTokenDisplay.Token.TokenId} and {targetTokenDisplay.Token.TokenId}.");
                }
            }
        }

        public async Task AddAlignment(TokenDisplayViewModel targetTokenDisplay)
        {
            if (SelectionManager.AnySourceTokens)
            {
                await AddAlignment(SelectionManager.SelectedSourceTokens.First(), targetTokenDisplay);
            }
        }

        //public async Task AddAlignments(IEnumerable<Alignment> alignments)
        //{
        //    try
        //    {
        //        await AlignmentSet!.PutAlignments(alignments);

        //        Alignments!.AddRange(alignments.Where(x => x.AlignmentId!=null));
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, $"An unexpected error occurred while placing multiple alignments at once.");
        //    }

        //}

        private AlignmentPopupViewModel GetAlignmentPopupViewModel(SimpleMessagePopupMode mode, TokenDisplayViewModel? targetTokenDisplay = null,
            TokenDisplayViewModel? sourceTokenDisplay = null)
        {
            var alignmentPopupViewModel = LifetimeScope?.Resolve<AlignmentPopupViewModel>();

            if (alignmentPopupViewModel == null)
            {
                throw new ArgumentNullException(nameof(alignmentPopupViewModel), "AlignmentPopupViewModel needs to be registered with the DI container.");
            }
            alignmentPopupViewModel.SimpleMessagePopupMode = mode;
            alignmentPopupViewModel.SourceTokenDisplay = sourceTokenDisplay;
            alignmentPopupViewModel.TargetTokenDisplay = targetTokenDisplay;
            return alignmentPopupViewModel;
        }

        public async Task DeleteAlignment(TokenDisplayViewModel tokenDisplay)//, bool autoConfirm = false
        {
            var alignmentPopupViewModel = GetAlignmentPopupViewModel(SimpleMessagePopupMode.Delete);
            alignmentPopupViewModel.TargetTokenDisplay = tokenDisplay;

            var result = false;
            //if (!autoConfirm)
            //{
            result = await WindowManager.ShowDialogAsync(alignmentPopupViewModel, null, SimpleMessagePopupViewModel.CreateDialogSettings(alignmentPopupViewModel.Title));
            //}
            
            //if (result == true || autoConfirm)
            //{
            var alignmentIds = FindAlignmentIds(tokenDisplay);
     
            // gather all of the alignments which can be removed and delete them form the database.
            var alignmentsToRemove = new List<Alignment>();
            foreach (var alignmentId in alignmentIds)
            {
                if (alignmentId != null)
                {
                    try
                    {
                        await AlignmentSet!.DeleteAlignment(alignmentId);
                        var alignment = Alignments!.FirstOrDefault(a => a.AlignmentId == alignmentId);
                        if (alignment != null)
                        {
                            alignmentsToRemove.Add(alignment);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "An unexpected error occurred while deleting an alignment.");
                    }
                }
            }


            // Now remove all of the alignments from the AlignmentCollection.
            if (alignmentsToRemove.Count > 0)
            {
                Alignments!.RemoveRange(alignmentsToRemove);
                // Message the rest of the app that the alignments have been removed.
                //if (!autoConfirm)
                //{
                foreach (var alignment in alignmentsToRemove)
                {
                    await EventAggregator.PublishOnUIThreadAsync(new AlignmentDeletedMessage(alignment));
                }
                //}
            }
            //}
        }

        private AlignmentId? FindAlignmentId(TokenDisplayViewModel tokenDisplay)
        {
           var alignment =  Alignments!.FindAlignmentByTokenId(tokenDisplay.AlignmentToken.TokenId.Id);
           return alignment?.AlignmentId;
          
        }

        private IEnumerable<AlignmentId?> FindAlignmentIds(TokenDisplayViewModel tokenDisplay)
        {
            var alignments = Alignments!.FindAlignmentsByTokenId(tokenDisplay.AlignmentToken.TokenId.Id).Where(a=>a != null);
            return alignments.Where(a=>a?.AlignmentId != null).Select(a =>  a?.AlignmentId);

        }
    }
}
