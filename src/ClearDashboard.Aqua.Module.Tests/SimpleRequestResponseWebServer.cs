using System.Net;
using Xunit.Abstractions;

namespace ClearDashboard.Aqua.Module.Tests
{
    public class SimpleRequestResponseWebServer : IDisposable
    {
        private HttpListener listener_;
        private readonly ITestOutputHelper? output_;

        public SimpleRequestResponseWebServer(
            string listenerPrefix = "http://127.0.0.1:8080/",
            ITestOutputHelper? output = null)
        {
            // URI prefix is required,
            listener_ = new HttpListener();
            listener_.Prefixes.Add(listenerPrefix);
            output_ = output;
        }

        public Task Start(
            EventWaitHandle? startedWaitHandle = null)
        {
            return Task.Run(() =>
            {
                listener_.Start();
                output_?.WriteLine("Http server listening...");
                //tell callers its started and listening.
                if (startedWaitHandle != null)
                    startedWaitHandle.Set();

                HttpListenerContext context = listener_.GetContext(); //blocks
                HttpListenerRequest request = context.Request;

                using Stream body = request.InputStream;
                System.Text.Encoding encoding = request.ContentEncoding;
                using StreamReader reader = new StreamReader(body, encoding);
                if (request.ContentType != null)
                {
                    output_?.WriteLine("Client data content type {0}", request.ContentType);
                }
                output_?.WriteLine("Client data content length {0}", request.ContentLength64);

                output_?.WriteLine("Start of client data:");
                // Convert the data to a string and display it on the console.
                string s = reader.ReadToEnd();
                output_?.WriteLine(s);
                output_?.WriteLine("End of client data:");
                //body.Close();
                //reader.Close();

                // Obtain a response object.
                HttpListenerResponse response = context.Response;
                // Construct a response.
                string responseString = s;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                using Stream streamOutput = response.OutputStream;
                streamOutput.Write(buffer, 0, buffer.Length);
                streamOutput.Flush();
                //streamOutput.Close();
                //listener.Stop();
                output_?.WriteLine("Http server stopped.");
            });
        }

        public void Stop()
        {
            listener_.Stop();
        }
        public void Dispose()
        {
            Stop();
        }
    }
}
