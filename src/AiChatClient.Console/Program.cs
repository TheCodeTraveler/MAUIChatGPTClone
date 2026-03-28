using AiChatClient.Common;
using Amazon;
using Amazon.BedrockRuntime;

using var chatClientService = new ChatClientService(
	new AmazonBedrockRuntimeClient(AwsCredentials.AccessKeyId, AwsCredentials.SecretAccessKey, RegionEndpoint.USEast1)
		.AsIChatClient("anthropic.claude-sonnet-4-5-20250929-v1:0"));

await foreach (var response in chatClientService.GetStreamingResponseForUserAsync("Is this working?", new(), CancellationToken.None))
{
	Console.Write(response.Text);
}