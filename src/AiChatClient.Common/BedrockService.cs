using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public class BedrockService(IAmazonBedrockRuntime runtime, string modelId)
{
	readonly IChatClient _client = runtime.AsIChatClient(modelId);

	public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(string input, ChatOptions options, CancellationToken token) => _client.GetStreamingResponseAsync(input, options, token);
}