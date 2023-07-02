namespace HolmesBooking;

public class Reservation
{
    public Guid? Id { get; set; }
    public Customer? Customer { get; set; }
    public Service? Service { get; set; }
    public DateTime? Time { get; set; }
    public State? State { get; set; }
    public int? NumberDiners { get; set; }
    public string? Note { get; set; }

    public Reservation()
    {
    }

    public Reservation(Guid? Id, Customer Customer, Service Service, DateTime Time, State State, int NumberDiners, string Note)
    {
        this.Id = Id;
        this.Customer = Customer;
        this.Service = Service;
        this.Time = Time;
        this.State = State;
        this.NumberDiners = NumberDiners;
        this.Note = Note;
    }

    public override string ToString()
    {
        return $"Id: {Id}, Customer: {Customer}, Service: {Service}, Time: {Time}, State: {State}, NumberDiners: {NumberDiners}, Note: {Note}";
    }

}