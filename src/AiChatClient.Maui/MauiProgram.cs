using System.ClientModel;
using System.ComponentModel;
using AiChatClient.Common;
using AiChatClient.Common.Models;
using AiChatClient.Maui.Pages;
using Azure.AI.OpenAI;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

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
		builder.Services.AddSingleton<IFilePicker>(static _ => FilePicker.Default);

		builder.Services.AddChatClient(CreateChatClient());
		builder.Services.AddEmbeddingGenerator(CreateEmbeddingGenerator());
		builder.Services.AddSingleton(CreateVectorCollection());

		return builder.Build();
	}

	static IServiceCollection AddTransientWithShellRoute<TView, TViewModel>(this IServiceCollection services)
		where TView : NavigableElement, IRoutable
		where TViewModel : class, INotifyPropertyChanged
	{
		return services.AddTransientWithShellRoute<TView, TViewModel>(TView.Route);
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

	static VectorStoreCollection<string, PdfChunkRecord> CreateVectorCollection()
	{
		const string collectionName = "pdf-chunks";

#if ANDROID || IOS
		// sqlite-vec does not ship Android/iOS native binaries; use in-memory store on mobile
		var vectorStore = new InMemoryVectorStore();
#else
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vectorstore.db");
		var vectorStore = new SqliteVectorStore($"Data Source={dbPath}");
#endif

		return vectorStore.GetCollection<string, PdfChunkRecord>(collectionName);
	}
}