using System.Globalization;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : Controller
{
    private readonly ILogger<ServicesController> _logger;
    private readonly HolmeBookingDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IConfiguration _configuration;

    public ReservationsController(IConfiguration configuration, IHubContext<NotificationHub> hubContext, ILogger<ServicesController> logger, HolmeBookingDbContext dbContext, IEmailService emailService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _dbContext = dbContext;
        _emailService = emailService;
        _configuration = configuration;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpGet("/days-offline", Name = "DaysOffline")]
    public IActionResult DaysOffline()
    {
        try
        {
            return Ok(_dbContext.DatesNotAvailable.ToList());
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpPost("/save-reservation", Name = "SaveReservation")]
    public IActionResult SaveReservation([FromForm] Reservation reservation)
    {
        var result = SaveReservations(reservation);
        if (result != null && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")! != "Development")
        {
            var culture = new CultureInfo("es-ES");
            var reservationDate = reservation.Time!.Value.ToString("D", culture);
            var subject = "Nueva Reserva";
            var message = "Nueva reserva para el dia: " + reservationDate + ", " + reservation.TimeSelected + " Hs. Para " + reservation.NumberDiners + " personas a nombre de: " + reservation.Customer!.Name;
            if(reservation.Id != null)
            {
                if(reservation.State == State.CANCELADA)
                {
                    subject = "Cancelación Reserva";
                    message = "Cancelación de reserva para el dia: " + reservationDate + ", " + reservation.TimeSelected + " Hs. Para " + reservation.NumberDiners + " personas a nombre de: " + reservation.Customer!.Name;
                }
                else
                {
                    subject = "Actualización Reserva";
                    message = "Actualización de reserva para el dia: " + reservationDate + ", " + reservation.TimeSelected + " Hs. Para " + reservation.NumberDiners + " personas a nombre de: " + reservation.Customer!.Name;
                }
            }
            var recipientEmail = "reservasholmes@gmail.com";
            _hubContext.Clients.All.SendAsync("UpdateLayout", message);
            _emailService.SendEmail(recipientEmail, subject, message);
            var messageOptions = new CreateMessageOptions(
              new PhoneNumber("whatsapp:+5492616149877"));
            messageOptions.From = new PhoneNumber("whatsapp:+14155238886");
            messageOptions.Body = message;
            var whatsapp = MessageResource.Create(messageOptions);
            return Ok(reservation);
        }
        return Ok(reservation);

    }

    [HttpPost("/save-reservation-admin", Name = "SaveReservationAdmin")]
    public IActionResult SaveReservationAdmin([FromForm] Reservation reservation)
    {
        var result = SaveReservations(reservation);
        if (result != null)
        {
            return FilteredReservations(null, null);
        }
        return BadRequest();

    }

    private IActionResult SaveReservations(Reservation reservation)
    {
        try
        {
            if (ReservationValidations.IsNull(reservation))
            {
                return BadRequest("Reserva NULA.");
            }

            if (ReservationValidations.IsNewReservation(reservation))
            {
                if (ReservationValidations.IsValid(reservation))
                {
                    reservation.Service = _dbContext.Services.Find(reservation.Service!.Id);
                    reservation.Customer = _dbContext.Customers.Find(reservation.Customer!.Id);
                    DateTime date = reservation.Time!.Value;
                    TimeSpan time = reservation.TimeSelected!.Value;
                    DateTime combinedDateTime = new(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
                    reservation.Time = combinedDateTime;
                    reservation.State = State.CONFIRMADA;
                    reservation.CreatedDate = DateTime.Now.ToUniversalTime();
                    _dbContext.Reservations.Add(reservation);
                    _dbContext.SaveChanges();
                    SendConfirmation(reservation);

                    return Ok(reservation);
                }
                else
                {
                    return BadRequest("Reserva no válida.");
                }
            }
            else
            {
                var existingReservation = _dbContext.Reservations.Find(reservation.Id!.Value);
                if (existingReservation != null)
                {
                    existingReservation.Customer = _dbContext.Customers.Find(reservation.Customer!.Id);
                    existingReservation.Service = _dbContext.Services.Find(reservation.Service!.Id);
                    DateTime date = reservation.Time!.Value;
                    TimeSpan time = reservation.TimeSelected!.Value;
                    DateTime combinedDateTime = new(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
                    existingReservation.Time = combinedDateTime;
                    existingReservation.NumberDiners = reservation.NumberDiners;
                    existingReservation.Note = reservation.Note;
                    existingReservation.State = reservation.State;
                    existingReservation.NumberKids = reservation.NumberKids;
                    existingReservation.NumberCeliac = reservation.NumberCeliac;
                    existingReservation.Pets = reservation.Pets;
                    reservation.CreatedDate = reservation.CreatedDate.GetValueOrDefault().ToUniversalTime();
                    _dbContext.SaveChanges();
                    if(reservation.State == State.CANCELADA)
                    {
                        SendCancelConfirmation(existingReservation);
                    }
                    return Ok(existingReservation);
                }
                else
                {
                    return BadRequest("No se encontró la reserva solicitada.");
                }

            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task SendConfirmation(Reservation reservation)
    {
        var culture = new CultureInfo("es-ES");
        var reservationDate = reservation.Time!.Value.ToString("D", culture);
        var reservationTime = reservation.TimeSelected;
        var numberOfDiners = reservation.NumberDiners;
        var customerName = reservation.Customer!.Name;
        var customerLastName = reservation.Customer!.Lastname;
        var serviceName = reservation.Service!.Name;
        var serviceDescription = reservation.Service!.Description;

        var editLink = _configuration["FrontUrl"];
        var cancelLink = _configuration["AdminUrl"] + "/reservations/cancel-reservation/" + reservation.Id;

        var message = $"<html>" +
                      $"<body>" +
                      $"<h2>Confirmación de su reserva en Holmes</h2>" +
                      $"<p>Fecha de reserva: {reservationDate}, {reservationTime} Hs.</p>" +
                      $"<p>Personas: {numberOfDiners}</p>" +
                      $"<p>Nombre del cliente: {customerName} {customerLastName}</p>" +
                      $"<p>Servicio seleccionado: {serviceName}</p>" +
                      $"<p>Descripción del servicio: {serviceDescription}</p>" +
                      $"<p>Para editar su reservas puede ingresar a <a href='{editLink}'>Ver mis reservas</a></p>" +
                      $"<p>Si desea cancelar simplemente haga click en este link <a href='{cancelLink}'>Cancelar reserva</a></p>" +
                      $"</body>" +
                      $"</html>";
        var recipientEmail = reservation.Customer.Email!;
        var subject = "Confirmación Reserva en Holmes";
        await _emailService.SendEmail(recipientEmail, subject, message);
    }

    private async Task SendCancelConfirmation(Reservation reservation)
    {
        var culture = new CultureInfo("es-ES");
        var reservationDate = reservation.Time!.Value.ToString("D", culture);
        var reservationTime = reservation.TimeSelected;
        var numberOfDiners = reservation.NumberDiners;
        var customerName = reservation.Customer!.Name;
        var customerLastName = reservation.Customer!.Lastname;
        var serviceName = reservation.Service!.Name;

        var editLink = _configuration["FrontUrl"];

        var message = $"<html>" +
                      $"<body>" +
                      $"<h2>Confirmación de cancelación de su reserva en Holmes</h2>" +
                      $"<p>Fecha de reserva: {reservationDate}, {reservationTime} Hs.</p>" +
                      $"<p>Personas: {numberOfDiners}</p>" +
                      $"<p>Nombre del cliente: {customerName} {customerLastName}</p>" +
                      $"<p>Servicio seleccionado: {serviceName}</p>" +
                      $"<p>Para realizar una nueva reserva puede ingresar a <a href='{editLink}'>Nueva Reserva</a></p>" +
                      $"</body>" +
                      $"</html>";
        var recipientEmail = reservation.Customer.Email!;
        var subject = "Cancelación Reserva en Holmes";
        await _emailService.SendEmail(recipientEmail, subject, message);
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-reservations", Name = "GetAllReservations")]
    public List<Reservation> GetAllReservations(DateTime? date)
    {
        try
        {
            var reservations = _dbContext.Reservations
                                    .Include(r => r.Customer)
                                    .Include(r => r.Service)
                                    .OrderBy(x => x.Time)
                                    .ToList();
            if (date.HasValue)
            {
                return reservations.FindAll(x => x.Time.GetValueOrDefault().Date == date.GetValueOrDefault().Date);
            }

            return reservations;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpGet("/calendarreservations", Name = "CalendarReservations")]
    public IActionResult CalendarReservations()
    {
        try
        {
            var reservations = _dbContext.Reservations
                                    .Include(r => r.Customer)
                                    .Include(r => r.Service)
                                    .OrderBy(x => x.Time)
                                    .Select(x => new
                                    {
                                        reservationId = x.Id.GetValueOrDefault(),
                                        title = x.NumberDiners + " Personas",
                                        start = x.Time,
                                        end = x.Time,
                                        status = x.State,
                                        description = x.Service!.Name + " a nombre de " + x.Customer!.Name + " " + x.Customer!.Lastname
                                    })
                                    .ToList();

            return Ok(reservations);
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("customer-reservations/{customerId}", Name = "CustomerReservations")]
    public IActionResult CustomerReservations(Guid customerId)
    {
        List<Reservation> reservations = _dbContext.Reservations
                                    .Include(r => r.Customer)
                                    .Include(r => r.Service)
                                    .OrderBy(x => x.Time)
                                    .Where(x => x.Customer!.Id == customerId)
                                    .ToList();
        TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

        foreach (var reservation in reservations)
        {
            if (reservation.CreatedDate!.Value.Kind == DateTimeKind.Utc)
            {
                reservation.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(reservation.CreatedDate.GetValueOrDefault(), localTimeZone);
            }
        }
        return Ok(reservations);
    }

    [HttpGet("/filtered-reservations", Name = "FilteredReservations")]
    [Authorize]
    public IActionResult FilteredReservations([FromQuery] string? selectedServices, DateTime? selectedDate)
    {
        if (selectedDate == null) selectedDate = DateTime.Today;
        var selectedServicesIds = ConvertStringToGuidList(selectedServices);
        List<Reservation> reservations = GetAllReservations(selectedDate.GetValueOrDefault(DateTime.Today));
        if (selectedServicesIds.Count > 0)
        {
            reservations = reservations.Where(r => selectedServicesIds.Contains(r.Service!.Id)).ToList();
        }
        TimeZoneInfo localTimeZone = TimeZoneInfo.Local;

        foreach (var reservation in reservations)
        {
            if (reservation.CreatedDate!.Value.Kind == DateTimeKind.Utc)
            {
                reservation.CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(reservation.CreatedDate.GetValueOrDefault(), localTimeZone);
            }
        }
        var model = new AllReservationsViewModel
        {
            Reservations = reservations,
            Services = _dbContext.Services.ToList(),
            SelectedServices = selectedServicesIds,
            SelectedDate = selectedDate,
            TotalNumberDiners = reservations.Sum(reservation => reservation.NumberDiners).GetValueOrDefault(),
            IsOnline = !_dbContext.DatesNotAvailable.Any(x => x.Date == selectedDate)
    };
        return View("AllReservations", model);
    }

    private List<Guid> ConvertStringToGuidList(string? guidString)
    {
        if (string.IsNullOrEmpty(guidString))
            return new List<Guid>();

        var guidArray = guidString.Split(',', StringSplitOptions.RemoveEmptyEntries);
        var guidList = guidArray.Select(x => Guid.Parse(x)).ToList();

        return guidList;
    }

    [HttpGet("cancel-reservation/{id}", Name = "CancelReservation")]
    public IActionResult CancelReservation(Guid id)
    {
        Reservation reservation = GetReservationById(id);

        if (reservation == null)
        {
            return NotFound();
        }
        reservation.State = State.CANCELADA;
        DateTime date = reservation.Time!.Value;
        TimeSpan time = reservation.TimeSelected!.Value;
        DateTime combinedDateTime = new(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
        reservation.Time = combinedDateTime;
        _dbContext.Update(reservation);
        _dbContext.SaveChanges();
        return View("CancelReservation", reservation);
    }

    [HttpGet("edit-reservation/{id}", Name = "EditReservation")]
    [Authorize]
    public IActionResult EditReservation(Guid id)
    {
        Reservation reservation = GetReservationById(id);

        if (reservation == null)
        {
            return NotFound();
        }

        return View("EditReservation", reservation);
    }

    [HttpGet("get-reservation/{reservationId}", Name = "GetReservationById")]
    public Reservation GetReservationById(Guid reservationId)
    {
        try
        {
            Reservation reservation = _dbContext.Reservations.Find(reservationId)!;
            reservation.CustomerOptions = _dbContext.Customers.ToList();
            reservation.ServiceOptions = _dbContext.Services.Where(x => x.IsActive && x.EndDate > DateTime.Today).ToList();
            reservation.Customer = _dbContext.Customers.Find(reservation.Customer!.Id);
            reservation.Service = _dbContext.Services.Find(reservation.Service!.Id);
            reservation.TimeSelected = reservation.Time?.TimeOfDay;
            reservation.Time = reservation.Time?.Date;
            reservation.CreatedDate = reservation.CreatedDate.GetValueOrDefault().ToLocalTime();
            return reservation;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("update-reservation", Name = "UpdateReservation")]
    [Authorize]
    public IActionResult UpdateService(Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        return View("EditReservation", reservation);
    }

    [HttpGet("create-new-reservation", Name = "CreateReservation")]
    [Authorize]
    public IActionResult CreateReservation()
    {
        List<Customer> customers = _dbContext.Customers.ToList();
        List<Service> services = _dbContext.Services.Where(x => x.IsActive && x.EndDate > DateTime.Today).ToList();

        Reservation model = new Reservation
        {
            CustomerOptions = customers,
            ServiceOptions = services
        };
        return View("CreateReservation", model);
    }

    [HttpGet("fullcalendar", Name = "FullCalendar")]
    [Authorize]
    public IActionResult FullCalendar()
    {
      return View("FullCalendar");
    }

    [HttpPost("SetDateAvailableOnline", Name = "SetDateAvailableOnline")]
    public IActionResult SetDateAvailableOnline([FromForm] DateTime selectedDate)
    {
        var datesNotAvailable = _dbContext.DatesNotAvailable;
        var existingDate = datesNotAvailable.FirstOrDefault(d => d.Date == selectedDate);

        if (existingDate != null)
        {
            _dbContext.DatesNotAvailable.Remove(existingDate);
            _dbContext.SaveChanges();
            return Ok("Fecha eliminada de la lista de no disponibles.");
        }
        else
        {
            _dbContext.DatesNotAvailable.Add(new DatesNotAvailable { Date = selectedDate });
            _dbContext.SaveChanges();
            return Ok("Fecha agregada a la lista de no disponibles.");
        }
    }

    [HttpPost("ChangeReservationState", Name = "ChangeReservationState")]
    public IActionResult ChangeReservationState([FromForm] ChangeReservationStateViewModel reservationToBeUpdated)
    {
        var reservation = _dbContext.Reservations.Find(reservationToBeUpdated.id);

        if (reservationToBeUpdated.newState == "CHECKTIME" && reservation!.State == State.CONFIRMADA)
        {
            if (DateTime.Now.Subtract(reservation!.Time.GetValueOrDefault()) >= TimeSpan.FromMinutes(30))
            {
                reservation.State = State.DEMORADA;
            }
            else
            {
                var nochanges = new
                {
                    badgeColor = GetBadgeColor(reservation.State!.Value),
                    stateText = GetStateText(reservation.State.Value)
                };

                return Json(nochanges);
            }
        }
        else
        {
            if (Enum.TryParse(reservationToBeUpdated.newState, out State parsedState))
            {
                reservation!.State = parsedState;
            }
            else
            {
                return BadRequest("Estado de reserva inválido");
            }
        }

        _dbContext.Update(reservation);
        _dbContext.SaveChanges();

        var response = new
        {
            badgeColor = GetBadgeColor(reservation.State.Value),
            stateText = GetStateText(reservation.State.Value)
        };

        return Json(response);
    }

    private string GetBadgeColor(State state)
    {
        return state switch
        {
            State.PRESENTE => "primary",
            State.CONFIRMADA => "success",
            State.CANCELADA => "secondary",
            State.DEMORADA => "warning",
            State.NOSHOW => "danger",
            _ => "info",
        };
    }

    private string GetStateText(State state)
    {
        return state switch
        {
            State.PRESENTE => "Ha llegado",
            State.CONFIRMADA => "Confirmada",
            State.CANCELADA => "Cancelada",
            State.DEMORADA => "Demorada",
            State.NOSHOW => "No Show",
            _ => "Sin confirmar",
        };
    }
}
