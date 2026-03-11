using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiChatClient.Maui;

public sealed partial class ChatModel(string text, ChatRole role) : ObservableObject, IDisposable, IAsyncDisposable
{
	public ChatRole Role { get; } = role;

	[ObservableProperty]
	public partial string Text { get; set; } = text;

	[ObservableProperty]
	public partial Stream ImageStream { get; set; } = Stream.Null;

	public void Dispose() => ImageStream.Dispose();

	public ValueTask DisposeAsync() => ImageStream.DisposeAsync();
}