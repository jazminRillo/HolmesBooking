using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController : Controller
{
    private readonly ILogger<CustomersController> _logger;
    private readonly HolmeBookingDbContext _dbContext;
    public CustomersController(ILogger<CustomersController> logger, HolmeBookingDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    [HttpGet("create-new-customer", Name = "CreateCustomer")]
    public IActionResult CreateCustomer()
    {
        return View("CreateCustomer", new Customer());
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/save-customer", Name = "SaveCustomer")]
    public IActionResult SaveCustomer([FromForm] Customer customer)
    {
        try
        {
            if (CustomerValidations.IsNull(customer))
            {
                return BadRequest("Cliente NULO.");
            }

            if (CustomerValidations.IsNewCustomer(customer))
            {
                if (CustomerValidations.IsValid(_dbContext.Customers.ToList(), customer))
                {
                    _dbContext.Customers.Add(customer);
                    _dbContext.SaveChanges();
                    return View("AllCustomers", _dbContext.Customers.ToList());
                }
                else
                {
                    return BadRequest("Cliente no válido.");
                }
            }
            else
            {
                var existingCustomer = _dbContext.Customers.Find(customer.Id);
                if (existingCustomer != null)
                {
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Lastname = customer.Lastname;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.PhoneNumber = customer.PhoneNumber;
                    _dbContext.SaveChanges();
                }
                return View("AllCustomers", _dbContext.Customers.ToList());
                //revisar
                /*if (CustomerValidations.IsValid(_dbContext.Customers.ToList(), customer)) 
                {
                    var existingCustomer = _dbContext.Customers.Find(customer.Id);
                    if (existingCustomer != null)
                    {
                        existingCustomer.Name = customer.Name;
                        existingCustomer.Lastname = customer.Lastname;
                        existingCustomer.Email = customer.Email;
                        existingCustomer.PhoneNumber = customer.PhoneNumber;
                        _dbContext.SaveChanges();
                    }
                    return Ok("Cliente con id " + customer.Id + " actualizado con éxito!");
                }
                else
                {
                    return BadRequest("La información proporcionada para actualizar al cliente contiene algún error.");
                }*/
            }
        }
        catch (Exception)
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
            return _dbContext.Customers.ToList();
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

    [HttpGet("get-customer/{id}", Name = "GetCustomerById")]
    public Customer GetCustomerById(Guid customerId)
    {
        try
        {
            return _dbContext.Customers.ToList().Find(x => x.Id == customerId)!;
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
