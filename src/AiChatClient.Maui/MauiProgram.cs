using System.ClientModel;
using System.ComponentModel;
using System.Runtime.Versioning;
using AiChatClient.Common;
using AiChatClient.Common.Models;
using Azure.AI.OpenAI;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.Maui.Essentials.AI;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OllamaSharp;

namespace AiChatClient.Maui;

static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder()
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.UseMauiCommunityToolkitMarkup()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
			.ConfigureMauiHandlers(static handlers =>
			{
#if IOS || MACCATALYST
				handlers.AddHandler<CollectionView, CollectionViewNoScrollBarsHandler>();
#endif
			});


#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddSingleton<App>();
		builder.Services.AddSingleton<AppShell>();

		// Add Pages + View Models
		builder.Services.AddTransientWithShellRoute<ChatPage, ChatViewModel>();
		builder.Services.AddTransientWithShellRoute<TrainedFilesPage, TrainedFilesViewModel>();

		// Add Services
		builder.Services.AddSingleton<InventoryService>();
		builder.Services.AddSingleton<ChatClientService>();
		builder.Services.AddSingleton<PdfIngestionService>();
		builder.Services.AddSingleton<ImageGenerationService>();
		builder.Services.AddSingleton<TrainedFileNameService>();
		builder.Services.AddSingleton<IFilePicker>(static _ => FilePicker.Default);
		builder.Services.AddSingleton<IPreferences>(static _ => Preferences.Default);
		builder.Services.AddSingleton<IDeviceDisplay>(static _ => DeviceDisplay.Current);

		builder.Services.AddChatClient(static _ => (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26)) 
		                                           && DeviceInfo.Current.DeviceType == DeviceType.Physical
														? CreateAppleIntelligenceChatClient()
														: CreateOllamaChatClient());
		
		builder.Services.AddEmbeddingGenerator(static _ => (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13,1))
		                                                   && DeviceInfo.Current.DeviceType == DeviceType.Physical
																? CreateAppleEmbeddingGenerator()
																: CreateOllamaEmbeddingGenerator());

		builder.Services.AddImageGenerator(static _ => CreateAzureOpenAiImageGenerator());
		builder.Services.AddKeyedSingleton<VectorStoreCollection<string, PdfChunkRecord>>(
			"PdfVectorStore", static (_, _) => CreateVectorCollection());

		return builder.Build();
	}

	static IServiceCollection AddTransientWithShellRoute<TView, TViewModel>(this IServiceCollection services)
		where TView : NavigableElement, IRoutable
		where TViewModel : class, INotifyPropertyChanged
	{
		return services.AddTransientWithShellRoute<TView, TViewModel>(TView.Route);
	}

	static IChatClient CreateAzureOpenAiChatClient()
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

	static IChatClient CreateOllamaChatClient()
	{
		const string modelId = "qwen3.5";

		var ollamaClient = new OllamaApiClient(GetOllamaEndpoint(), modelId);

		return new ChatClientBuilder(ollamaClient)
			.UseFunctionInvocation()
			.Build();
	}

	[SupportedOSPlatform("iOS26.0")]
	[SupportedOSPlatform("macOS26.0")]
	[SupportedOSPlatform("MacCatalyst26.0")]
	static IChatClient CreateAppleIntelligenceChatClient() =>
#if IOS || MACCATALYST
		new AppleIntelligenceChatClient();
#else
		throw new NotSupportedException("AppleIntelligenceChatClient is not supported on the current operating system");
#endif

	[SupportedOSPlatform("iOS13.0")]
	[SupportedOSPlatform("macOS10.15")]
	[SupportedOSPlatform("MacCatalyst13.1")]
	static IEmbeddingGenerator<string, Embedding<float>> CreateAppleEmbeddingGenerator() =>
#if IOS || MACCATALYST
		new NLEmbeddingGenerator();
#else
		throw new NotSupportedException("NLEmbeddingGenerator is not supported on the current operating system");
#endif

	static IEmbeddingGenerator<string, Embedding<float>> CreateAzureOpenAiEmbeddingGenerator()
	{
		const string embeddingModelId = "text-embedding-3-small";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		return new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials)
			.GetEmbeddingClient(embeddingModelId)
			.AsIEmbeddingGenerator();
	}

	static IImageGenerator CreateAzureOpenAiImageGenerator()
	{
		const string imageModelId = "gpt-image-1.5";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		return new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials)
			.GetImageClient(imageModelId)
			.AsIImageGenerator();
	}

	static IEmbeddingGenerator<string, Embedding<float>> CreateOllamaEmbeddingGenerator()
	{
		const string embeddingModelId = "qwen3-embedding";

		return new OllamaApiClient(GetOllamaEndpoint(), embeddingModelId);
	}

	static VectorStoreCollection<string, PdfChunkRecord> CreateVectorCollection()
	{
		const string collectionName = "pdf-chunks";

#if ANDROID || IOS || MACCATALYST
		// sqlite-vec does not ship Android/iOS native binaries; use in-memory store on mobile
		var vectorStore = new InMemoryVectorStore();
#else
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vectorstore.db");
		var vectorStore = new SqliteVectorStore($"Data Source={dbPath}");
#endif

		return vectorStore.GetCollection<string, PdfChunkRecord>(collectionName);
	}

	static string GetOllamaEndpoint()
	{
#if ANDROID
		return "http://10.0.2.2:11434";
#else
		return "http://127.0.0.1:11434";
#endif
	}
}