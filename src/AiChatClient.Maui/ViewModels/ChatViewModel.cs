using AiChatClient.Common;
using Amazon.BedrockRuntime;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using Trace = System.Diagnostics.Trace;

namespace AiChatClient.Maui;

public partial class ChatViewModel(ChatClientService chatClientService) : BaseViewModel
{
	readonly ChatClientService _chatClientService = chatClientService;

	[ObservableProperty]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string OutputText { get; private set; } = string.Empty;

	[RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
	public async Task SubmitInputText(CancellationToken token)
	{
		var inputText = InputText;

		CanSubmitInputTextExecute = false;
		OutputText = string.Empty;
		
		try
		{
			await foreach (var response in _chatClientService.GetStreamingResponseAsync(inputText, new(), token).ConfigureAwait(false))
			{
				if (response.Text is not null)
				{
					OutputText = string.Concat(OutputText, response.Text);
				}
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