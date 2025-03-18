using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace AiChatClient.Maui;

class App(AppShell appShell) : Application
{
	readonly AppShell _appShell = appShell;

	protected override Window CreateWindow(IActivationState? activationState) => new(_appShell);
}