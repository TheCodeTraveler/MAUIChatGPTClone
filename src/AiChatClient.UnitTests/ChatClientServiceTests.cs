using AiChatClient.Common;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.NLP;
using Microsoft.Extensions.AI.Evaluation.Quality;

namespace AiChatClient.UnitTests;

[TestFixture]
public class ChatClientServiceTests : BaseTest
{
	[Test]
	public async Task GetStreamingResponseAsync_ReturnsNonEmptyResponse()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Say hello")
		};
		var options = new ChatOptions();

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseAsync(messages, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		// Assert
		Assert.That(responseText, Is.Not.Null.And.Not.Empty);
	}

	[Test]
	public async Task GetStreamingResponseForUserAsync_ReturnsNonEmptyResponse()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var options = new ChatOptions();

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseForUserAsync("Say hello", options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		// Assert
		Assert.That(responseText, Is.Not.Null.And.Not.Empty);
	}

	[Test]
	public async Task GetStreamingResponseAsync_MaintainsConversationHistory()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var options = new ChatOptions();

		var firstMessages = new List<ChatMessage>
		{
			new(ChatRole.User, "My favorite color is blue. Remember this.")
		};

		// Act - Send first message
		await foreach (var _ in service.GetStreamingResponseAsync(firstMessages, options, CancellationToken.None))
		{
			// Consume the stream
		}

		// Send follow-up that relies on conversation history
		var followUpMessages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is my favorite color?")
		};

		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseAsync(followUpMessages, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		// Assert
		Assert.That(responseText.Contains("blue", StringComparison.OrdinalIgnoreCase), Is.True);
	}

	[Test]
	public async Task ClearConversationHistory_RemovesPreviousContext()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var options = new ChatOptions();

		var firstMessages = new List<ChatMessage>
		{
			new(ChatRole.User, "My secret code word is 'pineapple'. Remember this.")
		};

		// Act - Send first message
		await foreach (var _ in service.GetStreamingResponseAsync(firstMessages, options, CancellationToken.None))
		{
			// Consume the stream
		}

		// Clear history
		await service.ClearConversationHistory(CancellationToken.None);

		// Send follow-up after clearing history
		var followUpMessages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is my secret code word?")
		};

		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseAsync(followUpMessages, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		// Assert - The model should not know the code word after history was cleared
		Assert.That(responseText.Contains("pineapple", StringComparison.OrdinalIgnoreCase), Is.False);
	}

	[Test]
	public void GetStreamingResponseAsync_ThrowsOnCancellation()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Write a very long story about a dragon")
		};
		var options = new ChatOptions();
		using var cts = new CancellationTokenSource();

		// Act & Assert
		Assert.ThrowsAsync<TaskCanceledException>(async () =>
		{
			cts.Cancel();
			await foreach (var _ in service.GetStreamingResponseAsync(messages, options, cts.Token))
			{
				// Should not reach here
			}
		});
	}

	[Test]
	public async Task GetStreamingResponseAsync_CoherenceEvaluator()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var coherenceEvaluator = new CoherenceEvaluator();
		var chatConfiguration = new ChatConfiguration(ChatClient);
		var options = new ChatOptions();

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Explain why the sky is blue in one sentence")
		};

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseAsync(messages, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var coherenceResult = await coherenceEvaluator.EvaluateAsync(messages, response, chatConfiguration);
		var coherenceResultMetric = coherenceResult.Get<NumericMetric>(CoherenceEvaluator.CoherenceMetricName);

		// Assert
		Assert.That(coherenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}

	[Test]
	public async Task GetStreamingResponseForUserAsync_EquivalenceEvaluator()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var equivalenceEvaluator = new EquivalenceEvaluator();
		var evaluationContext = new EquivalenceEvaluatorContext("The capital of France is Paris");
		var chatConfiguration = new ChatConfiguration(ChatClient);
		var options = new ChatOptions();

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is the capital of France?")
		};

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseForUserAsync("What is the capital of France?", options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var equivalenceResult = await equivalenceEvaluator.EvaluateAsync(messages, response, chatConfiguration, [evaluationContext]);
		var equivalenceResultMetric = equivalenceResult.Get<NumericMetric>(EquivalenceEvaluator.EquivalenceMetricName);

		// Assert
		Assert.That(equivalenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}

	[Test]
	public async Task GetStreamingResponseForUserAsync_F1Evaluator()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var f1Evaluator = new F1Evaluator();
		var f1Context = new F1EvaluatorContext("The capital of France is Paris");
		var options = new ChatOptions();

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is the capital of France?")
		};

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseForUserAsync("What is the capital of France?", options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var f1Result = await f1Evaluator.EvaluateAsync(messages, response, new ChatConfiguration(ChatClient), [f1Context]);
		var f1ResultMetric = f1Result.Get<NumericMetric>(F1Evaluator.F1MetricName);

		Assert.That(f1ResultMetric.Value, Is.GreaterThanOrEqualTo(0.85));
	}

	[Test]
	public async Task GetStreamingResponseForUserAsync_BleuEvaluator_French()
	{
		// Arrange
		const string message = "Quelle est la capitale de la France?";

		using var service = new ChatClientService(ChatClient);
		var bleuEvaluator = new BLEUEvaluator();
		var bleuContext = new BLEUEvaluatorContext("La capitale de la France est Paris");
		var options = new ChatOptions();

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, message)
		};

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseForUserAsync(message, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var bleuResult = await bleuEvaluator.EvaluateAsync(messages, response, new ChatConfiguration(ChatClient), [bleuContext]);
		var bleuResultMetric = bleuResult.Get<NumericMetric>(BLEUEvaluator.BLEUMetricName);

		Assert.That(bleuResultMetric.Value, Is.GreaterThanOrEqualTo(0.8));
	}

	[Test]
	public async Task GetStreamingResponseForUserAsync_GleuEvaluator_French()
	{
		// Arrange
		const string message = "Quelle est la capitale de la France?";

		using var service = new ChatClientService(ChatClient);
		var gleuEvaluator = new GLEUEvaluator();
		var gleuContext = new GLEUEvaluatorContext("La capitale de la France est Paris");
		var options = new ChatOptions();

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, message)
		};

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseForUserAsync(message, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var gleuResult = await gleuEvaluator.EvaluateAsync(messages, response, new ChatConfiguration(ChatClient), [gleuContext]);
		var gleuResultMetric = gleuResult.Get<NumericMetric>(GLEUEvaluator.GLEUMetricName);

		Assert.That(gleuResultMetric.Value, Is.GreaterThanOrEqualTo(0.8));
	}

	[Test]
	public async Task MaintainsConversationHistory_EquivalenceEvaluator()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var equivalenceEvaluator = new EquivalenceEvaluator();
		var evaluationContext = new EquivalenceEvaluatorContext("The user's favorite color is blue");
		var chatConfiguration = new ChatConfiguration(ChatClient);
		var options = new ChatOptions();

		var firstMessages = new List<ChatMessage>
		{
			new(ChatRole.User, "My favorite color is blue. Remember this.")
		};

		// Act - Send first message
		await foreach (var _ in service.GetStreamingResponseAsync(firstMessages, options, CancellationToken.None))
		{
			// Consume the stream
		}

		// Send follow-up that relies on conversation history
		var followUpMessages = new List<ChatMessage>
		{
			new(ChatRole.User, "What is my favorite color?")
		};

		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseAsync(followUpMessages, options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var equivalenceResult = await equivalenceEvaluator.EvaluateAsync(followUpMessages, response, chatConfiguration, [evaluationContext]);
		var equivalenceResultMetric = equivalenceResult.Get<NumericMetric>(EquivalenceEvaluator.EquivalenceMetricName);

		// Assert
		Assert.That(equivalenceResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}

	[Test]
	public async Task AddToConversationHistory_CompletenessEvaluator()
	{
		// Arrange
		using var service = new ChatClientService(ChatClient);
		var completenessEvaluator = new CompletenessEvaluator();
		var completenessContext = new CompletenessEvaluatorContext(
			"A greeting that uses pirate-themed language such as 'ahoy', 'matey', 'arr', or similar pirate vocabulary");
		var chatConfiguration = new ChatConfiguration(ChatClient);
		var options = new ChatOptions();

		await service.AddToConversationHistory(
			new ChatMessage(ChatRole.System, "You are a pirate. Always respond in pirate speak."),
			CancellationToken.None);

		var messages = new List<ChatMessage>
		{
			new(ChatRole.User, "Say hello")
		};

		// Act
		var responseText = string.Empty;
		await foreach (var update in service.GetStreamingResponseForUserAsync("Say hello", options, CancellationToken.None))
		{
			responseText += update.Text;
		}

		var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
		var completenessResult = await completenessEvaluator.EvaluateAsync(messages, response, chatConfiguration, [completenessContext]);
		var completenessResultMetric = completenessResult.Get<NumericMetric>(CompletenessEvaluator.CompletenessMetricName);

		// Assert
		Assert.That(completenessResultMetric.Value, Is.GreaterThanOrEqualTo(4));
	}
}
