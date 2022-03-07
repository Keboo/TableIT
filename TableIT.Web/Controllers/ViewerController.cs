using Microsoft.AspNetCore.Mvc;
using TableIT.Web.ViewModels.Viewer;

namespace TableIT.Web.Controllers
{
    public class ViewerController : Controller
    {
        public IActionResult Index(string? tableId = null)
        {
            ViewerViewModel vm = new();
            if (tableId is not null)
            {
                vm.TableId = tableId;
            }
            return View(vm);
        }
    }
}
