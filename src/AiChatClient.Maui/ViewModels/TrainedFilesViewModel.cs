using System.Collections.ObjectModel;
using AiChatClient.Common;
using AiChatClient.Common.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Trace = System.Diagnostics.Trace;

namespace AiChatClient.Maui;

public partial class TrainedFilesViewModel(
	PdfIngestionService pdfIngestionService,
	IFilePicker filePicker)
	: BaseViewModel
{
	readonly IFilePicker _filePicker = filePicker;
	readonly PdfIngestionService _pdfIngestionService = pdfIngestionService;

	public IReadOnlyList<EmbeddedPdfModel> IngestedFileNames =>
		[.. _pdfIngestionService.IngestedFileNames.Select(n => new EmbeddedPdfModel(n))];

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

			OnPropertyChanged(nameof(IngestedFileNames));
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
