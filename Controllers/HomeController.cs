using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HolmesBooking.Models;

namespace HolmesBooking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (!User.Identity!.IsAuthenticated)
        {
            string loginUrl = Url.Action("LoginUser", "Users", new { returnUrl = Url.Action("Index", "Home") })!;
            return Redirect(loginUrl!);
        }

        return RedirectToAction("FilteredReservations", "Reservations");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

