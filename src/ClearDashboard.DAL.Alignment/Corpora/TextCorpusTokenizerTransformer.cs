using ClearBible.Engine.Proto;
using ClearEngineWebApi.ProtoBufUtils;
using Google.Protobuf;
using SIL.Machine.Corpora;
using SIL.Machine.Tokenization;
using System.Net;
using System.Net.WebSockets;

namespace ClearDashboard.DAL.Alignment.Corpora
{
	public class TextCorpusTokenizerTransformer
	{
		public TextCorpusTokenizerTransformer(ScriptureTextCorpus textCorpus)
		{
			TextCorpus = textCorpus;
			TokenizeTransformChain = new();
		}

		public TextCorpusTokenizerTransformer AddTokenizer<T>()
			where T : ITokenizer<string, int, string>
		{
			return AddTokenizer(typeof(T).FullName!);
		}

		public TextCorpusTokenizerTransformer AddTokenizer(string tokenizerClassName)
		{
			TokenizeTransformChain.Add($"Tokenize({tokenizerClassName})");
			return this;
		}

		public TextCorpusTokenizerTransformer AddTransformer<T>()
			where T : IRowProcessor<TextRow>
		{
			return AddTransformer(typeof(T).FullName!);
		}

		public TextCorpusTokenizerTransformer AddTransformer(string transformerClassName)
		{
			TokenizeTransformChain.Add($"Transform({transformerClassName})");
			return this;
		}

		public ITextCorpus TokenizeTransform()
		{
			var tokenizedTextCorpus = RemoteTokenize().GetAwaiter().GetResult();
			if (tokenizedTextCorpus != null)
			{
				return tokenizedTextCorpus;
			}

			tokenizedTextCorpus = TextCorpus;
			var tokenizerTransformers = TokenizerConverter.FromProtocolBuf(TokenizeTransformChain);

			foreach (var (Tokenizer, RowProcessor) in tokenizerTransformers)
			{
				if (Tokenizer is not null)
				{
					tokenizedTextCorpus = tokenizedTextCorpus.Tokenize(Tokenizer);
				}
				else
				{
					tokenizedTextCorpus = tokenizedTextCorpus.Transform(RowProcessor);
				}
			}
			return tokenizedTextCorpus;
		}

		private async Task<ITextCorpus?> RemoteTokenize()
		{
			var uri = new Uri("ws://ec2-3-145-211-91.us-east-2.compute.amazonaws.com/ws/tokenizetextcorpus");
			var httpVersion = HttpVersion.Version11;

			using SocketsHttpHandler handler = new();
			using ClientWebSocket ws = new();

			Console.WriteLine($"HttpVersion set to: {httpVersion}");
			ws.Options.HttpVersion = httpVersion;
			ws.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

			await ws.ConnectAsync(uri, new HttpMessageInvoker(handler), CancellationToken.None);

			WebSocketReceiveResult? result = null;
			byte[] buf = new byte[1056];
			ITextCorpus? tokenizedCorpus = null;

			if (ws.State == WebSocketState.Open)
			{
				var textCorpus = CorporaConverter.ToProtocolBuf(TextCorpus);

				var tokenizationRequest = new TokenizationRequest();
				tokenizationRequest.TokenizeTransformChainFullNames.AddRange(TokenizeTransformChain);
				tokenizationRequest.TextCorpus = textCorpus;
				var textCorpusBinary = tokenizationRequest.ToByteArray();

				await BufferHelper.SendBinary(ws, textCorpusBinary);

				(result, var buffer) = await BufferHelper.ReceiveBinary(ws);
				if (result.MessageType == WebSocketMessageType.Binary)
				{
					var tokensTextCorpus = TokensTextCorpus.Parser.ParseFrom(buffer);
					(tokenizedCorpus, var Versification) = CorporaConverter.FromProtocolBuf(tokensTextCorpus);
				}

				// if (result.MessageType == WebSocketMessageType.Close)
				// {
				await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
				Console.WriteLine(result.CloseStatusDescription);
				// }
			}

			return tokenizedCorpus;
		}

		public ScriptureTextCorpus TextCorpus { get; private set; }
		public List<string> TokenizeTransformChain { get; }
	}
}
