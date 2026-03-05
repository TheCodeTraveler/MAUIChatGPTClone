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
	IFilePicker filePicker) 
	: BaseViewModel
{
	readonly IFilePicker _filePicker = filePicker;
	readonly ChatClientService _chatClientService = chatClientService;
	readonly InventoryService _inventoryService = inventoryService;
	readonly PdfIngestionService _pdfIngestionService = pdfIngestionService;

	public ObservableCollection<string> IngestedFileNames { get; } = [];

	[ObservableProperty]
	public partial string InputText { get; set; } = string.Empty;

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(SubmitInputTextCommand))]
	public partial bool CanSubmitInputTextExecute { get; private set; } = true;

	[ObservableProperty]
	public partial string OutputText { get; private set; } = string.Empty;

	[RelayCommand]
	async Task PickAndIngestPdf(CancellationToken token)
	{
		CanSubmitInputTextExecute = false;

		try
		{
			var result = await _filePicker.PickAsync(new PickOptions
			{
				PickerTitle = "Select a PDF file",
				FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
				{
					[DevicePlatform.Android] = ["application/pdf"],
					[DevicePlatform.iOS] = ["public.pdf"],
					[DevicePlatform.MacCatalyst] = ["public.pdf"],
					[DevicePlatform.WinUI] = [".pdf"],
				})
			}).ConfigureAwait(false);

			if (result is null)
				return;

			using var stream = await result.OpenReadAsync().ConfigureAwait(false);
			await _pdfIngestionService.IngestPdfAsync(stream, result.FileName, token).ConfigureAwait(false);

			IngestedFileNames.Add(result.FileName);
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
			var pdfContext = await _pdfIngestionService.SearchAsync(inputText, token).ConfigureAwait(false);

			var prompt = pdfContext is not null
				? $"Use the following context from ingested documents to answer the question. If the context does not contain the answer, say so.\n\nContext:\n{pdfContext}\n\nQuestion: {inputText}"
				: inputText;

			await foreach (var response in _chatClientService.GetStreamingResponseAsync(prompt, chatOptions, token).ConfigureAwait(false))
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