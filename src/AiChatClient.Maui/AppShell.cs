namespace AiChatClient.Maui;

partial class AppShell: Shell
{
	public AppShell(ChatPage chatPage)
	{
		Items.Add(chatPage);
	}
}