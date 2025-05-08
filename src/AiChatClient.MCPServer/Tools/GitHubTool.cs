using AiChatClient.MCPServer.Services;
using ModelContextProtocol.Server;
using Octokit;
using System.ComponentModel;

namespace AiChatClient.MCPServer.Tools;

[McpServerToolType]
class GitHubTool(GitHubService gitHubService)
{
	readonly GitHubService _gitHubService = gitHubService;

	[McpServerTool, Description("Get the README for a specific GitHub Repository")]
	public Task<Repository> GetCurrentTime(string repositoryOwner, string repositoryName) =>
		_gitHubService.GetReadme(repositoryOwner, repositoryName);
}