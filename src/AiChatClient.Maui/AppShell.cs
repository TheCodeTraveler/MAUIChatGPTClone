using Microsoft.Maui.Controls;

namespace AiChatClient.Maui;

class AppShell: Shell
{
	public AppShell(ChatPage chatPage)
	{
		Items.Add(chatPage);
	}
}