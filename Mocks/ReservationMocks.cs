namespace HolmesBooking;

public class ReservationMocks
{
    public List<Reservation>? Reservations { get; set; }

    public ReservationMocks(CustomerMocks cm, ServiceMocks sm)
    {   
        Reservations = new List<Reservation> {
            new Reservation(0, cm.Customers[0], sm.AvailableServices[0], new DateTime(2023, 7, 1), State.CONFIRMADA, 4, "Un celiaco y 2 niños."),
            new Reservation(1, cm.Customers[1], sm.AvailableServices[1], new DateTime(2023, 7, 1), State.CANCELADA, 2, "Sin nota."),
            new Reservation(2, cm.Customers[2], sm.AvailableServices[2], new DateTime(2023, 7, 1), State.PLANIFICADA, 3, "Sin nota."),
            new Reservation(3, cm.Customers[3], sm.AvailableServices[0], new DateTime(2023, 7, 1), State.SIN_CONFIRMAR, 5, "Un celiaco."),
            new Reservation(4, cm.Customers[4], sm.AvailableServices[1], new DateTime(2023, 7, 1), State.CONFIRMADA, 2, "Mesa afuera."),
            new Reservation(5, cm.Customers[0], sm.AvailableServices[2], new DateTime(2023, 7, 1), State.CONFIRMADA, 2, "Mesa adentro."),
        };
    }
}