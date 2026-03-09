using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public class ChatClientService(IChatClient client)
{
	readonly IChatClient _client = client;
	readonly List<ChatMessage> _conversationHistory = [];

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(string input, ChatOptions options, CancellationToken token)
	{
		_conversationHistory.Add(new ChatMessage(ChatRole.User, input));
		return _client.GetStreamingResponseAsync(_conversationHistory, options, token);
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