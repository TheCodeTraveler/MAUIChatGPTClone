using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;

namespace AiChatClient.Maui;

class ChatPage : BasePage<ChatViewModel>
{
	public ChatPage(ChatViewModel chatViewModel) : base(chatViewModel)
	{
		Content = new StackLayout
		{
			Children =
			{
				new ScrollView
				{
					Content = new Label()
						.Top().FillHorizontal()
						.TextTop().TextJustify()
						.Bind(Label.TextProperty,
							getter: static (ChatViewModel vm) => vm.OutputText),
				},

				new Entry { ReturnType = ReturnType.Go }
					.Placeholder("Ask Anything")
					.Center()
					.Behaviors(new UserStoppedTypingBehavior
					{
						MinimumLengthThreshold = 2,
						ShouldDismissKeyboardAutomatically = true,
						StoppedTypingTimeThreshold = 2_000
					}.Bind(UserStoppedTypingBehavior.CommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime))
					.Bind(Entry.TextProperty,
						getter: static (ChatViewModel vm) => vm.InputText,
						setter: static (ChatViewModel vm, string text) => vm.InputText = text)
					.Bind(Entry.ReturnCommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime),

				new Button()
					.Text("Go")
					.Center()
					.Bind(Button.CommandProperty,
						getter: static (ChatViewModel vm) => vm.SubmitInputTextCommand,
						mode: BindingMode.OneTime)
			}
		};
	}
}