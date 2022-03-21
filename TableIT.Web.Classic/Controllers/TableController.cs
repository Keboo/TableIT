using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TableIT.Web.Controllers;

[Authorize]
public class TableController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public ActionResult<CreateTableResponse> Create()
    {
        return new CreateTableResponse
        {
            TableId = "TABLE1"
        };
    }
}

public class CreateTableResponse
{
    public string? TableId { get; set; }
}
