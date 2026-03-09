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
		var collectionView = (CollectionView)container;

		if (chatModel.Role == ChatRole.User)
			return _userChatTemplate;
		else if (chatModel.Role == ChatRole.Assistant)
			return _assistantChatTemplate;
		else
			throw new NotSupportedException("The current chat role is not yet supported");
	}

	class ChatDataTemplate(ChatRole role) : DataTemplate(() => CreateDataTemplate(role))
	{
		static Grid CreateDataTemplate(ChatRole role) => new()
		{
			Children =
			{
				new Border
				{
					StrokeThickness = 0,
					StrokeShape = new RoundRectangle { CornerRadius = 16 },

					Content = new Label { LineBreakMode = LineBreakMode.WordWrap }
								.TextColor(role == ChatRole.User ? Colors.White : Color.FromArgb("#1C1C1E"))
								.Bind(Label.TextProperty,
									getter: static (ChatModel b) => b.Text)
				}
				.BackgroundColor(role == ChatRole.User ? Color.FromArgb("#007AFF") : Color.FromArgb("#E5E5EA"))
				.Margin(role == ChatRole.User ? new Thickness(60, 0, 0, 0) : new Thickness(0, 0, 150, 0))
				.Padding(12, 8)
				.Bind(Border.HorizontalOptionsProperty,
					getter: static (ChatModel b) => b.Role,
					convert: static (ChatRole role) => role == ChatRole.User ? LayoutOptions.End : LayoutOptions.Start,
					mode: BindingMode.OneTime)
				.Bind(Border.MaximumWidthRequestProperty,
					getter: static (ChatModel b) => b.Role,
					convert: static (ChatRole role) => role == ChatRole.User ? 280d : (double)Border.MaximumWidthRequestProperty.DefaultValue,
					mode: BindingMode.OneTime)
			}
		};
	}
}