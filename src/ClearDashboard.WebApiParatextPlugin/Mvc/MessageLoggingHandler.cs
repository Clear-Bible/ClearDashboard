using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClearDashboard.WebApiParatextPlugin.Mvc
{
    public abstract class MessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var corrId = string.Format("{0}{1}", DateTime.Now.Ticks, Thread.CurrentThread.ManagedThreadId);
            var requestInfo = string.Format("{0} {1}", request.Method, request.RequestUri);

            var requestMessage = await request.Content.ReadAsByteArrayAsync();

            await IncommingMessageAsync(corrId, requestInfo, requestMessage);

            var response = await base.SendAsync(request, cancellationToken);

            byte[] responseMessage;

            if (response.IsSuccessStatusCode)
                responseMessage = await response.Content.ReadAsByteArrayAsync();
            else
                responseMessage = Encoding.UTF8.GetBytes(response.ReasonPhrase);

            await OutgoingMessageAsync(corrId, requestInfo, responseMessage);

            return response;
        }


        protected abstract Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message);
        protected abstract Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message);
    }



    public class MessageLoggingHandler : MessageHandler
    {
        private MainWindow _mainWindow;
        public MessageLoggingHandler(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                var logMessage = $"{correlationId} - Request: {requestInfo}\r\n{Encoding.UTF8.GetString(message)}";
                Debug.WriteLine(logMessage);
                _mainWindow.AppendText(Color.SteelBlue, logMessage);

            });
        }
                
   


        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                var logMessage = $"{correlationId} - Response: {requestInfo}\r\n{Encoding.UTF8.GetString(message)}";
                Debug.WriteLine(logMessage);
                _mainWindow.AppendText(Color.SteelBlue, logMessage);
            });
        }
    }
}
