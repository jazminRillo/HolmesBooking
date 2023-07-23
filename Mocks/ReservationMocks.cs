namespace HolmesBooking;

public class ReservationMocks
{
    public List<Reservation>? Reservations { get; set; }

    public ReservationMocks(CustomerMocks cm, ServiceMocks sm)
    {   
        Reservations = new List<Reservation> {
            new Reservation(Guid.NewGuid(), cm.Customers[0], sm.AvailableServices[0], new DateTime(2023, 7, 1), State.CONFIRMADA, 4, "Un celiaco y 2 niños."),
            new Reservation(Guid.NewGuid(), cm.Customers[1], sm.AvailableServices[1], new DateTime(2023, 7, 1), State.CANCELADA, 2, "Sin nota."),
            new Reservation(Guid.NewGuid(), cm.Customers[4], sm.AvailableServices[1], new DateTime(2023, 7, 1), State.CONFIRMADA, 2, "Mesa afuera."),
            new Reservation(Guid.NewGuid(), cm.Customers[0], sm.AvailableServices[2], new DateTime(2023, 7, 1), State.CONFIRMADA, 2, "Mesa adentro."),
        };
    }
}