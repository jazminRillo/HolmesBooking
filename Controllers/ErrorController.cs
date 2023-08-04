using Microsoft.AspNetCore.Mvc;

public class ErrorController : Controller
{
    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        var viewName = statusCode.ToString();
        return View(viewName);
    }
}


