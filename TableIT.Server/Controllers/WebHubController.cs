using Microsoft.AspNetCore.Mvc;

namespace TableIT.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebHubController : ControllerBase
    {
        private readonly ILogger<WebHubController> _logger;
        private readonly BroadcastHub _hub;

        public WebHubController(ILogger<WebHubController> logger, BroadcastHub hub)
        {
            _logger = logger;
            _hub = hub;
        }

        [HttpPost(Name = "SendMessage")]
        public async Task PostMessage()
        {
            //await _hub.SendAsync("Name", "Message");
        }
    }
}