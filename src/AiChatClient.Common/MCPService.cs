using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

namespace AiChatClient.Common;

public class MCPService : IAsyncDisposable
{
	IMcpClient? _client;

	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (_client is not null)
			await _client.DisposeAsync();
	}

	public async Task<IList<McpClientTool>> GetTools(CancellationToken token)
	{
		var client = await GetClient(token).ConfigureAwait(false);
		return await client.ListToolsAsync(cancellationToken: token);
	}

	async ValueTask<IMcpClient> GetClient(CancellationToken token)
	{
		return _client ??= await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions()
		{
			Name = nameof(AiChatClient),
			Command = "dotnet",
			Arguments = ["run", "--project", $"../../../../{nameof(AiChatClient)}.MCPServer", "--no-build"]
		}), cancellationToken: token);
	}
}
