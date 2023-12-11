using Codeuctivity.ImageSharpCompare;
using Microsoft.Playwright;
using Pipeline.Services;
using Pipeline.Services.Steps.OptimizeGlb;
using SixLabors.ImageSharp;

namespace Tests;

public class CompressionTests : IClassFixture<PlaywrightFixture>
{
    private readonly OptimizeGlbStep sut = new OptimizeGlbStep();
    private readonly PlaywrightFixture _;

    public CompressionTests(PlaywrightFixture playwrightFixture)
    {
        _ = playwrightFixture;
    }

    [Fact]
    public async void Should_compress_file()
    {
        var originalFile = new FileInfo("Files/hay_chair.glb");
        var originalSize = originalFile.Length;

        var context = new FileProcessContext
        {
            Stream = null!,
            // We are working on the file directly.
            WorkingFile = originalFile,
        };

        context.Metadata[MetadataKeys.Extension] = "glb";

        await sut.ProcessAsync(context);

        var compressionRate = (double)context.WorkingFile.Length / originalSize;

        Assert.InRange(compressionRate, 0, 0.2);
        await AssertVisualAsync(context.WorkingFile.FullName, "Files/hay_chair.png");
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