using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolmesBooking;

public class Service
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int? MaxPeople { get; set; }

    [NotMapped]
    public Dictionary<DayOfWeek, List<TimeSpan>> Schedule { get; set; }

    public Service()
    {
        Schedule = new Dictionary<DayOfWeek, List<TimeSpan>>();
    }

    public Service(Guid Id, string Name, DateTime StartDate, DateTime EndDate, bool IsActive, int MaxPeople, Dictionary<DayOfWeek, List<TimeSpan>> Schedule)
    {
        this.Id = Id;
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.IsActive = IsActive;
        this.MaxPeople = MaxPeople;
        this.Schedule = Schedule;
    }
}
