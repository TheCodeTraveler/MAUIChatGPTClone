using Octokit;

namespace AiChatClient.MCPServer.Services;

class GitHubService(IGitHubClient client)
{
	readonly IGitHubClient _gitHubClient = client;

	public Task<Repository> GetReadme(string repositoryOwner, string repositoryName) => _gitHubClient.Repository.Get(repositoryOwner, repositoryName);
}
