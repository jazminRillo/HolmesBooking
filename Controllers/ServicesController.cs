using System.Globalization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("services")]
public class ServicesController : Controller
{
    private readonly ILogger<ServicesController> _logger;
    private readonly ServiceMocks _serviceMocks;

    public ServicesController(ILogger<ServicesController> logger, ServiceMocks serviceMocks)
    {
        _logger = logger;
        _serviceMocks = serviceMocks;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/available-services/{Date}", Name = "GetAvailableServicesByDate")]
    public List<Service>? GetAvailableServicesByDate(DateTime Date)
    {
        try
        {
            CultureInfo culture = new CultureInfo("es-ES");
            DayOfWeek Day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), culture.DateTimeFormat.GetDayName(Date.DayOfWeek));

            List<Service> Response = new List<Service>();

            foreach (var service in _serviceMocks.AvailableServices)
            {
                bool IsInRange = (Date >= service.StartDate) && (Date <= service.EndDate);
                bool IsValid = (service.Schedule!.schedule.ContainsKey(Day)) && (Date >= DateTime.Today);

                if (service.IsActive && IsInRange && IsValid)
                {
                    Response.Add(service);
                }
            }

            return Response;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-services", Name = "GetAllServices")]
    public List<Service> GetAllServices()
    {
        try
        {
            return _serviceMocks.AvailableServices;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("/add-schedule", Name = "AddSchedule")]
    public IActionResult AddSchedule(int serviceId, [FromBody] Schedule schedule)
    {
        return Ok();
    }

    public IActionResult ShowAllServices()
    {
        List<Service> services = GetAllServices();
        return View("AllServices", services);
    }

    [HttpGet("edit-service/{id}", Name = "EditService")]
    public IActionResult EditService(Guid id)
    {
        // Obtener el servicio de la base de datos u otra fuente de datos según el "id"
        Service service = GetServiceById(id);

        if (service == null)
        {
            return NotFound();
        }

        return View("EditService", service);
    }

    [HttpGet("get-service/{serviceId}", Name = "GetServiceById")]
    public Service GetServiceById(Guid serviceId)
    {
        try
        {
            return _serviceMocks.AvailableServices.Find(x => x.Id == serviceId)!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("update-service", Name = "UpdateService")]
    public IActionResult UpdateService(Service service)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        return View("EditService", service);
    }


}
