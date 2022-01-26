using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TableIT.Functions
{
    
    public static class ResourceFunctions
    {
        [FunctionName("Resource")]
        public static async Task<IActionResult> GetResource(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "resources/{resourceId}")] HttpRequest req,
            [Blob("resources/{resourceId}", FileAccess.Read, Connection = "BlobConnection")] CloudBlob blob,
            ILogger log)
        {
            if (!await blob.ExistsAsync())
            {
                return new NotFoundResult();
            }

            var result = new FileStreamResult(await blob.OpenReadAsync(), blob.Properties.ContentType)
            {
                EntityTag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue(blob.Properties.ETag)
            };
            return result;
        }

        [FunctionName("ListResources")]
        public static IActionResult ListResources(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "list/resources")] HttpRequest req,
            [StorageAccount("BlobConnection")] CloudStorageAccount account,
            ILogger log)
        {
            CloudBlobClient blobClient = new(account.BlobEndpoint, account.Credentials);
            var container = blobClient.GetContainerReference("resources");

            List<Resource> resources = new();
            foreach(CloudBlob blob in container.ListBlobs(useFlatBlobListing:true, blobListingDetails:BlobListingDetails.Metadata))
            {
                if (!blob.Metadata.TryGetValue("DisplayName", out string displayName))
                {
                    displayName = blob.Name;
                }
                resources.Add(new Resource(blob.Name, displayName, blob.Properties.ETag));
            }
            return new JsonResult(resources);
        }
    }
}
