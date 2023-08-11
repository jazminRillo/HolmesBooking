using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace HolmesBooking;

[Table("Service")]
public class Service
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int? MaxPeople { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? ImageUrl { get; set; }
    public bool AvailableOnline { get; set; }
    public int? Order { get; set; }

    [Column(TypeName = "json")]
    public Dictionary<int, List<TimeSpan>> Schedule { get; set; }


    public Service()
    {
        Schedule = new Dictionary<int, List<TimeSpan>>();
    }

    public Service(Guid Id, string Name, DateTime StartDate, DateTime EndDate, bool IsActive, int MaxPeople, Dictionary<int, List<TimeSpan>> Schedule)
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


