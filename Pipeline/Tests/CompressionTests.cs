using Codeuctivity.ImageSharpCompare;
using Microsoft.Playwright;
using Pipeline.Services;
using Pipeline.Services.Steps.OptimizeGlb;
using SixLabors.ImageSharp;

namespace Tests;

public class CompressionTests : IClassFixture<PlaywrightFixture>
{
    private readonly OptimizeStep sut = new OptimizeStep();
    private readonly PlaywrightFixture _;

    public CompressionTests(PlaywrightFixture playwrightFixture)
    {
        _ = playwrightFixture;
    }

    [Fact]
    public async void Should_compress_file()
    {
        var originalStream = new FileStream("Files/hay_chair.glb", FileMode.Open);
        var originalSize = originalStream.Length;

        var context = new FileProcessContext
        {
            Stream = originalStream
        };

        context.Metadata[MetadataKeys.Extension] = "glb";

        await sut.ProcessAsync(context);

        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.glb");

        using (var fs = new FileStream(tempFile, FileMode.Create))
        {
            await context.Stream.CopyToAsync(fs);
        }

        var compressionRate = (double)context.Stream.Length / originalSize;

        Assert.InRange(compressionRate, 0, 0.2);
        await AssertVisualAsync(tempFile, "Files/hay_chair.png");
    }

    private async Task AssertVisualAsync(string tempFile, string expectedFile)
    {
        var page = await _.Browser.NewPageAsync();

        await page.GotoAsync("https://gltf-viewer.donmccurdy.com/");

        var fileChooser = await page.RunAndWaitForFileChooserAsync(async () =>
        {
            await page.GetByText("Choose file").ClickAsync();
        });

        var message = await page.RunAndWaitForConsoleMessageAsync(async () =>
        {
            await fileChooser.SetFilesAsync(tempFile);
        },
        new PageRunAndWaitForConsoleMessageOptions
        {
            Predicate = message => message.Text.Contains("Scene") == true
        });

        var expected = await Image.LoadAsync(expectedFile);

        var screenshotBytes = await page.ScreenshotAsync();
        var screenshotStream = new MemoryStream(screenshotBytes);
        var screenshotImage = await Image.LoadAsync(screenshotStream);

        var diff = ImageSharpCompare.CalcDiff(screenshotImage, expected);

        Assert.InRange(diff.MeanError, 0, 10);
    }
}