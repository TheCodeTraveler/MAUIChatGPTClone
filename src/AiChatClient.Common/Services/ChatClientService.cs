using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public class ChatClientService(IChatClient client)
{
	readonly IChatClient _client = client;

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(string input, ChatOptions options, CancellationToken token) => _client.GetStreamingResponseAsync(input, options, token);
}