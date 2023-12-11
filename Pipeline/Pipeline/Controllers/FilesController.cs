using Microsoft.AspNetCore.Mvc;
using Pipeline.Services.Middlewares.Store;

namespace Pipeline.Controllers;

[ApiController]
[Route("/files/")]
public class FilesController : ControllerBase
{
    private readonly IMetadataStore metadataStore;

    public FilesController(IMetadataStore metadataStore)
    {
        this.metadataStore = metadataStore;
    }

    [HttpGet("", Name = "GetFiles")]
    public async Task<IEnumerable<FileMetadata>> GetAll()
    {
        return await metadataStore.QueryAllAsync();
    }

    [HttpGet("{id}", Name = "GetFile")]
    public async Task<ActionResult> GetFile(Guid id)
    {
        var file = await metadataStore.FindOneAsync(id);

        if (file == null)
        {
            return NotFound();
        }

        return Ok(file);
    }
}
