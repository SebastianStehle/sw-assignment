using Codeuctivity.ImageSharpCompare;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Pipeline.Services;
using Pipeline.Services.Steps.ArchiveStep;
using Pipeline.Services.Steps.CopyFile;
using Pipeline.Services.Steps.ExtractMetadata;
using Pipeline.Services.Steps.OptimizeGlb;
using SixLabors.ImageSharp;

namespace Tests;

public class CompressionTests : IClassFixture<PlaywrightFixture>
{
    private readonly LastWorkingDirectory lastWorking = new LastWorkingDirectory();
    private readonly IPipelineRunner sut;
    private readonly PlaywrightFixture _;

    public class LastWorkingDirectory : IPipelineStep
    {
        public FileInfo File { get; private set; } = null!;

        public Task ProcessAsync(FileProcessContext context)
        {
            File = context.WorkingFile;

            return Task.CompletedTask;
        }
    }

    public CompressionTests(PlaywrightFixture playwrightFixture)
    {
        _ = playwrightFixture;

        var serviceProvider = A.Fake<IServiceProvider>();

        A.CallTo(() => serviceProvider.GetService(typeof(IPipelineRunner)))
            .ReturnsLazily(() => sut);

        sut = new PipelineRunner(A.Fake<IDataSource>(),
            new IPipelineStep[]
            {
                new CopyFileStep(),
                new ArchiveStep(serviceProvider),
                new ExtractMetadataStep(A.Fake<ILogger<ExtractMetadataStep>>()),
                new OptimizeGlbStep(),
                lastWorking
            },
            []);
    }

    [Theory]
    [InlineData("Files/hay_chair.glb", "Files/hay_chair.png")]
    [InlineData("Files/hay_chair.zip", "Files/hay_chair.png")]
    public async void Should_compress_file(string sourceFile, string screenshot)
    {
        var originalFile = new FileInfo(sourceFile);
        var originalSize = originalFile.Length;

        var context = new FileProcessContext
        {
            Stream = null!,
            // We are working on the file directly.
            WorkingFile = originalFile,
        };

        context.Metadata[MetadataKeys.Extension] = originalFile.Extension[1..];

        await sut.ProcessInlineAsync(context);

        var compressionRate = (double)lastWorking.File.Length / originalSize;

        Assert.InRange(compressionRate, 0, 0.2);
        await AssertVisualAsync(lastWorking.File.FullName, screenshot);
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