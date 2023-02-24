using System;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using Microsoft.Extensions.Logging;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public sealed class MergeContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///
    /// MergeContext(MergeBehavior)
    ///     anytime it or one of its handlers needs to actually do something, it uses mergeBehavior
    ///     when processing an individual Create, Modify or Delete item, the specific (or default) handler is used
    ///
    /// 
    /// </summary>
    public bool RemoteOverridesLocal => true;
    public MergeBehaviorBase MergeBehavior { get; private set; }

    public IUserProvider UserProvider { get; private set; }
    public ILogger Logger { get; private set; }
    public DefaultMergeHandler DefaultMergeHandler { get; private set; }
    private readonly Dictionary<Type, DefaultMergeHandler> _mergeHandlerRegistry = new();

    // For the moment, MergeContext is acting as the MergeBehavior factory.
    // If null is passed in for ProjectDbContext, creates the MergeBehavior
    // that only logs (no database interaction).  If we end up making a
    // MergeBehavior that does the querying part (as a validation) but no
    // updates, then obviously 
    public MergeContext(IUserProvider userProvider, ILogger logger, MergeBehaviorBase mergeBehavior)
	{
        UserProvider = userProvider;
        Logger = logger;
        MergeBehavior = mergeBehavior;

        DefaultMergeHandler = new DefaultMergeHandler(this);
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TokenizedCorpus>), new TokenizedCorpusHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.VerseRow>), new VerseRowHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TokenComposite>), new TokenCompositeHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Alignment>), new AlignmentHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Translation>), new TranslationHandler(this));
        _mergeHandlerRegistry.Add(typeof(NoteModelRef), new NoteModelRefHandler(this));
    }

    public DefaultMergeHandler FindMergeHandler<T>()
    {
        if (_mergeHandlerRegistry.TryGetValue(typeof(T), out var mergeHandler))
        {
            return mergeHandler;
        }

        return DefaultMergeHandler;
    }

    public void Dispose() => MergeBehavior.Dispose();
    public async ValueTask DisposeAsync() => await MergeBehavior.DisposeAsync();
}

