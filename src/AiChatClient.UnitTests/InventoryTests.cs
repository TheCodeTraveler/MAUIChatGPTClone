using AiChatClient.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AiChatClient.UnitTests;

public class InventoryTests : BaseTest
{
	[Test]
	public async Task TotalInventoryTest()
	{
		// Arrange
		var coherenceEvaluator = new CoherenceEvaluator();
		var inventoryService = new InventoryService();

		var chatConfiguration = new ChatConfiguration(ChatClient);
		var chatOptions = new ChatOptions
		{
			Tools =
			[
				AIFunctionFactory.Create(inventoryService.GetWines)
			]
		};

		List<ChatMessage> chatMessages =
		[
			new ChatMessage(ChatRole.User, "How many bottles of wine do I have in inventory?")
		];

		// Act
		var response = await ChatClient.GetResponseAsync(chatMessages, chatOptions);
		var inventoryCountResponseDigits = new string(response.Text.Where(char.IsDigit).ToArray());
		int.TryParse(inventoryCountResponseDigits, out var winesInInventoryResponse);
		
		var evaluationResult = await coherenceEvaluator.EvaluateAsync(chatMessages, response, chatConfiguration);
		var numericCoherence = evaluationResult.Get<NumericMetric>(CoherenceEvaluator.CoherenceMetricName);

		// Assert
		Assert.Multiple(() =>
		{
			Assert.That(numericCoherence.Value, Is.GreaterThanOrEqualTo(4));
			Assert.That(winesInInventoryResponse, Is.EqualTo(inventoryService.GetWines().Count));
		});

	}
}