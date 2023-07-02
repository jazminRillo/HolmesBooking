using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController : Controller
{
    private readonly ILogger<CustomersController> _logger;
    private readonly CustomerMocks _customerMocks;
    public CustomersController(ILogger<CustomersController> logger, CustomerMocks customerMocks)
    {
        _logger = logger;
        _customerMocks = customerMocks;
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/new-customer", Name = "CreateCustomer")]
    public IActionResult CreateCustomer([FromBody] Customer customer)
    {
        try
        {
            if (CustomerValidations.IsNull(customer))
            {
                return BadRequest("Cliente NULO.");
            }

            if (CustomerValidations.IsNewCustomer(customer))
            {
                if (CustomerValidations.IsValid(_customerMocks.Customers, customer))
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
                if (CustomerValidations.IsPresent(_customerMocks.Customers, customer.Id))
                {
                    if (CustomerValidations.IsValid(_customerMocks.Customers, customer))
                    {
                        Customer aux = CustomerValidations.GetCustomer(_customerMocks.Customers, customer.Id);

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

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-customers", Name = "GetAllCustomers")]
    public List<Customer> GetAllCustomers()
    {
        try
        {
            return _customerMocks.Customers;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public IActionResult ShowAllCustomers()
    {
        List<Customer> customers = GetAllCustomers();
        return View("AllCustomers", customers);
    }

    [HttpGet("edit-customer/{id}", Name = "EditCustomer")]
    public IActionResult EditCustomer(Guid id)
    {
        Customer customer = GetCustomerById(id);

        if (customer == null)
        {
            return NotFound();
        }

        return View("EditCustomer", customer);
    }

    [HttpGet("get-customer/{customerId}", Name = "GetCustomerById")]
    public Customer GetCustomerById(Guid customerId)
    {
        try
        {
            return _customerMocks.Customers.Find(x => x.Id == customerId)!;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("update-customer", Name = "UpdateCustomer")]
    public IActionResult UpdateService(Customer customer)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        return View("EditCustomer", customer);
    }
}
