// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Runtime.Serialization;
using ConsoleAppNamedPipes;
using H.Formatters;
using Pipes_Shared;

Console.WriteLine("Hello, World!");
// Named Pipe Props
const string PipeName = "ClearDashboard";
  
string? mode;
string pipeName = PipeName;

if (args.Any())
{
    mode = args.ElementAt(0);
    pipeName = args.ElementAtOrDefault(1) ?? PipeName;
}
else
{

    _ = new PipeMessage();
    await PipesClient.RunAsync(pipeName).ConfigureAwait(false);
}

