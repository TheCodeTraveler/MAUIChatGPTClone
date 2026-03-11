using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiChatClient.Maui;

public partial class ChatModel(string text, ChatRole role) : ObservableObject
{
	public ChatRole Role { get; } = role;
	
	[ObservableProperty]
	public partial string Text { get; set; } = text;

	[ObservableProperty]
	public partial Stream ImageStream { get; set; } = Stream.Null;

}