using Pipes_Shared;
using H.Pipes;

namespace ConsoleAppNamedPipes
{
    public static class PipesClient
    {
        private static void OnExceptionOccurred(Exception exception)
        {
            Console.Error.WriteLine($"Exception: {exception}");
        }

        public static async Task RunAsync(string pipeName)
        {
            try
            {
                using var source = new CancellationTokenSource();

                Console.WriteLine($"Running in CLIENT mode. PipeName: {pipeName}");
                Console.WriteLine("Enter 'q' to exit");

                await using var client = new PipeClient<PipeMessage>(pipeName);
                client.Disconnected += (o, args) => Console.WriteLine("Disconnected from server");
                client.Connected += (o, args) => Console.WriteLine("Connected to server");
                client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);
                client.MessageReceived += (sender, args) =>
                {
                    if (args.Message is not null)
                    {
                        OnMessageReceivedAsync(args.Message);
                    }
                };

                // Dispose is not required
                var _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        try
                        {
                            var message = await Console.In.ReadLineAsync().ConfigureAwait(false);
                            if (message == "q")
                            {
                                source.Cancel();
                                break;
                            }

                            if (message is not null)
                            {
                                await client.WriteAsync(new PipeMessage
                                {
                                    Action = ActionType.SendText,
                                    Text = message,
                                }, source.Token).ConfigureAwait(false);
                            }
                        }
                        catch (Exception exception)
                        {
                            OnExceptionOccurred(exception);
                        }
                    }
                }, source.Token);

                Console.WriteLine("Client connecting...");

                await client.ConnectAsync(source.Token).ConfigureAwait(false);

                await Task.Delay(Timeout.InfiniteTimeSpan, source.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private static void OnMessageReceivedAsync(PipeMessage message)
        {
            Console.WriteLine(message.Text);
        }
    }
}
