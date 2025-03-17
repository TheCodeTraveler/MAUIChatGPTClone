using AiChatClient.Common;
using Amazon.BedrockRuntime;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;
using Trace = System.Diagnostics.Trace;

namespace AiChatClient.Maui;

public partial class ChatViewModel(BedrockService bedrockService) : BaseViewModel
{
	readonly BedrockService _bedrockService = bedrockService;

	[ObservableProperty]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty]
	public partial string OutputText { get; private set; } = string.Empty;

	[RelayCommand(IncludeCancelCommand = true, AllowConcurrentExecutions = false, CanExecute = nameof(CanSubmitInputTextExecute))]
	public async Task SubmitInputText(CancellationToken token)
	{
		try
		{
			CanSubmitInputTextExecute = false;
			OutputText = string.Empty;

			await foreach (var response in _bedrockService.GetStreamingResponseAsync(InputText, new(), token).ConfigureAwait(false))
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
			CanSubmitInputTextExecute = true;
		}
	}
}