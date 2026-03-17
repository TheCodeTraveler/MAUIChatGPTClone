using Microsoft.Extensions.AI;

namespace AiChatClient.Common;

public sealed class ImageGenerationService(IImageGenerator imageGenerator)
{
	readonly IImageGenerator _imageGenerator = imageGenerator;

	public async Task<byte[]?> GenerateImageAsync(string prompt, CancellationToken token)
	{
		var options = new ImageGenerationOptions
		{
			MediaType = "image/png",
			ImageSize = new System.Drawing.Size(1024, 1024),
			Count = 1
		};

		var response = await _imageGenerator.GenerateImagesAsync(prompt, options, cancellationToken: token).ConfigureAwait(false);

		var firstImage = response.Contents.OfType<DataContent>().FirstOrDefault();

		return firstImage?.Data.ToArray();
	}
}