using ClearBible.Engine.Proto;
using ClearEngineWebApi.ProtoBufUtils;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ClearDashboard.DAL.Alignment.Features
{
	public class ClearEngineClientWebSocket : IDisposable
	{
		private readonly Uri _serverSocketUri;

		private ClientWebSocket? _webSocket = null;
		private SocketsHttpHandler? _handler = null;

		private readonly ILogger _logger;
		private bool _isSendingReceiving = false;
		private static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

		public ClearEngineClientWebSocket(string? host, ILogger<ClearEngineClientWebSocket> logger) 
		{
			host ??= "localhost:5173";
//			host ??= "ec2-3-145-211-91.us-east-2.compute.amazonaws.com";

			_serverSocketUri = new($@"ws://{host}/ws");

			_handler = new();
			_webSocket = new();

			_webSocket.Options.HttpVersion = HttpVersion.Version11;
			_webSocket.Options.HttpVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;

			_logger = logger;
		}

		public bool IsSendingReceiving => _isSendingReceiving;

		public async Task<R> SendReceiveBinary<S,R>(S messageToSend, CancellationToken cancellationToken)
			where S : IMessage<S>, new()
			where R : IMessage<R>, new()
		{
			if (_webSocket is null || _handler is null)
				throw new ObjectDisposedException(nameof(ClearEngineClientWebSocket));

			if (_webSocket.State != WebSocketState.Open)
				await OpenWebSocket(cancellationToken);

			await semaphoreSlim.WaitAsync(cancellationToken);
			try
			{
				_isSendingReceiving = true;
				IMessage clearEngineRequest = typeof(S).IsAssignableFrom(typeof(ClearEngineRequest))
					? messageToSend
					: new ClearEngineRequest { Message = Any.Pack(messageToSend) };

				await BufferHelper.SendBinary(_webSocket, clearEngineRequest.ToByteArray(), cancellationToken);
				var (result, buffer) = await BufferHelper.ReceiveBinary(_webSocket, cancellationToken);

				if (result.MessageType == WebSocketMessageType.Binary)
				{
					var clearEngineResponse = ClearEngineResponse.Parser.ParseFrom(buffer);
//					var responseParser = new MessageParser<R>(() => new R());
					if (clearEngineResponse.Message.TryUnpack<R>(out var messageResponse))
					{
						return messageResponse;
					}
					else
					{
						throw new InvalidDataException($"Expected WebSocket response message of type '{typeof(R).Name}', but received '{Any.GetTypeName(clearEngineResponse.Message.TypeUrl)}' instead");
					}
				}
				else
				{
					throw new InvalidDataException($"Received unexpected response message type: {result.MessageType}");
				}
			}
			finally
			{
				semaphoreSlim.Release();
				_isSendingReceiving = false;
			}
		}

		public async Task OpenWebSocket(CancellationToken cancellationToken)
		{
			if (_webSocket is null || _handler is null)
				throw new ObjectDisposedException(nameof(ClearEngineClientWebSocket));

			await semaphoreSlim.WaitAsync(cancellationToken);
			try
			{
				if (_webSocket.State != WebSocketState.Open)
				{
					_logger.LogInformation("Opening WebSocket connection");

					await _webSocket.ConnectAsync(
						_serverSocketUri,
						new HttpMessageInvoker(_handler),
						cancellationToken);
				}
			}
			finally
			{
				semaphoreSlim.Release();
			}
		}

		public async Task CloseWebSocket(string? closeStatusDescription, CancellationToken cancellationToken)
		{
			if (_webSocket is null || _handler is null)
				throw new ObjectDisposedException(nameof(ClearEngineClientWebSocket));

			await semaphoreSlim.WaitAsync(cancellationToken);
			try
			{
				if (_webSocket.State != WebSocketState.Closed)
				{
					_logger.LogInformation($"Closing WebSocket: {closeStatusDescription}");

					await _webSocket.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						closeStatusDescription, 
						cancellationToken);
				}
			}
			finally
			{
				semaphoreSlim.Release();
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_webSocket is not null)
				{
					CloseWebSocket("Dispose", CancellationToken.None).GetAwaiter().GetResult();

					_logger.LogInformation($"Disposing WebSocket");
					_webSocket.Dispose();
				}

				if (_handler is not null)
				{
					_logger.LogInformation($"Disposing SocketsHttpHandler");
					_handler.Dispose();
				}

				_webSocket = null;
				_handler = null;
			}
		}
	}
}
