using Microsoft.AspNetCore.Mvc;

namespace SiteNamorada.Controllers;

public class HomeController : Controller
{
    public IActionResult Painel()
    {
        return View();
    }
}