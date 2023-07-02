namespace HolmesBooking;

public class Service
{
    public string? Name { get; set; }

    // TODO:
    /*
    Both StartDate and EndDate are DateTime types, in which you have to specify year, month and day.
    The problem is that the service has to be independent of the year, otherwise the value of year should change automatically.
    */
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int? MaxPeople { get; set; }
    public Schedule Schedule { get; set; }

    public Service()
    {
    }

    public Service(string Name, DateTime StartDate, DateTime EndDate, bool IsActive, int MaxPeople, Schedule Schedule)
    {
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.IsActive = IsActive;
        this.MaxPeople = MaxPeople;
        this.Schedule = Schedule;
    }

    public override string ToString()
    {
        return $"StartDate: {StartDate}, EndDate: {EndDate}, IsActive: {IsActive}, MaxPeople: {MaxPeople}, Schedule: {Schedule}";
    }

}
