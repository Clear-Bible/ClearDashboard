using H.Pipes;
using H.Pipes.Args;
using NamedPipes;
using System;
using System.Threading.Tasks;

namespace ClearDashboard.ParatextPlugin
{
    public class NamedPipesServer : IDisposable
    {
        const string PIPE_NAME = "ClearDashboard";

        private PipeServer<PipeMessage> server;

        public NamedPipesServer()
        {

        }

        public async Task InitializeAsync()
        {
            server = new PipeServer<PipeMessage>(PIPE_NAME);

            server.ClientConnected += async (o, args) => await OnClientConnectedAsync(args);
            server.ClientDisconnected += (o, args) => OnClientDisconnected(args);
            server.MessageReceived += (sender, args) => OnMessageReceived(args.Message);
            server.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await server.StartAsync();
        }

        private async Task OnClientConnectedAsync(ConnectionEventArgs<PipeMessage> args)
        {
            Console.WriteLine($"Client {args.Connection.ServerName} is now connected!");

            await args.Connection.WriteAsync(new PipeMessage
            {
                Action = NamedPipeMessage.ActionType.SendText,
                Text = "Hi from server"
            });
        }

        private void OnClientDisconnected(ConnectionEventArgs<PipeMessage> args)
        {
            Console.WriteLine($"Client {args.Connection.ServerName} disconnected");
        }

        private void OnMessageReceived(PipeMessage message)
        {
            if (message == null)
                return;

            switch (message.Action)
            {
                case NamedPipeMessage.ActionType.SendText:
                    Console.WriteLine($"Text from client: {message.Text}");
                    break;

                default:
                    Console.WriteLine($"Unknown Action Type: {message.Action}");
                    break;
            }
        }

        private void OnExceptionOccurred(Exception ex)
        {
            Console.WriteLine($"Exception occured in pipe: {ex}");
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async Task DisposeAsync()
        {
            if (server != null)
                await server.DisposeAsync();
        }
    }
}
