using AiChatClient.Console;
using Amazon;
using Amazon.BedrockRuntime;
using Microsoft.Extensions.AI;

IAmazonBedrockRuntime runtime = new AmazonBedrockRuntimeClient(AwsCredentials.AccessKeyId, AwsCredentials.SecretAccessKey, RegionEndpoint.USEast1);
IChatClient client = runtime.AsChatClient();

var chatMessage = new ChatMessage { Text = "Is this working?" };

await foreach (var response in client.GetStreamingResponseAsync(chatMessage, new() { ModelId = "anthropic.claude-v2" }))
{
	Console.WriteLine(response.Text);
}