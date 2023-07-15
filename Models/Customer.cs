using System.ComponentModel.DataAnnotations.Schema;

namespace HolmesBooking;

[Table("Customer")]
public class Customer
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    [NotMapped]
    public string? Password { get; set; }
    [NotMapped]
    public bool? CalledFromReservation { get; set; }
    public Classification? Classification { get; set; }

    public Customer()
    {
    }

    public Customer(Guid? Id, string Name, string Lastname, string Email, string PhoneNumber, Classification Classification)
    {
        this.Id = Id;
        this.Name = Name;
        this.Lastname = Lastname;
        this.Email = Email;
        this.PhoneNumber = PhoneNumber;
        this.Classification = Classification;
    }

    public Classification GetClassification()
    {
        if (Classification.HasValue)
        {
            return Classification.Value;
        }
        else
        {
            return default;
        }
    }
}
