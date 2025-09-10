using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.AI;

namespace AiChatClient.Maui;

public partial class ChatViewModel : BaseViewModel
{
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

		try
		{
			await Task.Delay(TimeSpan.FromSeconds(1), token);
			OutputText = "Uhh - You haven't implemented any GenAI yet.";
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