using AiChatClient.Common;

namespace AiChatClient.UnitTests;

public class ImageGenerationServiceTests : BaseTest
{
	[Test]
	public async Task GenerateImageAsync_ReturnsNonNullStream()
	{
		// Arrange
		var service = new ImageGenerationService(ImageGenerator);

		// Act
		var result = await service.GenerateImageAsync("A simple red circle on a white background", CancellationToken.None);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.Not.Empty);
	}
}
