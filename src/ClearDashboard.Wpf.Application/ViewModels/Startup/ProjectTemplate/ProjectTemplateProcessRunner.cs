using ClearBible.Engine.Tokenization;
using ClearDashboard.DAL.Alignment.Corpora;
using ClearDashboard.DAL.Alignment.Exceptions;
using ClearDashboard.DAL.Alignment;
using DataAccessLayerModels = ClearDashboard.DataAccessLayer.Models;
using ClearDashboard.DataAccessLayer.Threading;
using ClearDashboard.Wpf.Application.Controls.ProjectDesignSurface;
using ClearDashboard.Wpf.Application.Models;
using ClearDashboard.Wpf.Application.Services;
using ClearDashboard.Wpf.Application.ViewModels.Project;
using ClearDashboard.Wpf.Application.ViewModels.Shell;
using ControlzEx.Standard;
using MediatR;
using SIL.Machine.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ClearDashboard.DataAccessLayer.Threading.BackgroundTaskStatus;
using ClearDashboard.Wpf.Application.Properties;
using ClearDashboard.Wpf.Application.Helpers;
using Caliburn.Micro;
using Microsoft.Extensions.Logging;
using SIL.Machine.Corpora;
using ClearBible.Engine.Corpora;
using SIL.ObjectModel;

namespace ClearDashboard.Wpf.Application.ViewModels.Startup.ProjectTemplate
{
    class ProjectTemplateProcessRunner
    {
        public Tokenizers Tokenizers { get; init; }

        private Mediator Mediator { get; init; }
        private ILogger Logger { get; init; }
        private IEventAggregator EventAggregator { get; init; }

        private LongRunningTaskManager LongRunningTaskManager { get; init; }
        private SystemPowerModes SystemPowerModes { get; init; }

        private readonly ObservableDictionary<string, bool> _busyState = new();

        public bool IsBusy => _busyState.Count > 0;


        //public Task RunAsync(CancellationToken cancellationToken)
        //{

        //}

        public Task AddParatextProjectAsync(DataAccessLayerModels.ParatextProjectMetadata paratextProject, Tokenizers tokenizer, IEnumerable<string> bookIds, CancellationToken cancellationToken)
        {
            var taskName = $"{paratextProject.Name}";
            _busyState.Add(taskName, true);

            var task = LongRunningTaskManager.Create(taskName, LongRunningTaskStatus.Running);
            var taskCancellationToken = task.CancellationTokenSource!.Token;

            var taskRun = Task.Run(async () =>
            //_ = await Task.Factory.StartNew(async () =>
            {
                if (Settings.Default.EnablePowerModes && SystemPowerModes.IsLaptop)
                {
                    await SystemPowerModes.TurnOnHighPerformanceMode();
                }

                CorpusNodeViewModel node = new()
                {
                    TranslationFontFamily = paratextProject.FontFamily
                };

                var soundType = SoundType.Success;

                try
                {
                    var topLevelProjectIds = await TopLevelProjectIds.GetTopLevelProjectIds(Mediator);
                    var corpusId = topLevelProjectIds.CorpusIds.FirstOrDefault(c => c.ParatextGuid == paratextProject.Id);
                    var corpus = corpusId != null ? await Corpus.Get(Mediator, corpusId) : null;

                    // first time for this corpus
                    if (corpus is null)
                    {
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                            description: $"Creating corpus '{paratextProject.Name}'...", cancellationToken: cancellationToken,
                            backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                        corpus = await Corpus.Create(
                            mediator: Mediator,
                            IsRtl: paratextProject.IsRtl,
                            FontFamily: paratextProject.FontFamily,
                            Name: paratextProject.Name!,
                            Language: paratextProject.LanguageName!,
                            CorpusType: paratextProject.CorpusTypeDisplay,
                            ParatextId: paratextProject.Id!,
                            token: cancellationToken);

                        corpus.CorpusId.FontFamily = paratextProject.FontFamily;
                    }

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Tokenizing and transforming '{paratextProject.Name}' corpus...", cancellationToken: cancellationToken,
                        backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    var textCorpus = tokenizer switch
                    {
                        Tokenizers.LatinWordTokenizer =>
                            (await ParatextProjectTextCorpus.Get(Mediator!, paratextProject.Id!, bookIds, cancellationToken))
                            .Tokenize<LatinWordTokenizer>()
                            .Transform<IntoTokensTextRowProcessor>()
                            .Transform<SetTrainingBySurfaceLowercase>(),
                        Tokenizers.WhitespaceTokenizer =>
                            (await ParatextProjectTextCorpus.Get(Mediator!, paratextProject.Id!, bookIds, cancellationToken))
                            .Tokenize<WhitespaceTokenizer>()
                            .Transform<IntoTokensTextRowProcessor>()
                            .Transform<SetTrainingBySurfaceLowercase>(),
                        Tokenizers.ZwspWordTokenizer => (await ParatextProjectTextCorpus.Get(Mediator!, paratextProject.Id!, bookIds, cancellationToken))
                            .Tokenize<ZwspWordTokenizer>()
                            .Transform<IntoTokensTextRowProcessor>()
                            .Transform<SetTrainingBySurfaceLowercase>(),
                        _ => (await ParatextProjectTextCorpus.Get(Mediator!, paratextProject.Id!, null, cancellationToken))
                            .Tokenize<WhitespaceTokenizer>()
                            .Transform<IntoTokensTextRowProcessor>()
                            .Transform<SetTrainingBySurfaceLowercase>()
                    };

                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Running,
                        description: $"Creating tokenized text corpus for '{paratextProject.Name}' corpus...",
                        cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    // ReSharper disable once UnusedVariable
                    var tokenizedTextCorpus = await textCorpus.Create(Mediator, corpus.CorpusId,
                        paratextProject.Name, tokenizer.ToString(), paratextProject.ScrVers, cancellationToken);


                    await SendBackgroundStatus(taskName, LongRunningTaskStatus.Completed,
                        description: $"Creating tokenized text corpus for '{paratextProject.Name}' corpus...Completed",
                        cancellationToken: cancellationToken, backgroundTaskMode: BackgroundTaskMode.PerformanceMode);

                    LongRunningTaskManager.TaskComplete(taskName);
                }
                catch (OperationCanceledException)
                {
                    Logger!.LogInformation("AddParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                }
                catch (MediatorErrorEngineException ex)
                {
                    if (ex.Message.Contains("The operation was canceled"))
                    {
                        Logger!.LogInformation($"AddParatextCorpus() - OperationCanceledException was thrown -> cancellation was requested.");
                    }
                    else
                    {
                        Logger!.LogError(ex, "an unexpected Engine exception was thrown.");
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            soundType = SoundType.Error;
                            await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                                exception: ex, cancellationToken: cancellationToken,
                                backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                        }
                    }


                }
                catch (Exception ex)
                {
                    Logger!.LogError(ex, $"An unexpected error occurred while creating the the corpus for {paratextProject.Name} ");
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        soundType = SoundType.Error;
                        await SendBackgroundStatus(taskName, LongRunningTaskStatus.Failed,
                            exception: ex, cancellationToken: cancellationToken,
                            backgroundTaskMode: BackgroundTaskMode.PerformanceMode);
                    }
                }
                finally
                {
                    LongRunningTaskManager.TaskComplete(taskName);
                    _busyState.Remove(taskName);
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        PlaySound.PlaySoundFromResource(soundType);
                    }

                    // check to see if there are still High Performance Tasks still out there
                    //var numTasks = BackgroundTasksViewModel.GetNumberOfPerformanceTasksRemaining();
                    //if (numTasks == 0 && SystemPowerModes.IsHighPerformanceEnabled)
                    //{
                    //    // shut down high performance mode
                    //    await SystemPowerModes.TurnOffHighPerformanceMode();
                    //}

                }
            }, cancellationToken);

            return taskRun;
        }

        public async Task SendBackgroundStatus(string name, LongRunningTaskStatus status,
            CancellationToken cancellationToken, string? description = null, Exception? exception = null,
            BackgroundTaskMode backgroundTaskMode = BackgroundTaskMode.Normal)
        {
            var backgroundTaskStatus = new BackgroundTaskStatus
            {
                Name = name,
                EndTime = DateTime.Now,
                Description = !string.IsNullOrEmpty(description) ? description : null,
                ErrorMessage = exception != null ? $"{exception}" : null,
                TaskLongRunningProcessStatus = status,
                BackgroundTaskType = backgroundTaskMode,
            };
            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(backgroundTaskStatus), cancellationToken);
        }

        public ProjectTemplateProcessRunner(
            ILogger logger,
            Mediator mediator,
            IEventAggregator eventAggregator,
            LongRunningTaskManager longRunningTaskManager,
            SystemPowerModes systemPowerModes)
        {
            Logger = logger;
            Mediator = mediator;
            EventAggregator = eventAggregator;
            LongRunningTaskManager = longRunningTaskManager;
            SystemPowerModes = systemPowerModes;
        }
    }
}
