using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace TableIT.Functions;

internal class ClientFunctions
{
    [FunctionName("ViewerLogin")]
    public static async Task<IActionResult> ViewerLogin(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "viewer/login")] HttpRequest req,
        ILogger log)
    {
        return new ViewResult()
        {
            ViewName = "~/Views/Index1.cshtml"
        };
    }
}
