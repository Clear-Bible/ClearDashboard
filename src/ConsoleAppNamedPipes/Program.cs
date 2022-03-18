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

sealed class CustomizedBinder : SerializationBinder
{
    public override Type BindToType(string assemblyName, string typeName)
    {
        Type returntype = null;
        string sharedAssemblyName = "pipes_shared, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null";
        assemblyName = Assembly.GetExecutingAssembly().FullName;
        typeName = typeName.Replace(sharedAssemblyName, assemblyName);
        returntype =
            Type.GetType(String.Format("{0}, {1}",
                typeName, assemblyName));

        return returntype;
    }

    public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        base.BindToName(serializedType, out assemblyName, out typeName);
        assemblyName = "SharedAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
    }
}