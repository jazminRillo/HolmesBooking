using System.Globalization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("[services]")]
public class ServicesController : ControllerBase
{
    private readonly ILogger<ServicesController> _logger;
    public ServicesController(ILogger<ServicesController> logger)
    {
        _logger = logger;
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
            ServiceMocks serviceMocks = new ServiceMocks();

            foreach (var service in serviceMocks.AvailableServices)
            {
                bool IsInRange = (Date >= service.StartDate) && (Date <= service.EndDate);
                bool IsValid = (service.Schedule.schedule.ContainsKey(Day)) && (Date >= DateTime.Today);

                if (service.IsActive && IsInRange && IsValid)
                {
                    Response.Add(service);
                }
            }

            return Response;
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-services", Name = "GetAllServices")]
    public List<Service>? GetAllServices()
    {
        try
        {
            ServiceMocks serviceMocks = new ServiceMocks();
            return serviceMocks.AvailableServices;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
