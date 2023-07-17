using HolmesBooking;

public class AllCustomersViewModel
{
    public List<Customer>? Customers { get; set; }
    public int? Page { get; set; }
    public int? TotalPages { get; set; }
    public Dictionary<Guid, Dictionary<State, int>>? ReservationsByState { get; set; }
}
