using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public sealed class ChatClientService(IChatClient client) : IDisposable
{
	readonly SemaphoreSlim _chatHistorySemaphoreSlim = new(1, 1);
	readonly IChatClient _client = client;
	readonly List<ChatMessage> _conversationHistory = [];

	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options, [EnumeratorCancellation] CancellationToken token)
	{
		await _chatHistorySemaphoreSlim.WaitAsync(token).ConfigureAwait(false);
		var getStreamingResponseAsyncEnumerable = _client.GetStreamingResponseAsync(_conversationHistory, options, token);

		try
		{
			token.ThrowIfCancellationRequested();

			_conversationHistory.AddRange(messages);

			await foreach (var response in getStreamingResponseAsyncEnumerable.ConfigureAwait(false))
			{
				yield return response;
			}
		}
		finally
		{
			if (!await IsAsyncEnumeratorComplete(getStreamingResponseAsyncEnumerable, token) 
				&& token.IsCancellationRequested)
			{
				IReadOnlyList<ChatMessage> messagesList = [.. messages];

				_conversationHistory.RemoveRange(_conversationHistory.Count - messagesList.Count, messagesList.Count);
				token.ThrowIfCancellationRequested();
			}

			_chatHistorySemaphoreSlim.Release();
		}

		static async ValueTask<bool> IsAsyncEnumeratorComplete<T>(IAsyncEnumerable<T> asyncEnumerable, CancellationToken token) =>
			!await (asyncEnumerable.GetAsyncEnumerator(token)?.MoveNextAsync() ?? ValueTask.FromResult(false));
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