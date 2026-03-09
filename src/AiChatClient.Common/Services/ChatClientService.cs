using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public sealed class ChatClientService(IChatClient client) : IDisposable
{
	readonly SemaphoreSlim _chatHistorySemaphoreSlim = new(1, 1);
	readonly IChatClient _client = client;
	readonly List<ChatMessage> _conversationHistory = [];

	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IReadOnlyList<ChatMessage> messages, ChatOptions options, [EnumeratorCancellation] CancellationToken token)
	{
		bool didEnumerableCompleteSuccessfully = false;

		await _chatHistorySemaphoreSlim.WaitAsync(token).ConfigureAwait(false);

		try
		{
			token.ThrowIfCancellationRequested();

			_conversationHistory.AddRange(messages);

			await foreach (var response in _client.GetStreamingResponseAsync(_conversationHistory, options, token).ConfigureAwait(false))
			{
				yield return response;
			}

			didEnumerableCompleteSuccessfully = true;
		}
		finally
		{
			if (!didEnumerableCompleteSuccessfully && token.IsCancellationRequested)
			{
				_conversationHistory.RemoveRange(_conversationHistory.Count - messages.Count, messages.Count);
				token.ThrowIfCancellationRequested();
			}

			_chatHistorySemaphoreSlim.Release();
		}
	}

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseForUserAsync(string input, ChatOptions options, CancellationToken token)
	{
		return GetStreamingResponseAsync([new ChatMessage(ChatRole.User, input)], options, token);
	}

	public async Task AddAssistantResponse(string response, CancellationToken token)
	{
		await _chatHistorySemaphoreSlim.WaitAsync(token).ConfigureAwait(false);

		try
		{
			_conversationHistory.Add(new ChatMessage(ChatRole.Assistant, response));
		}
		finally
		{
			_chatHistorySemaphoreSlim.Release();
		}
	}

	public async Task ClearConversationHistory(CancellationToken token)
	{
		await _chatHistorySemaphoreSlim.WaitAsync(token).ConfigureAwait(false);

		try
		{
			_conversationHistory.Clear();
		}
		finally
		{
			_chatHistorySemaphoreSlim.Release();
		}
	}

	public void Dispose()
	{
		_chatHistorySemaphoreSlim.Dispose();
	}
}