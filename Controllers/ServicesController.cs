using System.Globalization;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("services")]
public class ServicesController : Controller
{
    private readonly HolmeBookingDbContext _dbContext;

    private readonly ILogger<ServicesController> _logger;
    private readonly ServiceMocks _serviceMocks;

    public ServicesController(HolmeBookingDbContext dbContext, ILogger<ServicesController> logger, ServiceMocks serviceMocks)
    {
        _dbContext = dbContext;
        _logger = logger;
        _serviceMocks = serviceMocks;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/available-services/{Date}", Name = "GetAvailableServicesByDate")]
    public List<Service>? GetAvailableServicesByDate(string Date)
    {
        try
        {
            DateTime date;
            DateTime.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            var dayOfWeek = GetDayOfWeek(date);
            List<Service> Response = new();

            var services = _dbContext.Services
                .AsEnumerable()
                .Where(service => service.IsActive && date >= service.StartDate
                    && date <= service.EndDate
                    && service.Schedule != null
                    && service.Schedule.ContainsKey((int)date.DayOfWeek)
                    && date >= DateTime.Today)
                .ToList();

            Response.AddRange(services);

            return Response;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private int GetDayOfWeek(DateTime date)
    {
        var day = date.DayOfWeek;
        if (day == DayOfWeek.Saturday)
            return 0;
        else
            return (int)day + 1;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-services", Name = "GetAllServices")]
    public IActionResult GetAllServices()
    {
        try
        {
            List<Service> services = _dbContext.Services.ToList();
            return Ok(services);
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
        List<Service> services = _dbContext.Services.ToList();
        return View("AllServices", services);
    }

    [HttpGet("edit-service/{id}", Name = "EditService")]
    public IActionResult EditService(Guid id)
    {
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
            return _dbContext.Services.ToList().Find(x => x.Id == serviceId)!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("update-service", Name = "UpdateService")]
    public IActionResult UpdateService([FromForm] Service service, [FromForm] string[] DeletedDays)
    {
        if (ModelState.IsValid)
        {
            var deletedDayValues = DeletedDays.Select(d => (int)Enum.Parse<DayOfWeek>(d));

            foreach (var day in deletedDayValues)
            {
                service.Schedule.Remove(day);
            }

            var existingService = _dbContext.Services.Find(service.Id);
            if (existingService != null)
            {
                existingService.Name = service.Name;
                existingService.StartDate = service.StartDate;
                existingService.EndDate = service.EndDate;
                existingService.IsActive = service.IsActive;
                existingService.MaxPeople = service.MaxPeople;
                existingService.Schedule = service.Schedule;
                existingService.Description = service.Description;
                _dbContext.SaveChanges();
            }

            return RedirectToAction("ShowAllServices");
        }

        // El modelo no es válido, regresar la vista "EditService" con los errores de validación
        return View("EditService", service);
    }

    [HttpGet("create-new-service", Name = "CreateNewService")]
    public IActionResult CreateNewService()
    {
        return View("CreateService", new Service());
    }

    [HttpPost("create-service", Name = "CreateService")]
    public IActionResult CreateService([FromForm] Service service)
    {
        if (ModelState.IsValid)
        {
            _dbContext.Services.Add(service);
            _dbContext.SaveChanges();

            return RedirectToAction("ShowAllServices");
        }

        return View("CreateService", service);
    }
}
