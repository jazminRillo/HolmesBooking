using HolmesBooking;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("[customers]")]
public class CustomersController : ControllerBase
{
    private readonly ILogger<ServicesController> _logger;
    public CustomersController(ILogger<ServicesController> logger)
    {
        _logger = logger;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/new-customer", Name = "CreateCustomer")]
    public IActionResult CreateCustomer([FromBody] Customer? customer)
    {
        CustomerMocks cm = new CustomerMocks();
        try
        {
            if (CustomerValidations.IsNull(customer))
            {
                return BadRequest("Cliente NULO.");
            }

            if (CustomerValidations.IsNewCustomer(customer))
            {
                if (CustomerValidations.IsValid(cm.Customers, customer))
                {
                    // Database.save(reservation)
                    return Ok("Cliente creado con éxito!");
                }
                else
                {
                    return BadRequest("Cliente no válido.");
                }
            }
            else
            {
                if (CustomerValidations.IsPresent(cm.Customers, (int)customer.Id))
                {
                    if (CustomerValidations.IsValid(cm.Customers, customer))
                    {
                        Customer aux = CustomerValidations.GetCustomer(cm.Customers, (int)customer.Id);

                        aux.Name = customer.Name;
                        aux.Lastname = customer.Lastname;
                        aux.Email = customer.Email;
                        aux.PhoneNumber = customer.PhoneNumber;

                        // Database.update(reservation)
                        return Ok("Cliente con id " + customer.Id + " actualizado con éxito!");
                    }
                    else
                    {
                        return BadRequest("La información proporcionada para actualizar al cliente contiene algún error.");
                    }
                }
                else
                {
                    return BadRequest("No se encontró el cliente solicitado.");
                }
            }
        }
        catch (System.Exception)
        {
            throw;
            // Handle error related with DB (?).
        }
    }
}
