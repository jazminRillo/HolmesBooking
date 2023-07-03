using System;
using System.Globalization;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
                bool IsValid = (service.Schedule!.ContainsKey(Day)) && (Date >= DateTime.Today);

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
    public IActionResult UpdateService([FromForm] Service service)
    {
        if (ModelState.IsValid)
        {
            // Actualizar los datos del servicio en la base de datos
            var existingService = _dbContext.Services.Find(service.Id);
            if (existingService != null)
            {
                existingService.Name = service.Name;
                existingService.StartDate = service.StartDate;
                existingService.EndDate = service.EndDate;
                existingService.IsActive = service.IsActive;
                existingService.MaxPeople = service.MaxPeople;

                // Guardar los cambios en la base de datos
                _dbContext.SaveChanges();
            }

            // Redirigir a la acción "ShowAllServices"
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
            var DayTime = new List<TimeSpan>()
        {
            new TimeSpan(13, 00, 00),
            new TimeSpan(13, 30, 00),
            new TimeSpan(14, 00, 00),
            new TimeSpan(14, 30, 00),
            new TimeSpan(15, 00, 00),
            new TimeSpan(15, 30, 00),
            new TimeSpan(16, 00, 00),

        };
            var DaySchedule = new Dictionary<DayOfWeek, List<TimeSpan>>
            {
            { DayOfWeek.sábado, DayTime },
            { DayOfWeek.domingo, DayTime }
        };
            service.Schedule = DaySchedule;
            _dbContext.Services.Add(service);
            _dbContext.SaveChanges();

            return RedirectToAction("ShowAllServices");
        }

        // Si el modelo no es válido, vuelve a la vista de creación con los errores de validación
        return View("CreateService", service);
    }
}
