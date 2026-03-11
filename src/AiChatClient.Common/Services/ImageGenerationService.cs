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
		"create a painting",
		"create image",
		"draw me",
		"draw a ",
		"draw an ",
		"drawing of",
		"painting of",
		"picture of",
		"photo of",
		"illustration of",
		"make an image",
		"make image",
		"make a painting",
		"make a picture",
	];

	public static bool IsImageGenerationRequest(string input)
	{
		return Array.Exists(_imageKeywords, k => input.Contains(k, StringComparison.OrdinalIgnoreCase));
	}

	public async Task<Stream?> GenerateImageAsync(string prompt, CancellationToken token)
	{
		var options = new ImageGenerationOptions
		{
			MediaType = "image/png",
			ImageSize = new System.Drawing.Size(1024, 1024),
			Count = 1
		};

		var response = await _imageGenerator.GenerateImagesAsync(prompt, options, cancellationToken: token).ConfigureAwait(false);

		var firstImage = response.Contents.OfType<DataContent>().FirstOrDefault();

		return firstImage?.Base64Data is not null 
				? new MemoryStream(Convert.FromBase64String(firstImage.Base64Data.ToString()))
				: null;
	}
}
