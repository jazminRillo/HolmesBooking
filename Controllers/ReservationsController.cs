using HolmesBooking;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("[reservations]")]
public class ReservationsController : ControllerBase
{
    private readonly ILogger<ServicesController> _logger;
    private ReservationMocks rm;

    public ReservationsController(ILogger<ServicesController> logger)
    {
        _logger = logger;
        rm = new ReservationMocks(new CustomerMocks(), new ServiceMocks());
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/new-reservation", Name = "CreateReservation")]
    public IActionResult CreateReservation([FromBody] Reservation? reservation)
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
                    rm.Reservations.Add(reservation);
                    Console.WriteLine("Reserva recibida.");
                    foreach (var aux in rm.Reservations)
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
                if (ReservationValidations.IsPresent(rm.Reservations, (int)reservation.Id))
                {
                    if (ReservationValidations.IsValid(reservation))
                    {
                        Reservation aux = ReservationValidations.GetReservation(rm.Reservations, (int)reservation.Id);

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
        catch (System.Exception)
        {
            throw;
            // Handle error related with DB (?).
        }
    }

    // This endpoint works but it is not updated when you post a new reservation.
    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-reservations", Name = "GetAllReservations")]
    public List<Reservation>? GetAllReservations()
    {
        try
        {
            return rm.Reservations;
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
