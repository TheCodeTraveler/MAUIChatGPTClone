using System.ClientModel;
using AiChatClient.Common;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AiChatClient.UnitTests;

public abstract class BaseTest
{
	protected IChatClient ChatClient { get; } = CreateChatClient();
	protected IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator { get; } = CreateEmbeddingGenerator();

	[SetUp]
	public virtual void Setup()
	{

	}

	[OneTimeTearDown]
	public virtual void TearDown()
	{
		ChatClient.Dispose();
		EmbeddingGenerator.Dispose();
	}

	static IChatClient CreateChatClient()
	{
		const string modelId = "gpt-4.1";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		var azureOpenAiClient = new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials)
			.GetChatClient(modelId)
			.AsIChatClient();

		return new ChatClientBuilder(azureOpenAiClient)
			.UseFunctionInvocation()
			.Build();
	}

	static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator()
	{
		const string embeddingModelId = "text-embedding-3-small";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		return new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials)
			.GetEmbeddingClient(embeddingModelId)
			.AsIEmbeddingGenerator();
	}
}