using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using static CommunityToolkit.Maui.Markup.GridRowsColumns;

namespace AiChatClient.Maui;

partial class ChatPage : BasePage<ChatViewModel>
{
	public ChatPage(ChatViewModel chatViewModel) : base(chatViewModel)
	{
		Content = new Grid
		{
			RowSpacing = 12,
			
			RowDefinitions = Rows.Define(
				(Row.OutputText, GridLength.Star),
				(Row.InputText, 40),
				(Row.Button, 40),
				(Row.Indicator, 20)),
			
			Children =
			{
				new ScrollView
				{
					Content = new Label()
						.Top().FillHorizontal()
						.TextTop().TextJustify()
						.Bind(Label.TextProperty,
							getter: static (ChatViewModel vm) => vm.OutputText),
				}.Row(Row.OutputText),

				new Entry { ReturnType = ReturnType.Go }
					.Assign(out Entry inputEntry)
					.Row(Row.InputText)
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

				new Button { BorderColor = Colors.Gray, BorderWidth = 2 }
					.Row(Row.Button)
					.Text("Go")
					.Center()
					.Bind(Button.CommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime)
					.Bind(WidthRequestProperty,
						getter: static inputEntry => inputEntry.Width,
						source: inputEntry),
				
				new ActivityIndicator()
					.Row(Row.Indicator)
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

	enum Row
	{
		OutputText,
		InputText,
		Button,
		Indicator
	}
}