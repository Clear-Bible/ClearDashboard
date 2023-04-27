using System;
using ClearDashboard.Collaboration.DifferenceModel;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.DataAccessLayer.Data;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public sealed class MergeContext
{
    public bool RemoteOverridesLocal { get; private set; }
    public MergeBehaviorBase MergeBehavior { get; private set; }
    public IProgress<ProgressStatus> Progress { get => MergeBehavior.Progress; }

    public IUserProvider UserProvider { get; private set; }
    public ILogger Logger { get; private set; }
    public DefaultMergeHandler DefaultMergeHandler { get; private set; }
    private readonly Dictionary<Type, DefaultMergeHandler> _mergeHandlerRegistry = new();

    public bool FireAlignmentDenormalizationEvent = false;

    public MergeContext(IUserProvider userProvider, ILogger logger, MergeBehaviorBase mergeBehavior, bool remoteOverridesLocal)
	{
        UserProvider = userProvider;
        Logger = logger;
        MergeBehavior = mergeBehavior;
        RemoteOverridesLocal = remoteOverridesLocal;

        DefaultMergeHandler = new DefaultMergeHandler(this);
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Corpus>), new CorpusHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TokenizedCorpus>), new TokenizedCorpusHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.VerseRow>), new VerseRowHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TokenComposite>), new TokenCompositeHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.AlignmentSet>), new AlignmentSetHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Alignment>), new AlignmentHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Translation>), new TranslationHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.ParallelCorpus>), new ParallelCorpusHandler(this));
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

    public DefaultMergeHandler FindMergeHandler(Type type)
    {
        if (_mergeHandlerRegistry.TryGetValue(type, out var mergeHandler))
        {
            return mergeHandler;
        }

        return DefaultMergeHandler;
    }
}

