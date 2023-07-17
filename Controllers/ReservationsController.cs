using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : Controller
{
    private readonly ILogger<ServicesController> _logger;
    private readonly HolmeBookingDbContext _dbContext;
    private readonly IEmailService _emailService;

    public ReservationsController(ILogger<ServicesController> logger, HolmeBookingDbContext dbContext, IEmailService emailService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _emailService = emailService;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/save-reservation", Name = "SaveReservation")]
    public IActionResult SaveReservation([FromForm] Reservation reservation)
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
                    _dbContext.Reservations.Add(reservation);
                    _dbContext.SaveChanges();
                    if (!User.Identity!.IsAuthenticated)
                    {
                        var message = "Nueva reserva para el dia: " + reservation.Time!.Value.ToLongDateString() + ", " + reservation.TimeSelected + " Hs. Para " + reservation.NumberDiners + " personas a nombre de: " + reservation.Customer!.Name;
                        TempData["NotificationMessage"] = message;
                        var recipientEmail = "holmesbrewery@gmail.com";
                        var subject = "Nueva Reserva";
                        _emailService.SendReservationConfirmationEmail(recipientEmail, subject, message);
                    }
                    return FilteredReservations(null, null);
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
                    _dbContext.SaveChanges();
                    return FilteredReservations(null, null);
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
            // Handle error related with DB (?).
        }
    }

    // This endpoint works but it is not updated when you post a new reservation.
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
        var model = new AllServicesViewModel
        {
            Reservations = reservations,
            Services = _dbContext.Services.ToList(),
            SelectedServices = selectedServicesIds,
            SelectedDate = selectedDate
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
            reservation.ServiceOptions = _dbContext.Services.ToList();
            reservation.Customer = _dbContext.Customers.Find(reservation.Customer.Id);
            reservation.Service = _dbContext.Services.Find(reservation.Service.Id);
            reservation.TimeSelected = reservation.Time?.TimeOfDay;
            reservation.Time = reservation.Time?.Date;
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
        List<Service> services = _dbContext.Services.ToList();

        Reservation model = new Reservation
        {
            CustomerOptions = customers,
            ServiceOptions = services
        };
        return View("CreateReservation", model);
    }

    [HttpPost("ChangeReservationState", Name = "ChangeReservationState")]
    public IActionResult ChangeReservationState([FromForm] ChangeReservationStateViewModel reservationToBeUpdated)
    {
        var reservation = _dbContext.Reservations.Find(reservationToBeUpdated.id);

        if (reservation == null)
        {
            return NotFound();
        }

        if (Enum.TryParse(reservationToBeUpdated.newState, out State parsedState))
        {
            reservation.State = parsedState;
        }
        else
        {
            return BadRequest("Estado de reserva inválido");
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
        switch (state)
        {
            case State.CONFIRMADA:
                return "success";
            case State.CANCELADA:
                return "danger";
            case State.DEMORADA:
                return "warning";
            case State.SIN_CONFIRMAR:
                return "info";
            default:
                return "info";
        }
            
    }

    private string GetStateText(State state)
    {
        switch (state)
        {
            case State.CONFIRMADA:
                return "Confirmada";
            case State.CANCELADA:
                return "Cancelada";
            case State.DEMORADA:
                return "Demorada";
            case State.SIN_CONFIRMAR:
                return "Sin confirmar";
            default:
                return "Sin confirmar";
        }
    }
}
