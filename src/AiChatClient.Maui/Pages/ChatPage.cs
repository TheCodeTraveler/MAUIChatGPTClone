using AiChatClient.Maui.Pages;
using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;
using Microsoft.Maui.Controls.Shapes;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace AiChatClient.Maui;

partial class ChatPage : BasePage<ChatViewModel>, IRoutable
{
	public ChatPage(ChatViewModel chatViewModel) : base(chatViewModel)
	{
		ToolbarItems.Add(
			new ToolbarItem()
				.Text("Trained Files")
				.Invoke(item => item.Clicked += OnIngestedPdfsToolbarItemClicked));

		Content = new Grid
		{
			RowSpacing = 12,
			ColumnSpacing = 8,

			RowDefinitions = Rows.Define(
				(Row.OutputText, GridLength.Star),
				(Row.InputText, 40),
				(Row.Button, 40),
				(Row.Indicator, 30)),

			ColumnDefinitions = Columns.Define(
				(Col.Input, GridLength.Star),
				(Col.ClearButton, GridLength.Auto)),

			Children =
			{
				new CollectionView
				{
					ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 8 },
					ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
					ItemTemplate = new ChatDataTemplateSelector()
				}
				.Row(Row.OutputText).ColumnSpan(2)
				.Bind(ItemsView.ItemsSourceProperty,
					getter: static (ChatViewModel vm) => vm.ConversationHistory),

				new Entry { ReturnType = ReturnType.Go }
					.Assign(out Entry inputEntry)
					.Row(Row.InputText).Column(Col.Input)
					.Placeholder("Ask Anything")
					.FillHorizontal().Bottom()
					.Behaviors(new UserStoppedTypingBehavior
					{
						MinimumLengthThreshold = 5,
						ShouldDismissKeyboardAutomatically = true,
						StoppedTypingTimeThreshold = 5_000
					}.Bind(UserStoppedTypingBehavior.CommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime)
					 .Bind(UserStoppedTypingBehavior.BindingContextProperty,
						getter: static (Entry inputEntry) => inputEntry.BindingContext,
						mode: BindingMode.OneWay,
						source: inputEntry))
					 .Bind(Entry.TextProperty,
						getter: static (ChatViewModel vm) => vm.InputText,
						setter: static (vm, text) => vm.InputText = text ?? string.Empty)
					 .Bind(Entry.ReturnCommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime),

				new ImageButton { Source = "trash_can.png" }
					.BackgroundColor(Colors.PaleVioletRed)
					.Row(Row.InputText).Column(Col.ClearButton)
					.Center()
					.Invoke(button => button.Clicked += OnClearConversationHistoryButtonClicked)
					.Bind(ImageButton.HeightRequestProperty,
						getter: static (Entry entry) => entry.Height,
						convert: static (double entryHeight) => entryHeight > 0 ? entryHeight * 0.95 : -1,
						source: inputEntry)
					.Bind(ImageButton.MarginProperty,
						getter: static (Entry entry) => entry.Height,
						convert: static (double entryHeight) => entryHeight > 0 ? new Thickness(0, 8, 0, 0) : ImageButton.MarginProperty.DefaultValue,
						source: inputEntry)
					.Bind(IsEnabledProperty,
						getter: static (ChatViewModel vm) => vm.CanSubmitInputTextExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute),

				new Button { BorderColor = Colors.Gray, BorderWidth = 2 }
					.Row(Row.Button).ColumnSpan(2)
					.Text("Go")
					.CenterVertical().FillHorizontal()
					.Margin(0)
					.Bind(Button.CommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime),

				new ActivityIndicator()
					.Row(Row.Indicator).ColumnSpan(2)
					.Bind(IsEnabledProperty,
						getter: static (ChatViewModel vm) => vm.CanSubmitInputTextExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute)
					.Bind(ActivityIndicator.IsRunningProperty,
						getter: static (ChatViewModel vm) => vm.CanSubmitInputTextExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute)
					.Bind(ActivityIndicator.IsVisibleProperty,
						getter: static (ChatViewModel vm) => vm.CanSubmitInputTextExecute,
						convert: static (bool canSubmitInputTextExecute) => !canSubmitInputTextExecute)
			}
		};
	}

	public static string Route { get; } = $"/{nameof(ChatPage)}";

	async void OnClearConversationHistoryButtonClicked(object? sender, EventArgs e)
	{
		var shouldDelete = await DisplayAlertAsync(
			"Start New Conversation",
			"Would you like to delete your conversation history and start over? The conversation cannot be recovered once deleted.",
			"Delete",
			"Cancel");

		if (shouldDelete)
		{
			BindingContext.ClearConversationHistory();
		}
	}

	async void OnIngestedPdfsToolbarItemClicked(object? sender, EventArgs e) =>
		await Shell.Current.GoToAsync(TrainedFilesPage.Route, true);

	enum Row
	{
		OutputText,
		InputText,
		Button,
		Indicator
	}

	enum Col
	{
		Input,
		ClearButton
	}
}