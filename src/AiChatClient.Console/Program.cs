using AiChatClient.Common;
using AiChatClient.Console;
using Amazon;
using Amazon.BedrockRuntime;

var bedrockService = new BedrockService(
	new AmazonBedrockRuntimeClient(AwsCredentials.AccessKeyId, AwsCredentials.SecretAccessKey, RegionEndpoint.USEast1),
	"anthropic.claude-v2");

await foreach (var response in bedrockService.GetStreamingResponseAsync("Is this working?", new(), CancellationToken.None))
{
	Console.Write(response.Text);
}