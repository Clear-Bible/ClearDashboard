

using Caliburn.Micro;
using ClearDashboard.Wpf.Application.Messages;
using SIL.Scripture;
using System;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace ClearDashboard.Paranext.Module.Services
{
    public class DashboardJavascriptApi
    {
        private readonly IEventAggregator _eventAggregator;

        public DashboardJavascriptApi(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public DashboardJavascriptApi()
        {
        }

        public async Task<string> VerseChange(string verseString)
        {
            //await _eventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(verseString));
            await Task.Run(() => { });
            return $"FROM_DASHBOARD: {verseString}";
        }

        public string ConstructError(int? id, int code, string message)
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                jsonrpc = 2.0,
                error = code,
                message = message,
                id = id
            });
        }

        public virtual async Task<string?> ServiceRequest(string requestJsonRpc)
        {
            try
            {
                int? id = null;

                JsonNode? jsonRpc = JsonNode.Parse(requestJsonRpc)!;
                if (jsonRpc == null)
                {
                    return ConstructError(id, 100, "cannot parse parameter into valid json");
                }

                var versionNode = jsonRpc!["jsonrpc"];
                if (versionNode == null)
                    return ConstructError(id, 200, "parameter does not contain 'jsonrpc' parameter");
                string version;
                try
                {
                    version = versionNode.GetValue<string>();
                }
                catch (FormatException)
                {
                    return ConstructError(id, 201, "'jsonrpc' parameter cannot be represented as a string");
                }
                if (!version.Equals("2.0"))
                {
                    return ConstructError(id, 202, "'jsonrpc' parameter is not '2.0'");
                }

                var idNode = jsonRpc!["id"];
                if (idNode != null)
                {
                    try
                    {
                        id = idNode!.GetValue<int>();
                    }
                    catch (FormatException)
                    {
                        return ConstructError(id, 300, "'id' parameter is included but cannot be represented as an int");
                    }
                }

                var methodNode = jsonRpc!["method"];
                if (methodNode == null)
                    return ConstructError(id, 301, "parameter does not contain 'method' parameter");

                string method;
                try
                {
                    method = methodNode.GetValue<string>();
                }
                catch (FormatException)
                {
                    return ConstructError(id, 302, "'method' parameter cannot be represented as a string");
                }

                MethodInfo? methodInfo = GetType().GetMethod(method);
                if (methodInfo == null)
                {
                    return ConstructError(id, 303, $"no implementation for method {method} found");
                }

                string? parameters = null;
                var paramsNode = jsonRpc!["params"];
                if (paramsNode != null)
                {
                    try
                    {
                        parameters = paramsNode.GetValue<string>();
                    }
                    catch (FormatException)
                    {
                        return ConstructError(id, 400, "'params' parameter cannot be represented as a string");
                    }
                }

                var result = methodInfo!.Invoke(this, parameters == null ? null : new object[] { parameters! }) as Task<string>;
                if (result == null)
                {
                    return null;
                }
                else
                {
                    return await result;
                }
            }
            catch (Exception ex)
            {
                return ConstructError(null, 500, $"Unexpected error {ex}");
            }
            //return await methodInfo!.Invoke(this, new object[] { parameters! }) as Task<string> 
            //    ?? throw new System.Exception("");

            //await _eventAggregator.PublishOnUIThreadAsync(new VerseChangedMessage(verseString));
            
            //await Task.Run(() => { });
            //return $"FROM_DASHBOARD: {request}";
        }

        public virtual Task<string?> GetCorpus(string? parameters)
        { 
            return Task.FromResult<string?>(System.Text.Json.JsonSerializer.Serialize(new { Amount = 108, Message = "Hello" }));
        }
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
