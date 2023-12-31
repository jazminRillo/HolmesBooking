using System.Globalization;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("services")]
public class ServicesController : Controller
{
    private readonly HolmeBookingDbContext _dbContext;

    public ServicesController(HolmeBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
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
                    && service.AvailableOnline
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
    [Authorize]
    [HttpGet("/all-services", Name = "GetAllServices")]
    public IActionResult GetAllServices()
    {
        try
        {
            List<Service> services = _dbContext.Services.OrderBy(x => x.Order).ToList();
            if (User.Identity!.IsAuthenticated)
            {
                return View("AllServices", services);
            }
            return Ok(services);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpGet("/all-active-services", Name = "GetAllActiveServices")]
    public IActionResult GetAllActiveServices()
    {
        try
        {
            List<Service> services = _dbContext.Services
                .Where(x => x.IsActive
                && x.AvailableOnline
                && x.EndDate > DateTime.Today).OrderBy(x => x.Order).ToList();
            return Ok(services);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet("edit-service/{id}", Name = "EditService")]
    [Authorize]
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
                existingService.ShortDescription = service.ShortDescription;
                existingService.ImageUrl = service.ImageUrl;
                existingService.AvailableOnline = service.AvailableOnline;
                _dbContext.SaveChanges();
            }

            return RedirectToAction("GetAllServices");
        }

        // El modelo no es válido, regresar la vista "EditService" con los errores de validación
        return View("EditService", service);
    }

    [HttpGet("create-new-service", Name = "CreateNewService")]
    [Authorize]
    public IActionResult CreateNewService()
    {
        return View("CreateService", new Service());
    }

    [HttpPost("create-service", Name = "CreateService")]
    [Authorize]
    public IActionResult CreateService([FromForm] Service service)
    {
        if (ModelState.IsValid)
        {
            _dbContext.Services.Add(service);
            _dbContext.SaveChanges();

            return RedirectToAction("GetAllServices");
        }

        return View("CreateService", service);
    }

    [HttpGet("update-service-order/{serviceId}/{newPosition}", Name = "UpdateServiceOrder")]
    [Authorize]
    public IActionResult UpdateServiceOrder(Guid serviceId, int newPosition)
    {
        var service = _dbContext.Services.Find(serviceId);
        if (service != null)
        {
            service.Order = newPosition;
            _dbContext.SaveChanges();
        }

        return Ok();
    }
}
