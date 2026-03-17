using System.Collections.ObjectModel;
using AiChatClient.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;

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

	public string ImageGenerationModeImageButtonSource => IsImageGenerationMode
															? "palette_filled.png"
															: "palette_outline.png";

	public ObservableCollection<ChatModel> ConversationHistory { get; } = [];

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitInputTextCommand))]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	[ObservableProperty, NotifyPropertyChangedFor(nameof(ImageGenerationModeImageButtonSource))]
	public partial bool IsImageGenerationMode { get; set; } = false;

	public async Task ClearConversationHistory(CancellationToken token)
	{
		CanSubmitInputTextExecute = false;

		try
		{
			await _chatClientService.ClearConversationHistory(token);

			await Task.WhenAll(ConversationHistory.Select(static async x => await x.DisposeAsync()));

			ConversationHistory.Clear();
		}
		finally
		{
			CanSubmitInputTextExecute = true;
		}
	}

	[RelayCommand(CanExecute = nameof(CanSubmitInputTextExecute))]
	void ToggleImageGenerationModeButton()
	{
		IsImageGenerationMode = !IsImageGenerationMode;
	}

	[RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
	async Task SubmitInputText(CancellationToken token)
	{
		var inputText = InputText;
		var isImageGenerationMode = IsImageGenerationMode;

		CanSubmitInputTextExecute = false;

		ConversationHistory.Add(new ChatModel(inputText, ChatRole.User));
		InputText = string.Empty;

		var assistantBubble = new ChatModel(string.Empty, ChatRole.Assistant);
		ConversationHistory.Add(assistantBubble);

		try
		{
			if (isImageGenerationMode)
			{
				await _chatClientService.AddToConversationHistory(new ChatMessage(ChatRole.User, inputText), token).ConfigureAwait(false);

				var image = await _imageGenerationService.GenerateImageAsync(inputText, token).ConfigureAwait(false);
				if (image is not null)
				{
					assistantBubble.ImageData = image;
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