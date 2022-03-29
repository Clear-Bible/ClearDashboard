using System;
using System.Diagnostics;
using System.Threading.Tasks;
using H.Pipes;
using Pipes_Shared;

namespace ClearDashboard.DataAccessLayer.NamedPipes
{

    public class PipeEventArgs : EventArgs
    {
        private readonly PipeMessage _pm;

        public PipeEventArgs(PipeMessage pm)
        {
            this._pm = pm;
        }

        public PipeMessage PM
        {
            get { return this._pm; }
        }
    }

    public class NamedPipesClient : IDisposable
    {
        #region Events



        public delegate void PipesEventHandler(
            object sender,
            PipeEventArgs args);

        public event PipesEventHandler NamedPipeChanged;

        private void RaisePipesChangedEvent(PipeMessage s)
        {
            PipeEventArgs args = new PipeEventArgs(s);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion


        #region Props

        private const string pipeName = "ClearDashboard";
        private PipeClient<PipeMessage> client;

        #endregion

        #region startup

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            if (client is { IsConnected: true })
                return;

            client = new PipeClient<PipeMessage>(pipeName);
            client.MessageReceived += (sender, args) =>
            {
                if (args.Message is not null)
                {
                    OnMessageReceivedAsync(args.Message);
                }
            };
            client.Disconnected += (o, args) => HandleEvents(new PipeMessage
            {
                Action = ActionType.OnDisconnected,
                Text = "Disconnected from server",
            });
            client.Connected += (o, args) => Debug.WriteLine("Connected to server");
            client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await client.ConnectAsync();

            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.SendText,
                Text = "Hello from client",
            });
        }

        public void HandleEvents(PipeMessage pm)
        {
            RaisePipesChangedEvent(pm);
        }

        private void OnMessageReceivedAsync(PipeMessage message)
        {
            switch (message.Action)
            {
                case ActionType.OnConnected:
                    HandleEvents(message);
                    break;
                case ActionType.SendText:
                    HandleEvents(message);
                    break;
                default:
                    HandleEvents(message);
                    break;
            }
        }

        private void OnExceptionOccurred(Exception exception)
        {
            Debug.WriteLine($"An exception occurred: {exception}");
        }

        public void Dispose()
        {
            if (client != null)
                client.DisposeAsync().GetAwaiter().GetResult();
        }

        #endregion

        public async Task WriteAsync(PipeMessage message)
        {
            await client.WriteAsync(message);
        }
    }
}
