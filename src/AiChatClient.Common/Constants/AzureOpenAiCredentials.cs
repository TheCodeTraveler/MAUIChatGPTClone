namespace AiChatClient.Console;

public static class AzureOpenAiCredentials
{
	public const string ApiKey = nameof(ApiKey);
	const string _endPointUrl = nameof(_endPointUrl);
	
    public static Uri Endpoint { get; } = new Uri(_endPointUrl);
}