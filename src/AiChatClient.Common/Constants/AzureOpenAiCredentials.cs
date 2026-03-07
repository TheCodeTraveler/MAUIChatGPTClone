namespace AiChatClient.Common;

public static class AzureOpenAiCredentials
{
	const string _endPointUrl = nameof(_endPointUrl);

	public const string ApiKey = nameof(ApiKey);
	public static Uri Endpoint { get; } = new Uri(_endPointUrl);
}