using CommunityToolkit.Maui.Markup;
using Microsoft.Extensions.AI;
using Microsoft.Maui.Controls.Shapes;

namespace AiChatClient.Maui;

class ChatDataTemplateSelector : DataTemplateSelector
{
	readonly ChatDataTemplate _userChatTemplate = new(ChatRole.User);
	readonly ChatDataTemplate _assistantChatTemplate = new(ChatRole.Assistant);

	protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
	{
		var chatModel = (ChatModel)item;

		if (chatModel.Role == ChatRole.User)
			return _userChatTemplate;
		else if (chatModel.Role == ChatRole.Assistant)
			return _assistantChatTemplate;
		else
			throw new NotSupportedException("The current chat role is not yet supported");
	}

	class ChatDataTemplate(ChatRole role) : DataTemplate(() => CreateDataTemplate(role))
	{
		static Border CreateDataTemplate(ChatRole role)
		{
			var border = new Border
			{
				StrokeThickness = 0,
				StrokeShape = new RoundRectangle { CornerRadius = 16 },
				Padding = new Thickness(12, 8),

				Content = new Label { LineBreakMode = LineBreakMode.WordWrap }
				.Bind(Label.TextProperty,
					getter: static (ChatModel b) => b.Text)
				.Bind(Label.TextColorProperty,
					getter: static (ChatModel b) => b.Role,
					convert: static (ChatRole role) => role == ChatRole.User ? Colors.White : Color.FromArgb("#1C1C1E"),
					mode: BindingMode.OneTime)
			}.Bind(Border.BackgroundColorProperty,
					getter: static (ChatModel b) => b.Role,
					convert: static (ChatRole role) => role == ChatRole.User ? Color.FromArgb("#007AFF") : Color.FromArgb("#E5E5EA"),
					mode: BindingMode.OneTime)
			.Bind(View.HorizontalOptionsProperty,
				getter: static (ChatModel b) => b.Role,
				convert: static (ChatRole role) => role == ChatRole.User ? LayoutOptions.End : LayoutOptions.Start,
				mode: BindingMode.OneTime)
			.Bind(View.MarginProperty,
				getter: static (ChatModel b) => b.Role,
				convert: static (ChatRole role) => role == ChatRole.User ? new Thickness(60, 0, 0, 0) : new Thickness(0, 0, 60, 0),
				mode: BindingMode.OneTime);

			if (role == ChatRole.User)
			{
				border.MaximumWidthRequest = 280;
				border.HorizontalOptions = LayoutOptions.End;
			}
			else if (role == ChatRole.Assistant)
			{
				border.Margin = new Thickness(0, 0, 100, 0);
			}

			return border;
		}
	}
}
