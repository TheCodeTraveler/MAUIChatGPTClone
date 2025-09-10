using AiChatClient.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AiChatClient.UnitTests;

public class InventoryTests : BaseTest
{
	[Test]
	public async Task TotalInventoryTest_ManuallyParsingData()
	{
		// Arrange
		var inventoryService = new InventoryService();
		var chatOptions = new ChatOptions
		{
			Tools =
			[
				AIFunctionFactory.Create(inventoryService.GetWines)
			]
		};

		List<ChatMessage> chatMessages =
		[
			new(ChatRole.User, "How many bottles of wine do I have in inventory?")
		];

		// Act
		var response = await ChatClient.GetResponseAsync(chatMessages, chatOptions);
		var inventoryCountResponseDigits = new string(response.Text.Where(char.IsDigit).ToArray());
		int.TryParse(inventoryCountResponseDigits, out var winesInInventoryResponse);

		// Assert
		Assert.That(winesInInventoryResponse, Is.EqualTo(inventoryService.GetWines().Count));
	}

	[Test]
	public async Task TotalInventoryTest_EquivalenceEvaluator()
	{
		// Arrange
		var inventoryService = new InventoryService();

		var equivalenceEvaluator = new EquivalenceEvaluator();
		var evaluationContext = new EquivalenceEvaluatorContext($"There are {inventoryService.GetWines().Count} bottles of wine in our inventory");

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
			new(ChatRole.User, "How many bottles of wine do I have in inventory?")
		];

		// Act
		var response = await ChatClient.GetResponseAsync(chatMessages, chatOptions);

		var equivalenceResult = await equivalenceEvaluator.EvaluateAsync(chatMessages, response, chatConfiguration, [evaluationContext]);
		var equivalenceResultMetric = equivalenceResult.Get<NumericMetric>(EquivalenceEvaluator.EquivalenceMetricName);

		// Assert
		Assert.That(equivalenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}

	[Test]
	public async Task TotalInventoryTest_CoherenceEvaluator()
	{
		// Arrange
		var inventoryService = new InventoryService();

		var coherenceEvaluator = new CoherenceEvaluator();

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
			new(ChatRole.User, "How many bottles of wine do I have in inventory?")
		];

		// Act
		var response = await ChatClient.GetResponseAsync(chatMessages, chatOptions);

		var coherenceResult = await coherenceEvaluator.EvaluateAsync(chatMessages, response, chatConfiguration);
		var coherenceResultMetric = coherenceResult.Get<NumericMetric>(CoherenceEvaluator.CoherenceMetricName);

		// Assert
		Assert.That(coherenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}
}