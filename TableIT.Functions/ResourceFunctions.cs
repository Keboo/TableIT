using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;

namespace TableIT.Functions;

public static class ResourceFunctions
{
    [FunctionName("Resource")]
    public static async Task<IActionResult> GetResource(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resources/{resourceId}")] HttpRequest req,
        [Blob("resources/{resourceId}", FileAccess.Read, Connection = "BlobConnection")] CloudBlob blob,
        ILogger log)
    {
        try
        {
            if (!await blob.ExistsAsync())
            {
                return new NotFoundResult();
            }

            if (TryGetQueryParameter("width", out int? width) |
                TryGetQueryParameter("height", out int? height))
            {
                using var ms = new MemoryStream();
                await blob.DownloadToStreamAsync(ms);
                ms.Position = 0;
                using var bitmap = SKBitmap.Decode(ms);
                SKImageInfo imageInfo;
                if (width is not null && height is not null)
                {
                    imageInfo = new SKImageInfo(width.Value, height.Value);
                }
                else if (height is not null)
                {
                    int thumbnailWidth = (int)((bitmap.Height / (double)bitmap.Width) * height.Value);
                    imageInfo = new SKImageInfo(thumbnailWidth, height.Value);
                }
                else //if (width is not null)
                {
                    int thumbnailHeight = (int)((bitmap.Width / (double)bitmap.Height) * width.Value);
                    imageInfo = new SKImageInfo(width.Value, thumbnailHeight);
                }
                using var resized = bitmap.Resize(imageInfo, SKFilterQuality.Medium);
                SKData resizedData = resized.Encode(SKEncodedImageFormat.Jpeg, 100);

                return new FileStreamResult(resizedData.AsStream(true), blob.Properties.ContentType)
                {
                    EntityTag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue(blob.Properties.ETag)
                };
            }

            var blobStream = await blob.OpenReadAsync();
            var result = new FileStreamResult(blobStream, blob.Properties.ContentType)
            {
                EntityTag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue(blob.Properties.ETag)
            };
            return result;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error getting resource {0}", req.Query);
            throw;
        }

        bool TryGetQueryParameter(string name, [NotNullWhen(true)] out int? value)
        {
            if (!req.Query.TryGetValue(name, out var stringValue) ||
                !int.TryParse(stringValue, out int intValue))
            {
                value = null;
                return false;
            }
            value = intValue;
            return true;
        }
    }

    [FunctionName("ListResources")]
    public static IActionResult ListResources(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "list/resources")] HttpRequest req,
        [StorageAccount("BlobConnection")] CloudStorageAccount account,
        ILogger log)
    {
        try
        {
            CloudBlobClient blobClient = new(account.BlobEndpoint, account.Credentials);
            var container = blobClient.GetContainerReference("resources");

            List<Resource> resources = new();
            foreach (CloudBlob blob in container.ListBlobs(useFlatBlobListing: true, blobListingDetails: BlobListingDetails.Metadata))
            {
                if (!blob.Metadata.TryGetValue("DisplayName", out string displayName))
                {
                    displayName = blob.Name;
                }
                resources.Add(new Resource(blob.Name, displayName, blob.Properties.ETag));
            }
            return new JsonResult(resources);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error listing resorces");
            throw;
        }
    }

    //TODO: Auth
    [FunctionName("ImportResource")]
    public static async Task<IActionResult> ImportResources(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "import/resources")] HttpRequest req,
            [StorageAccount("BlobConnection")] CloudStorageAccount account,
            ILogger log)
    {
        try
        {
            IFormCollection form = await req.ReadFormAsync();

            if (form.Files.Count == 0) return new NoContentResult();
            IFormFile file = form.Files["file"];

            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("resources");
            CloudBlockBlob blob = container.GetBlockBlobReference(file.FileName + Guid.NewGuid().ToString());
            await blob.UploadFromStreamAsync(file.OpenReadStream());
            blob.Metadata["DisplayName"] = file.FileName;
            blob.SetMetadata();

            return new JsonResult(new Resource(blob.Name, file.FileName, blob.Properties.ETag));
        }
        catch (Exception e)
        {
            log.LogError(e, "Error importing resource");
            throw;
        }
    }

    //TODO: Auth
    [FunctionName("DeleteResource")]
    public static async Task<IActionResult> DeleteResources(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "resources/{resourceId}")] HttpRequest req,
            [Blob("resources/{resourceId}", FileAccess.ReadWrite, Connection = "BlobConnection")] CloudBlob blob,
            ILogger log)
    {
        try
        {
            if (!await blob.DeleteIfExistsAsync())
            {
                return new NotFoundResult();
            }
            return new OkResult();
        }
        catch (Exception e)
        {
            log.LogError(e, "Error deleting resource {0}", blob.Name);
            throw;
        }
    }
}
