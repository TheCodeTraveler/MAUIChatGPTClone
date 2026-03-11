using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiChatClient.Maui;

public partial class ChatModel(string text, ChatRole role) : ObservableObject
{
	[ObservableProperty]
	public partial string Text { get; set; } = text;

	[ObservableProperty]
	public partial Stream? ImageStream { get; set; }

	public ChatRole Role { get; } = role;
}