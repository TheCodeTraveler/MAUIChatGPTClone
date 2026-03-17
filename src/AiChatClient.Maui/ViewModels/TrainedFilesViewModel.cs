using System.Collections.Immutable;
using System.Text.Json;
using AiChatClient.Common;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Trace = System.Diagnostics.Trace;

namespace AiChatClient.Maui;

public partial class TrainedFilesViewModel(
	TrainedFileNameService trainedFileNameService,
	PdfIngestionService pdfIngestionService,
	IFilePicker filePicker)
	: BaseViewModel
{
	readonly IFilePicker _filePicker = filePicker;
	readonly PdfIngestionService _pdfIngestionService = pdfIngestionService;
	readonly TrainedFileNameService _trainedFileNameService = trainedFileNameService;

	public IReadOnlyList<string> TrainedFileNames => _trainedFileNameService.TrainedFileNames;

	[ObservableProperty, NotifyCanExecuteChangedFor(nameof(PickAndIngestPdfCommand))]
	public partial bool CanPickAndIngestPdfCommandExecute { get; private set; } = true;

	[RelayCommand(CanExecute = nameof(CanPickAndIngestPdfCommandExecute))]
	async Task PickAndIngestPdf(CancellationToken token)
	{
		CanPickAndIngestPdfCommandExecute = false;

		try
		{
			var result = await _filePicker.PickAsync(new PickOptions
			{
				PickerTitle = "Select a PDF file",
				FileTypes = FilePickerFileType.Pdf
			}).WaitAsync(token);

			if (result is null)
				return;

			await using var stream = await result.OpenReadAsync();
			await _pdfIngestionService.IngestPdfAsync(stream, result.FileName, token);

			_trainedFileNameService.AddFilename(result.FileName);
			OnPropertyChanged(nameof(TrainedFileNames));

			await Toast.Make($"Successfully ingested {result.FileName}").Show(token);
		}
		catch (Exception e)
		{
			Trace.TraceError(e.ToString());
		}
		finally
		{
			CanPickAndIngestPdfCommandExecute = true;
		}
	}


}