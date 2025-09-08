using System.ClientModel;
using AiChatClient.Common;
using AiChatClient.Console;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;

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
			});
#if DEBUG
		builder.Logging.AddDebug();
#endif
		builder.Services.AddSingleton<App>();
		builder.Services.AddSingleton<AppShell>();
		
		// Add Pages + View Models
		builder.Services.AddTransientWithShellRoute<ChatPage, ChatViewModel>(nameof(ChatPage));
		
		// Add Services
		builder.Services.AddSingleton<InventoryService>();
		builder.Services.AddChatClient(CreateChatClient());
		builder.Services.AddSingleton<ChatClientService>();
		
		return builder.Build();
	}

	static IChatClient CreateChatClient()
	{
		const string modelId = "o3-mini";
		var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);

		var azureOpenAiClient = new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint, apiCredentials).GetChatClient(modelId).AsIChatClient();
		return new ChatClientBuilder(azureOpenAiClient).UseFunctionInvocation().Build();
	}
}