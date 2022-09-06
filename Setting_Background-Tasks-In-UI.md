# Setting a Background Task to work with the UI StatusBar

On the ShellView view/viewmodel there is an event handler that listens for background task messages and their status.  This allows the user to get feedback on tasks that are still working, completed, or might have an error.  This quick guide helps you add background tasks to this queue.

## Background Task Lifecycle

1. *CREATING A NEW BACKGROUND TASK MESSAGE*

A background task is created in a viewmodel.  An event that describes that the task has been created is sent via the EventAggragator like this:

```
// send to the task started event aggregator for everyone else to hear about a background task starting
await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(new BackgroundTaskStatus
{
    Name = "PINS",
    Description = "Loading PINS data...",
    StartTime = DateTime.Now,
    TaskStatus = StatusEnum.Working
}));
```

Note: that the enums for `StatusEnum` are:

```
    public enum StatusEnum
    {
        Working,
        Completed,
        Error,
        CancelTaskRequested,
    }
```
The Key used to identify this task is the `Name` field.  Use this same string for all subsequent calls to identify if the task status as it goes through the pipeline till erorring or becoming complete.

2. *CREATING A MESSAGE FOR WHEN A TASK SHOWS AN ERROR*

If the background task errors, send a message indicating the error.  The example is shown like this:

```
// send to the task started event aggregator for everyone else to hear about a task error
await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
    new BackgroundTaskStatus
{
    Name = "PINS",
    EndTime = DateTime.Now,
    ErrorMessage = "Paratext is not installed",
    TaskStatus = StatusEnum.Error
}));             
```                
Things to note, you should set the `EndTime`, an `ErrorMessage`, and change the `TaskStatus` to be an error.  In the UI, tasks with errors are kept in there until replaced by a task with the same name.  Likewise the ErrorMessage is displayed instead of the Description in the UI.

3. *CREATING A MESSAGE FOR WHEN A TESK IS COMPLETE*

When your task is completed.  Once again, use the same `Name` field defined during message creation to identify the message to be replaced.  Use the following:

```
// send to the task started event aggregator for everyone else to hear about a task comletiong
await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(
    new BackgroundTaskStatus
    {
        Name = "PINS",
        EndTime = DateTime.Now,
        Description = "Loading PINS data...Complete",
        TaskStatus = StatusEnum.Completed
    }));
```

Again, add in a the `EndTime`, a new `Description`, and a `TaskStatus` to being complete.  

4. *UI LIFECYCLE*

The UI will automatically purge completed tasks somewhere after 45 seconds.  Items that are currently being worked on or tasks with errors will remain in the queue.

5. *CANCELLING A TASK*

If you have a long running task, from the UI, the user can cancel it.  This will send back through the EventAggragator a message with the same `Name` but with the `TaskStatus` set to `CancelTaskRequested`.  It is up to the developer to watch for this event in their modules.  You can enable the capturing of the messages by defining the `IHandle<BackgroundTaskChangedMessage>` as part of your view module definition.  It will require you to impliment the following handler:

```
    public async Task HandleAsync(BackgroundTaskChangedMessage message, CancellationToken cancellationToken)
    {
        var incomingMessage = message.Status;

        if(incomingMessage.Name == "<PUT IN YOUR TASK NAME HERE>" && incomingMessage.TaskStatus == StatusEnum.CancelTaskRequested)
        {
            // cancel your task here


            // return that your task was cancelled
            incomingMessage.EndTime = DateTime.Now;
            incomingMessage.TaskStatus = StatusEnum.Completed;
            incomingMessage.Description = "Task was cancelled";

            await EventAggregator.PublishOnUIThreadAsync(new BackgroundTaskChangedMessage(incomingMessage));
        }
        await Task.CompletedTask;
    }
```

See the ShellViewModel for an example of this in action.