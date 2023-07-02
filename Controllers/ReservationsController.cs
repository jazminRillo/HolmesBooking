using HolmesBooking;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("reservations")]
public class ReservationsController : Controller
{
    private readonly ILogger<ServicesController> _logger;
    private ReservationMocks _reservationMocks;

    public ReservationsController(ILogger<ServicesController> logger, ReservationMocks reservationMocks)
    {
        _logger = logger;
        _reservationMocks = reservationMocks;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/new-reservation", Name = "CreateReservation")]
    public IActionResult CreateReservation([FromBody] Reservation reservation)
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
                    // Database.save(reservation)

                    // I did this to test the endpoint by console:
                    _reservationMocks.Reservations!.Add(reservation);
                    Console.WriteLine("Reserva recibida.");
                    foreach (var aux in _reservationMocks.Reservations!)
                    {
                        Console.WriteLine("Id: " + aux.Id + " Nota: " + aux.Note);
                    }
                    return Ok("Reserva creada con éxito!");
                }
                else
                {
                    return BadRequest("Reserva no válida.");
                }
            }
            else
            {
                if (ReservationValidations.IsPresent(_reservationMocks.Reservations!, reservation.Id!.Value))
                {
                    if (ReservationValidations.IsValid(reservation))
                    {
                        Reservation aux = ReservationValidations.GetReservation(_reservationMocks.Reservations!, reservation.Id!.Value);

                        aux.Customer = reservation.Customer;
                        aux.Service = reservation.Service;
                        aux.Time = reservation.Time;
                        aux.NumberDiners = reservation.NumberDiners;
                        aux.Note = reservation.Note;

                        // Database.update(reservation)
                        return Ok("Reserva con id " + reservation.Id + " actualizada con éxito!");
                    }
                    else
                    {
                        return BadRequest("La información proporcionada para actualizar la reserva contiene algún error.");
                    }
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
    public List<Reservation> GetAllReservations()
    {
        try
        {
            return _reservationMocks.Reservations!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IActionResult ShowAllReservations()
    {
        List<Reservation> reservations = GetAllReservations();
        return View("AllReservations", reservations);
    }

    [HttpGet("edit-reservation/{id}", Name = "EditReservation")]
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
            return _reservationMocks.Reservations!.Find(x => x.Id == reservationId)!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("update-reservation", Name = "UpdateReservation")]
    public IActionResult UpdateService(Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        return View("EditReservation", reservation);
    }
}
