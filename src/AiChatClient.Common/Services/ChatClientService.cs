using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public class ChatClientService(IChatClient client)
{
	readonly IChatClient _client = client;
	readonly List<ChatMessage> _conversationHistory = [];

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options, CancellationToken token)
	{
		_conversationHistory.AddRange(messages);
		return _client.GetStreamingResponseAsync(_conversationHistory, options, token);
	}

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseForUserAsync(string input, ChatOptions options, CancellationToken token)
	{
		return GetStreamingResponseAsync([new ChatMessage(ChatRole.User, input)], options, token);
	}

	public void AddAssistantResponse(string response)
	{
		_conversationHistory.Add(new ChatMessage(ChatRole.Assistant, response));
	}

	public void ClearConversationHistory()
	{
		_conversationHistory.Clear();
	}
}