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
	PdfIngestionService pdfIngestionService)
	: BaseViewModel
{
	readonly ChatClientService _chatClientService = chatClientService;
	readonly InventoryService _inventoryService = inventoryService;
	readonly PdfIngestionService _pdfIngestionService = pdfIngestionService;

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitInputTextCommand))]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	public ObservableCollection<ChatModel> ConversationHistory { get; } = [];

	public void ClearConversationHistory()
	{
		_chatClientService.ClearConversationHistory();
		ConversationHistory.Clear();
	}

	[RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
	async Task SubmitInputText(CancellationToken token)
	{
		var inputText = InputText;

		CanSubmitInputTextExecute = false;

		ConversationHistory.Add(new ChatModel(inputText, ChatRole.User));

		var assistantBubble = new ChatModel(string.Empty, ChatRole.Assistant);
		ConversationHistory.Add(assistantBubble);

		var chatOptions = new ChatOptions
		{
			Tools =
			[
				AIFunctionFactory.Create(_inventoryService.GetWines)
			],
		};

		try
		{
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

			await foreach (var response in _chatClientService.GetStreamingResponseAsync(prompt, chatOptions, token).ConfigureAwait(false))
			{
				assistantBubble.Text = string.Concat(assistantBubble.Text, response.Text);
			}


			_chatClientService.AddAssistantResponse(assistantBubble.Text);
		}
		catch (Exception e)
		{
			Trace.TraceError(e.ToString());
		}
		finally
		{
			InputText = string.Empty;
			CanSubmitInputTextExecute = true;
		}
	}
}