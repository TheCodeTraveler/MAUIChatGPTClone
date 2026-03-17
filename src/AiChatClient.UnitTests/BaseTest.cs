using System.ClientModel;
using AiChatClient.Common;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AiChatClient.UnitTests;

public abstract class BaseTest
{
	readonly Lazy<IChatClient> _chatClientHolder = new(CreateChatClient);
	readonly Lazy<IEmbeddingGenerator<string, Embedding<float>>> _embeddingGeneratorHolder = new(CreateEmbeddingGenerator);
	readonly Lazy<IImageGenerator> _imageGeneratorHolder = new(CreateImageGenerator);

	protected IChatClient ChatClient => _chatClientHolder.Value;
	protected IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator => _embeddingGeneratorHolder.Value;
	protected IImageGenerator ImageGenerator => _imageGeneratorHolder.Value;

	[SetUp]
	public virtual void Setup()
	{

	}

	[OneTimeTearDown]
	public virtual void TearDown()
	{
		if (_chatClientHolder.IsValueCreated)
			ChatClient.Dispose();

		if (_embeddingGeneratorHolder.IsValueCreated)
			EmbeddingGenerator.Dispose();

		if (_imageGeneratorHolder.IsValueCreated)
			(ImageGenerator as IDisposable)?.Dispose();
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

	static IImageGenerator CreateImageGenerator()
	{
		const string imageModelId = "gpt-image-1.5";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		return new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials)
			.GetImageClient(imageModelId)
			.AsIImageGenerator();
	}
}