using H.Pipes;
using H.Pipes.Args;
//using NamedPipes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Pipes_Shared;

namespace ClearDashboard.ParatextPlugin
{
    public class NamedPipesServer : IDisposable
    {
        private const string PIPE_NAME = "ClearDashboard";

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
            //try
            //{
            //    await args.Connection.WriteAsync(new PipeMessage
            //    {
            //        Action = NamedPipeMessage.ActionType.SendText,
            //        Text = "Hi from server"
            //    });
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e);
            //}

        }

        private void OnClientDisconnected(ConnectionEventArgs<PipeMessage> args)
        {
            Debug.WriteLine($"Client {args.Connection.ServerName} disconnected");
        }

        private void OnMessageReceived(PipeMessage message)
        {
            if (message == null)
                return;

            //switch (message.Action)
            //{
            //    case NamedPipeMessage.ActionType.SendText:
            //        Debug.WriteLine($"Text from client: {message.Text}");
            //        break;

            //    default:
            //        Debug.WriteLine($"Unknown Action Type: {message.Action}");
            //        break;
            //}
        }

        private void OnExceptionOccurred(Exception ex)
        {
            Debug.WriteLine($"Exception occured in pipe: {ex}");
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
