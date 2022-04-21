using System;
using System.Diagnostics;
using System.Threading.Tasks;
using H.Pipes;
using Pipes_Shared;

namespace ClearDashboard.DataAccessLayer.NamedPipes
{
    public class NamedPipesClient : IDisposable
    {
        #region Events

        public delegate void PipesEventHandler(object sender, PipeEventArgs args);

        public event PipesEventHandler NamedPipeChanged = null!;

        private void RaisePipesChangedEvent(PipeMessage s)
        {
            var args = new PipeEventArgs(s);
            NamedPipeChanged?.Invoke(this, args);
        }

        #endregion

        #region Properties

        private const string PipeName = "ClearDashboard";
        private PipeClient<PipeMessage>? _client;

        #endregion

        #region startup

        #endregion

        #region Methods

        public async Task InitializeAsync()
        {
            if (_client is { IsConnected: true })
            {
                return;
            }

            _client = new PipeClient<PipeMessage>(PipeName);
            _client.MessageReceived += (sender, args) =>
            {
                if (args.Message is not null)
                {
                    OnMessageReceivedAsync(args.Message);
                }
            };
            _client.Disconnected += (o, args) => HandleEvents(new PipeMessage
            {
                Action = ActionType.OnDisconnected,
                Text = "Disconnected from server",
            });
            _client.Connected += (o, args) => Debug.WriteLine("Connected to server");
            _client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await _client.ConnectAsync();

            await _client.WriteAsync(new PipeMessage
            {
                Action = ActionType.SendText,
                Text = "Hello from client",
            });
        }

        public async void HandleEvents(PipeMessage pm)
        {
            RaisePipesChangedEvent(pm);

            // restart if disconnected
            if (pm.Action == ActionType.OnDisconnected)
            {
                if (_client is not null)
                {
                    try
                    {
                        await _client.ConnectAsync();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
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
            _client?.DisposeAsync().GetAwaiter().GetResult();
            _client = null;
        }

        #endregion

        public async Task WriteAsync(PipeMessage message)
        {
            await _client.WriteAsync(message);
        }
    }
}
