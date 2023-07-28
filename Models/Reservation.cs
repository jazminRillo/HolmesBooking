using System.ComponentModel.DataAnnotations.Schema;

namespace HolmesBooking;

[Table("Reservation")]
public class Reservation
{
    public Guid? Id { get; set; }
    public Customer? Customer { get; set; }
    public Service? Service { get; set; }
    public DateTime? Time { get; set; }
    public State? State { get; set; }
    public int? NumberDiners { get; set; }
    public string? Note { get; set; }
    public int? NumberKids { get; set; }
    public int? NumberCeliac { get; set; }
    public DateTime? CreatedDate { get; set; }
    public bool Pets { get; set; }
    [NotMapped]
    public TimeSpan? TimeSelected { get; set; }
    [NotMapped]
    public List<Customer>? CustomerOptions { get; internal set; }
    [NotMapped]
    public List<Service>? ServiceOptions { get; internal set; }

    public Reservation()
    {
    } 

}