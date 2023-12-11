using Microsoft.Playwright;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Tests;

public sealed class PlaywrightFixture : IAsyncLifetime
{
    public IPlaywright Playwright { get; set; }

    public IBrowser Browser { get; set; }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        Browser = await Playwright.Chromium.LaunchAsync();
    }
}
