using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiChatClient.Maui;

public partial class ChatModel(string text, ChatRole role) : ObservableObject
{
	[ObservableProperty]
	public partial string Text { get; set; } = text;

	public ChatRole Role { get; } = role;
}