using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace TableIT.Functions;

public static class TestFunctions
{
    [FunctionName("TestAuth")]
    public static async Task<IActionResult> TestAuth(
        [HttpTrigger(AuthorizationLevel.User, "get", Route = "authtest")] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("Auth!");
        return new OkObjectResult("Success!");
    }
}
