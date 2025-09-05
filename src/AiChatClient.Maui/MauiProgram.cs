using System.ClientModel;
using AiChatClient.Common;
using AiChatClient.Console;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using Polly;

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
		builder.Services.AddSingleton<ChatClientService>(static _ =>
		{
			const string modelId = "o3-mini";
			var apiCredentials = new ApiKeyCredential(AzureOpenAiCredentials.ApiKey);
			
			var client = new AzureOpenAIClient(AzureOpenAiCredentials.Endpoint,  apiCredentials).AsChatClient(modelId);

			return new(client);
		});
		
		return builder.Build();
	}

	sealed class MobileAmazonBedrockRuntimeConfig : AmazonBedrockRuntimeConfig
	{
		public MobileAmazonBedrockRuntimeConfig(RegionEndpoint regionEndpoint)
		{
			RegionEndpoint = regionEndpoint;
			RetryMode = RequestRetryMode.Adaptive;
		}
	}
	
	sealed class MobileHttpRetryStrategyOptions : HttpRetryStrategyOptions
	{
		public MobileHttpRetryStrategyOptions()
		{
			BackoffType = DelayBackoffType.Exponential;
			MaxRetryAttempts = 3;
			UseJitter = true;
			Delay = TimeSpan.FromSeconds(2);
		}
	}
}