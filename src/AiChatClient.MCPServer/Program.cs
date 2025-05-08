using AiChatClient.Common;
using AiChatClient.MCPServer.Services;
using AiChatClient.MCPServer.Tools;
using Octokit;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMcpServer()
	.WithStdioServerTransport()
	.WithTools<GitHubTool>();

builder.Services.AddSingleton<GitHubService>();
builder.Services.AddSingleton<IGitHubClient>(static _ => new GitHubClient(new ProductHeaderValue(nameof(AiChatClient)))
{
	Credentials = new Credentials(GitHubCredentials.Token)
});

var app = builder.Build();

app.Run();
