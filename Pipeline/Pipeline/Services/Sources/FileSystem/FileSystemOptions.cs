namespace Pipeline.Services.Sources.FileSystem;

public class FileSystemOptions
{
    required public string SourceFolder { get; set; }

    required public string[] FileExtensions { get; set; } = [".glb", ".gltf"];

    public bool DeleteFiles { get; set; }
}
