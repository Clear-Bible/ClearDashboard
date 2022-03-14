using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.Pipes;
using NamedPipes;


namespace ClearDashboard.DAL.NamedPipes
{
    public class NamedPipesClient : IDisposable
    {
        #region Events

        public class PipeEventArgs : EventArgs
    {
        private readonly string _text;

        public PipeEventArgs(string text)
        {
            this._text = text;
        }

        public string Text
        {
            get { return this._text; }
        }
    }

    public delegate void PipesEventHandler(
        object sender,
        PipeEventArgs args);

    public event PipesEventHandler NamedPipeChanged;

    private void RaisePipesChangedEvent(string s)
    {
        PipeEventArgs args = new PipeEventArgs(s);
        NamedPipeChanged?.Invoke(this, args);
    }

    #endregion


    #region Props

    const string pipeName = "ClearDashboard";

    private static NamedPipesClient instance;
    private PipeClient<PipeMessage> client;

    public static NamedPipesClient Instance
    {
        get
        {
            return instance ?? new NamedPipesClient();
        }
    }

    #endregion

    #region startup

    private NamedPipesClient()
    {
        instance = this;
    }

    #endregion

    #region Methods

    public async Task InitializeAsync()
    {
        if (client != null && client.IsConnected)
            return;

        client = new PipeClient<PipeMessage>(pipeName);
        client.MessageReceived += (sender, args) =>
        {
            if (args.Message is not null)
            {
                OnMessageReceivedAsync(args.Message);
            }
        };
        client.Disconnected += (o, args) => HandleEvents("Disconnected from server");
        client.Connected += (o, args) => Debug.WriteLine("Connected to server");
        client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

        await client.ConnectAsync();

        await client.WriteAsync(new PipeMessage
        {
            Action = NamedPipeMessage.ActionType.SendText,
            Text = "Hello from client",
        });
    }

    public void HandleEvents(string s)
    {
        RaisePipesChangedEvent(s);
    }


    private void OnMessageReceivedAsync(PipeMessage message)
    {
        switch (message.Action)
        {
            case NamedPipeMessage.ActionType.SendText:
                HandleEvents(message.Text);
                break;
            default:
                HandleEvents($"Method {message.Action} not implemented");
                break;
        }
    }

    private void OnExceptionOccurred(Exception exception)
    {
        Debug.WriteLine($"An exception occured: {exception}");
    }

    public void Dispose()
    {
        if (client != null)
            client.DisposeAsync().GetAwaiter().GetResult();
    }

    #endregion

    public async Task WriteAsync(string message)
    {
        await client.WriteAsync(new PipeMessage
        {
            Action = NamedPipeMessage.ActionType.SendText,
            Text = message,
        });
    }
}
}
