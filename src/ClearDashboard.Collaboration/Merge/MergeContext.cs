using System;
using ClearDashboard.Collaboration.Model;
using ClearDashboard.DAL.Interfaces;
using ClearDashboard.Collaboration.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using SIL.Machine.Utils;
using Models = ClearDashboard.DataAccessLayer.Models;

namespace ClearDashboard.Collaboration.Merge;

public sealed class MergeContext
{
    public MergeMode MergeMode {  get; private set; }

    public MergeBehaviorBase MergeBehavior { get; private set; }
    public IProgress<ProgressStatus> Progress { get => MergeBehavior.Progress; }

    public IUserProvider UserProvider { get; private set; }
    public ILogger Logger { get; private set; }
    public DefaultMergeHandler DefaultMergeHandler { get; private set; }
    private readonly Dictionary<Type, DefaultMergeHandler> _mergeHandlerRegistry = new();

    public bool FireAlignmentDenormalizationEvent = false;

    public MergeContext(IUserProvider userProvider, ILogger logger, MergeBehaviorBase mergeBehavior, MergeMode mergeMode)
	{
        UserProvider = userProvider;
        Logger = logger;
        MergeBehavior = mergeBehavior;
        MergeMode = mergeMode;

        DefaultMergeHandler = new DefaultMergeHandler(this);
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TokenizedCorpus>), new TokenizedCorpusHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.VerseRow>), new VerseRowHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TokenComposite>), new TokenCompositeHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Token>), new TokenHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.AlignmentSet>), new AlignmentSetHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Alignment>), new AlignmentHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.TranslationSet>), new TranslationSetHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Translation>), new TranslationHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.ParallelCorpus>), new ParallelCorpusHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Lexicon_Lexeme>), new LexiconHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Label>), new LabelHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.LabelGroup>), new LabelGroupHandler(this));
        _mergeHandlerRegistry.Add(typeof(IModelSnapshot<Models.Note>), new NoteHandler(this));
        _mergeHandlerRegistry.Add(typeof(NoteModelRef), new NoteModelRefHandler(this));
    }

    public DefaultMergeHandler<T> FindMergeHandler<T>() where T : IModelSnapshot
    {
        if (_mergeHandlerRegistry.TryGetValue(typeof(T), out var mergeHandler))
        {
            return (DefaultMergeHandler<T>)mergeHandler;
        }

        var handler = new DefaultMergeHandler<T>(this);
        _mergeHandlerRegistry.Add(typeof(T), handler);

        return handler;
    }

    public DefaultMergeHandler FindMergeHandler(Type type)
    {
        if (_mergeHandlerRegistry.TryGetValue(type, out var mergeHandler))
        {
            return mergeHandler;
        }

        Type[] handlerTypeArgs = { type };
        var handlerType = typeof(DefaultMergeHandler<>).MakeGenericType(handlerTypeArgs);

        object?[] constructorArgs = { this };
        var handler = (DefaultMergeHandler)Activator.CreateInstance(handlerType, constructorArgs)!;

        _mergeHandlerRegistry.Add(type, handler);

        return handler;
    }
}

