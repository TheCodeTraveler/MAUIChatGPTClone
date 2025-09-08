using AiChatClient.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using Trace = System.Diagnostics.Trace;

namespace AiChatClient.Maui;

public partial class ChatViewModel(ChatClientService chatClientService, InventoryService inventoryService) : BaseViewModel
{
	readonly ChatClientService _chatClientService = chatClientService;
	readonly InventoryService _inventoryService = inventoryService;

	[ObservableProperty]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	[ObservableProperty]
	public partial string OutputText { get; private set; } = string.Empty;
	
	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
	async Task SubmitInputText(CancellationToken token)
	{
		var inputText = InputText;

		CanSubmitInputTextExecute = false;
		OutputText = string.Empty;

		var chatOptions = new ChatOptions
		{
			Tools =
			[
				AIFunctionFactory.Create(_inventoryService.GetWines)
			],
		};
		
		try
		{
			await foreach (var response in _chatClientService.GetStreamingResponseAsync(inputText, chatOptions, token).ConfigureAwait(false))
			{
				OutputText = string.Concat(OutputText, response.Text);
			}
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