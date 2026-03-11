using System.Collections.ObjectModel;
using AiChatClient.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using Trace = System.Diagnostics.Trace;

namespace AiChatClient.Maui;

public partial class ChatViewModel(
	ChatClientService chatClientService,
	InventoryService inventoryService,
	PdfIngestionService pdfIngestionService,
	ImageGenerationService imageGenerationService)
	: BaseViewModel
{
	readonly ChatClientService _chatClientService = chatClientService;
	readonly InventoryService _inventoryService = inventoryService;
	readonly PdfIngestionService _pdfIngestionService = pdfIngestionService;
	readonly ImageGenerationService _imageGenerationService = imageGenerationService;

	public ObservableCollection<ChatModel> ConversationHistory { get; } = [];

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitInputTextCommand))]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	public async Task ClearConversationHistory(CancellationToken token)
	{
		await _chatClientService.ClearConversationHistory(token);
		ConversationHistory.Clear();
	}

	[RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
	async Task SubmitInputText(CancellationToken token)
	{
		var inputText = InputText;

		CanSubmitInputTextExecute = false;

		ConversationHistory.Add(new ChatModel(inputText, ChatRole.User));
		InputText = string.Empty;

		var assistantBubble = new ChatModel(string.Empty, ChatRole.Assistant);
		ConversationHistory.Add(assistantBubble);

		try
		{
			if (ImageGenerationService.IsImageGenerationRequest(inputText))
			{
				await _chatClientService.AddToConversationHistory(new ChatMessage(ChatRole.User, inputText), token).ConfigureAwait(false);

				var image = await _imageGenerationService.GenerateImageAsync(inputText, token).ConfigureAwait(false);
				if (image is not null)
				{
					assistantBubble.ImageStream = image;
					assistantBubble.Text = "Here's the generated image:";
				}
				else
				{
					assistantBubble.Text = "I was unable to generate an image for you";
				}
			}
			else
			{
				var chatOptions = new ChatOptions
				{
					Tools =
					[
						AIFunctionFactory.Create(_inventoryService.GetWines)
					],
				};

				var pdfContext = await _pdfIngestionService.SearchAsync(inputText, token).ConfigureAwait(false);

				var prompt = pdfContext is not null
					? $"""
						Use the following context from ingested documents to answer the question. 

						Begin your response by stating whether the information stored in the local database contains the answer.

						Context:
						{pdfContext}

						Question:
						{inputText}
						"""
					: inputText;

				await foreach (var response in _chatClientService.GetStreamingResponseForUserAsync(prompt, chatOptions, token).ConfigureAwait(false))
				{
					assistantBubble.Text = string.Concat(assistantBubble.Text, response.Text);
				}
			}

			await _chatClientService.AddToConversationHistory(new ChatMessage(ChatRole.Assistant, assistantBubble.Text), token).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			assistantBubble.Text = $"An error has occurred:\n{e}";
		}
		finally
		{
			CanSubmitInputTextExecute = true;
		}
	}
}