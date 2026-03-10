#pragma warning disable MEAI001 // IImageGenerator is for evaluation purposes only

using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public sealed class ImageGenerationService(IImageGenerator imageGenerator)
{
	readonly IImageGenerator _imageGenerator = imageGenerator;

	static readonly string[] _imageKeywords =
	[
		"generate an image",
		"generate image",
		"create an image",
		"create image",
		"draw me",
		"draw a ",
		"draw an ",
		"picture of",
		"photo of",
		"illustration of",
		"make an image",
		"make image",
		"make a picture",
	];

	public static bool IsImageGenerationRequest(string input)
	{
		return Array.Exists(_imageKeywords, k => input.Contains(k, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<Uri?> GenerateImageAsync(string prompt, CancellationToken token)
	{
		var response = await _imageGenerator.GenerateImagesAsync(prompt, options: null, cancellationToken: token).ConfigureAwait(false);

		var firstImage = response.Contents.OfType<DataContent>().FirstOrDefault();
		return firstImage?.Uri is string uri ? new Uri(uri) : null;
	}
}
