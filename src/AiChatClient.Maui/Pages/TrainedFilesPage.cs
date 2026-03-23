using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace AiChatClient.Maui;

partial class TrainedFilesPage : BasePage<TrainedFilesViewModel>, IRoutable
{
	public TrainedFilesPage(TrainedFilesViewModel viewModel) : base(viewModel)
	{
		Content = new Grid
		{
			RowSpacing = 12,

			RowDefinitions = Rows.Define(
				(Row.IngestButton, 40),
				(Row.TrainedFilesList, Star),
				(Row.Indicator, 30)),

			Children =
			{
				new Button { BorderColor = Colors.Gray, BorderWidth = 2 }
					.Row(Row.IngestButton)
					.Text("Ingest PDF")
					.Bind(Button.CommandProperty,
						getter: static (TrainedFilesViewModel vm) => vm.PickAndIngestPdfCommand,
						mode: BindingMode.OneTime),

				new CollectionView
					{
						ItemTemplate = new DataTemplate(() => new Label().Bind(Label.TextProperty, "."))
					}
					.Row(Row.TrainedFilesList)
					.Bind(CollectionView.ItemsSourceProperty,
						getter: static (TrainedFilesViewModel viewModel) => viewModel.TrainedFileNames,
						mode: BindingMode.OneWay),

				new ActivityIndicator()
					.Row(Row.Indicator)
					.Bind(IsEnabledProperty,
						getter: static (TrainedFilesViewModel vm) => vm.CanPickAndIngestPdfCommandExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute)
					.Bind(ActivityIndicator.IsRunningProperty,
						getter: static (TrainedFilesViewModel vm) => vm.CanPickAndIngestPdfCommandExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute)
					.Bind(ActivityIndicator.IsVisibleProperty,
						getter: static (TrainedFilesViewModel vm) => vm.CanPickAndIngestPdfCommandExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute)
			}
		};
	}

	public static string Route { get; } = $"{nameof(TrainedFilesPage)}";

	enum Row
	{
		IngestButton,
		TrainedFilesList,
		Indicator
	}
}