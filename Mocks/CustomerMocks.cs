namespace HolmesBooking;

public class CustomerMocks
{
    public List<Customer> Customers { get; set; }

    public CustomerMocks()
    {
        Customers = new List<Customer> {
        new Customer(0, "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Bronce),
        new Customer(1, "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Plata),
        new Customer(2, "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Oro),
        new Customer(3, "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Platino),
        new Customer(4, "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Bronce),
        };
    }
}