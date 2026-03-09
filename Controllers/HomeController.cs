using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NGMHS.Models;

namespace NGMHS.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Login", "Account");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var model = new ErrorViewModel
        {
            RequestId = HttpContext.TraceIdentifier
        };

        if (feature is not null)
        {
            ViewData["ErrorPath"] = feature.Path;
        }

        return View(model);
    }
}
