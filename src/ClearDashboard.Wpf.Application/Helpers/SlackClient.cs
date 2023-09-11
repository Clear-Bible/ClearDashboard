using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClearDashboard.Wpf.Application.Helpers
{
    public class SlackMessage
    {
        private readonly string _messageText;
        private readonly string _filePath;
        private readonly ILogger<SlackMessage> _logger;
        private readonly SlackMessageType _slackMessageType;
        private const string BugChannel = "C05PV2VM2JU";  //'clear-dashboard-external-logs' channel
        private const string SuggestionChannel = "C05PTF00DL5";  //'clear-dashboard-external-suggestions' channel
        private const string Token = "xoxb-4696406923-5804914278598-WoMrOnYk4E3zV2VoSrJvEEbn";

        public enum SlackMessageType
        {
            BugReport,
            Suggestion
        }


        public SlackMessage(string messageText, string filePath, ILogger<SlackMessage> logger, SlackMessageType slackMessageType)
        {
            _messageText = messageText;
            _filePath = filePath;
            _logger = logger;
            _slackMessageType = slackMessageType;
        }

        /// <summary>
        /// Used to send a message and file to a slack channel
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendFileToSlackAsync()
        {
            var webhookUrl = new Uri("https://slack.com/api/files.upload");
            var slackClient = new SlackClient(webhookUrl);

            var postMessage = new PostMessage();

            if (_slackMessageType == SlackMessageType.BugReport)
            {
                postMessage.Channel = BugChannel;
            }
            else
            {
                postMessage.Channel = SuggestionChannel;  // clear-dashboard-external-suggestions
            }
                        
            postMessage.MessageText = _messageText;
            postMessage.FilePath = _filePath;
            postMessage.Token = Token;

            // make the message content
            var content = postMessage.UploadFile();

            // do the message upload
            var response = await slackClient.UploadFileAsync(content);
            string responseJson = await response.Content.ReadAsStringAsync();

            // release the file lock
            content.Dispose();
            
            // convert JSON response to object
            var fileResponse = System.Text.Json.JsonSerializer.Deserialize<SlackFileResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (fileResponse is null)
            {
                _logger.LogError("Slack file upload failed. Response was null.");
                return false;
            }

            // throw exception if sending failed
            if (fileResponse.Ok == false)
            {
                _logger.LogWarning($"Error sending file to Slack: {fileResponse.Error}");
                return false;
            }

            return true;

        }
    }

    /// <summary>
    /// response from file methods
    /// </summary>
    public class SlackFileResponse
    {
        public bool Ok { get; set; }
        public String? Error { get; set; }
        public SlackFile? File { get; set; }
    }


    /// <summary>
    /// a slack file
    /// </summary>
    public class SlackFile
    {
        public String? Id { get; set; }
        public String? Name { get; set; }
    }


    public class SlackClient
    {
        public Uri Method { get; set; }
        
        private readonly HttpClient _httpClient = new();

        public SlackClient(Uri webHookUrl)
        {
            Method = webHookUrl;
        }

        public async Task<HttpResponseMessage> UploadFileAsync(MultipartFormDataContent requestContent)
        {
            var response = await _httpClient.PostAsync(Method, requestContent);
            return response;
        }
    }

    public class PostMessage
    {
        public string Token { get; set; } = "";
        public string Channel { get; set; } = "";
        public string MessageText { get; set; } = "";
        public string FilePath { get; set; } = "";

        public MultipartFormDataContent UploadFile()
        {
            GetFile getFile = new GetFile(FilePath);

            var requestContent = new MultipartFormDataContent();
            var fileContent = new StreamContent(getFile.ReadFile());
            requestContent.Add(new StringContent(Token), "token");
            requestContent.Add(new StringContent(MessageText), "initial_comment");
            requestContent.Add(new StringContent(Channel), "channels");
            requestContent.Add(fileContent, "file", Path.GetFileName(getFile.FilePath));

            return requestContent;
        }
    }

    public class GetFile
    {
        public string FilePath;
        public GetFile(string filePath)
        {
            FilePath = filePath;
        }
        
        public FileStream ReadFile()
        {
            var fs = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            return fs;
        }
    }
}
