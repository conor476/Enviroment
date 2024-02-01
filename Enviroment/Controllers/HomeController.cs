using Microsoft.AspNetCore.Mvc;

namespace Enviroment.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
