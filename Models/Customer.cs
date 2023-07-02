namespace HolmesBooking;

public class Customer
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
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

}
