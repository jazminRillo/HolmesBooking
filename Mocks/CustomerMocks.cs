namespace HolmesBooking;

public class CustomerMocks
{
    public List<Customer> Customers { get; set; }

    public CustomerMocks()
    {
        Customers = new List<Customer> {
        new Customer(Guid.NewGuid(), "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Bronce),
        new Customer(Guid.NewGuid(), "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Plata),
        new Customer(Guid.NewGuid(), "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Oro),
        new Customer(Guid.NewGuid(), "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Platino),
        new Customer(Guid.NewGuid(), "nombre", "apellido", "usuario@mail.com", "123456789", Classification.Bronce),
        };
    }
}