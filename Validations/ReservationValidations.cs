using HolmesBooking;

namespace HolmesBooking;

public class ReservationValidations
{
    public static bool IsNull(Reservation? r)
    {
        return r == null;
    }

    public static bool IsNewReservation(Reservation r)
    {
        return r.Id == null;
    }

    public static bool IsValid(Reservation r)
    {
        // Necessary validations?
        return true;
    }

    public static bool IsPresent(List<Reservation> reservations, int Id)
    {
        Reservation? r = reservations.Find(x => x.Id == Id);
        return r != null;
    }

    public static Reservation GetReservation(List<Reservation> reservations, int Id)
    {
        return reservations.Find(x => x.Id == Id);
    }
}