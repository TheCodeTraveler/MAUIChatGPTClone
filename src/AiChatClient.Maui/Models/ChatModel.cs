using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.AI;

namespace AiChatClient.Maui;

public sealed partial class ChatModel(string text, ChatRole role) : ObservableObject, IDisposable, IAsyncDisposable
{
	public ChatRole Role { get; } = role;

	public Stream ImageStream => CreateImageStream();

	[ObservableProperty]
	public partial string Text { get; set; } = text;

	[ObservableProperty, NotifyPropertyChangedFor(nameof(ImageStream))]
	public partial byte[]? ImageData { get; set; }

	public void Dispose() => ImageStream.Dispose();

	public ValueTask DisposeAsync() => ImageStream.DisposeAsync();

	partial void OnImageDataChanging(byte[]? value)
	{
		ImageStream.Dispose();
	}

	Stream CreateImageStream() => ImageData is null ? Stream.Null : new MemoryStream(ImageData);
}