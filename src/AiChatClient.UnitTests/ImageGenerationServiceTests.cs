using AiChatClient.Common;

namespace AiChatClient.UnitTests;

public class ImageGenerationServiceTests : BaseTest
{
	[TestCase("generate an image of a cat")]
	[TestCase("Generate Image of a sunset")]
	[TestCase("create an image of a dog")]
	[TestCase("Create a painting of mountains")]
	[TestCase("draw me a house")]
	[TestCase("Draw a landscape")]
	[TestCase("draw an apple")]
	[TestCase("drawing of a bird")]
	[TestCase("painting of a forest")]
	[TestCase("picture of the ocean")]
	[TestCase("photo of a flower")]
	[TestCase("illustration of a robot")]
	[TestCase("make an image of a tree")]
	[TestCase("make image of a car")]
	[TestCase("make a painting of a river")]
	[TestCase("make a picture of a castle")]
	public void IsImageGenerationRequest_ReturnsTrue_WhenInputContainsImageKeyword(string input)
	{
		// Act
		var result = ImageGenerationService.IsImageGenerationRequest(input);

		// Assert
		Assert.That(result, Is.True);
	}

	[TestCase("What is the weather today?")]
	[TestCase("Tell me a joke")]
	[TestCase("How many bottles of wine do I have?")]
	[TestCase("Summarize this document")]
	[TestCase("")]
	public void IsImageGenerationRequest_ReturnsFalse_WhenInputDoesNotContainImageKeyword(string input)
	{
		// Act
		var result = ImageGenerationService.IsImageGenerationRequest(input);

		// Assert
		Assert.That(result, Is.False);
	}

	[Test]
	public async Task GenerateImageAsync_ReturnsNonNullStream()
	{
		// Arrange
		var service = new ImageGenerationService(ImageGenerator);

		// Act
		var result = await service.GenerateImageAsync("A simple red circle on a white background", CancellationToken.None);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.AreNotEqual(Stream.Null, result);
		Assert.That(result.Length, Is.GreaterThan(0));
	}
}
