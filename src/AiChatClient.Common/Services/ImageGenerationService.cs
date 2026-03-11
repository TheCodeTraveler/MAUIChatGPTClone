using System.Buffers;
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

		if (firstImage is null)
			return null;

		// The buffer size is 3 bytes of binary data for every 4 chars of Base64
		var maxDecodedLength = (int)Math.Ceiling(firstImage.Base64Data.Length * 3.0 / 4);
		var rentedByteArray = ArrayPool<byte>.Shared.Rent(maxDecodedLength);
		
		try
		{
			if (Convert.TryFromBase64Chars(firstImage.Base64Data.Span, rentedByteArray, out var bytesWritten))
			{
				var finalBytes = new byte[bytesWritten];
				Array.Copy(rentedByteArray, 0, finalBytes, 0, bytesWritten);
				return new MemoryStream(finalBytes);
			}
			else
			{
				return null;
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(rentedByteArray);
		}
	}
}