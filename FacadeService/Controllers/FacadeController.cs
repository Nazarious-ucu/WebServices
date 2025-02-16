using Microsoft.AspNetCore.Mvc;

namespace BasicsMicroservicesLab1.Controllers;

public class FacadeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}