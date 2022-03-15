// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
const string DefaultPipeName = "ClearDashboard";

string? mode;
string pipeName = DefaultPipeName;

if (args.Any())
{
    mode = args.ElementAt(0);
    pipeName = args.ElementAtOrDefault(1) ?? DefaultPipeName;
}
else
{
    await MyClient.RunAsync(pipeName).ConfigureAwait(false);
}