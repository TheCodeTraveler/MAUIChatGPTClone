using System.ClientModel;
using AiChatClient.Maui;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AiChatClient.UnitTests;

public abstract class BaseTest
{
	protected IChatClient ChatClient { get; private set; } = CreateChatClient();

	[SetUp]
	public virtual void Setup()
	{

	}

	[OneTimeTearDown]
	public virtual void TearDown()
	{
		ChatClient.Dispose();
	}

	static IChatClient CreateChatClient()
	{
		const string modelId = "gpt-4.1-nano";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		var azureOpenAiClient = new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials)
			.GetChatClient(modelId)
			.AsIChatClient();

		return new ChatClientBuilder(azureOpenAiClient)
			.UseFunctionInvocation()
			.Build();
	}
}