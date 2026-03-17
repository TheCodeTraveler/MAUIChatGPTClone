using System.Diagnostics;

namespace AiChatClient.Maui;

public abstract class BasePage<TViewModel>(TViewModel viewModel, SafeAreaEdges? safeAreaEdges = null) : BasePage(viewModel, safeAreaEdges)
	where TViewModel : BaseViewModel
{
	public new TViewModel BindingContext => (TViewModel)base.BindingContext;
}

public abstract class BasePage : ContentPage
{
	protected BasePage(object? viewModel = null, SafeAreaEdges? safeAreaEdges = null)
	{
		BindingContext = viewModel;
		Padding = 24;

		SafeAreaEdges = safeAreaEdges ?? SafeAreaEdges.Default;

		if (string.IsNullOrWhiteSpace(Title))
		{
			Title = GetType()?.Name;
		}
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		Debug.WriteLine($"OnAppearing: {Title}");
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		Debug.WriteLine($"OnDisappearing: {Title}");
	}
}