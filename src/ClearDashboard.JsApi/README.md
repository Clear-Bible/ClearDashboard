## .NET WebAssembly Browser app

## Build

First install the required workload

```
dotnet workload install wasm-tools
```

Install node.js for windows using these instructions:

```
https://learn.microsoft.com/en-us/windows/dev-environment/javascript/nodejs-on-windows
```

Open up Windows Power Shell and cd to the ClearDashboard.JsApi source code directory

You can build the app from Visual Studio or from the command-line:

```
dotnet build
```

After building the app, the result is in the `bin/$(Configuration)/net7.0/browser-wasm/AppBundle` directory.

## Run

You can build the app from Visual Studio or the command-line:

```
dotnet run
```

Or you can start any static file server from the AppBundle directory:

```
dotnet tool install dotnet-serve
dotnet serve -d:bin/$(Configuration)/net7.0/browser-wasm/AppBundle
```