using AiChatClient.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AiChatClient.UnitTests;

public class ImageGenerationServiceTests : BaseTest
{
	[Test]
	public async Task GenerateImageAsync_ReturnsNonNullByteArray()
	{
		// Arrange
		var service = new ImageGenerationService(ImageGenerator);

		// Act
		var result = await service.GenerateImageAsync("A simple red circle on a white background", CancellationToken.None);

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result, Is.Not.Empty);
	}

	[Test]
	public async Task GenerateImageAsync_EquivalenceEvaluator()
	{
		// Arrange
		const string prompt = "A simple red circle on a white background";
		var service = new ImageGenerationService(ImageGenerator);

		var equivalenceEvaluator = new EquivalenceEvaluator();
		var evaluationContext = new EquivalenceEvaluatorContext("The image contains a red circle on a white background");
		var chatConfiguration = new ChatConfiguration(ChatClient);

		// Act - Generate the image
		var imageBytes = await service.GenerateImageAsync(prompt, CancellationToken.None);
		Assert.That(imageBytes, Is.Not.Null);

		// Send the image to the chat model for description
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User,
			[
				new DataContent(imageBytes, "image/png"),
				new TextContent("Describe what this image contains")
			])
		};

		var response = await ChatClient.GetResponseAsync(messages);

		var equivalenceResult = await equivalenceEvaluator.EvaluateAsync(messages, response, chatConfiguration, [evaluationContext]);
		var equivalenceResultMetric = equivalenceResult.Get<NumericMetric>(EquivalenceEvaluator.EquivalenceMetricName);

		// Assert
		Assert.That(equivalenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}
}