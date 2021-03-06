using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using TableIT.Functions.ViewModels;

namespace TableIT.Functions;

internal class ClientFunctions
{
    [FunctionName("Viewer")]
    public static IActionResult ViewerLogin(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "viewer")] HttpRequest req,
        ILogger log)
    {
        if (req.Query.TryGetValue("tableid", out var tableId) &&
            !string.IsNullOrWhiteSpace(tableId))
        {
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new());
            viewData.Model = new ViewerViewModel
            {
                TableId = tableId
            };
            return new ViewResult()
            {
                ViewName = "~/Views/Viewer.cshtml",
                ViewData = viewData
            };
        }
        return new ViewResult()
        {
            ViewName = "~/Views/ViewerLogin.cshtml"
        };
    }

    [FunctionName("ViewerRedirect")]
    public static IActionResult ViewerRedirect(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "viewer/{tableid}")] HttpRequest req,
        string tableId) => new RedirectResult($"{req.Scheme}://{req.Host}/api/viewer?tableid={tableId}", true);

    [FunctionName("Upload")]
    [Authorize]
    public static IActionResult UploadView(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "upload")] HttpRequest req)
    {
        return new ViewResult()
        {
            ViewName = "~/Views/Upload.cshtml"
        };
    }
}
