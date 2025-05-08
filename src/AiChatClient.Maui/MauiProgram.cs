using AiChatClient.Common;
using AiChatClient.Console;
using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
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
		builder.Services.AddTransient<MCPService>();
		builder.Services.AddTransient<IAmazonBedrockRuntime>(static _ => new AmazonBedrockRuntimeClient(AwsCredentials.AccessKeyId, AwsCredentials.SecretAccessKey, new MobileAmazonBedrockRuntimeConfig(RegionEndpoint.USEast1)));
		builder.Services.AddSingleton<BedrockService>(static serviceProvider =>
		{
			const string modelId = "anthropic.claude-v2";
			var runtime = serviceProvider.GetRequiredService<IAmazonBedrockRuntime>();

			return new(runtime, modelId);
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