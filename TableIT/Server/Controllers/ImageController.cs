using Microsoft.AspNetCore.Mvc;

namespace TableIT.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    [HttpGet("{ImageId}")]
    public async Task<IActionResult> Get(string? imageId)
    {
        if (imageId == null)
        {
            return BadRequest();
        }
        await Task.Yield();
        return File(Array.Empty<byte>(), "image/png");
    }
}
