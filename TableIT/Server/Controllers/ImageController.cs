using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Resources;
using TableIT.Shared.Resources;

namespace TableIT.Server.Controllers;

[ApiController]
[Route("api/image")]
public class ImageController : ControllerBase
{
    public ImageController(BlobContainerClient containerClient)
    {
        ContainerClient = containerClient;
    }

    public BlobContainerClient ContainerClient { get; }

    [HttpGet("{ImageId}")]
    public async Task<IActionResult> Get(string? imageId, int? width = null, int? height = null)
    {
        if (imageId == null)
        {
            return BadRequest();
        }

        BlobClient blobClient = ContainerClient.GetBlobClient(imageId);
        return File(await blobClient.OpenReadAsync(), "image/png");
    }

    [HttpGet("list")]
    public async Task<IActionResult> List()
    {
        List<ImageResource> resources = new();
        await foreach(BlobItem blob in ContainerClient.GetBlobsAsync(BlobTraits.Metadata))
        {
            if (!blob.Metadata.TryGetValue("DisplayName", out string? displayName))
            {
                displayName = blob.Name;
            }
            resources.Add(new ImageResource(blob.Name, displayName, blob.Properties.ETag.ToString() ?? ""));
        }
        return new JsonResult(resources);
    }
}
