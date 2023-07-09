using System.Data;
using System.Security.Cryptography;
using System.Text;
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
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(customer.Password!, out passwordHash, out passwordSalt);
                    var newUser = new User
                    {
                        Username = customer.Email!,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt,
                        CustomerKey = customer.Id
                    };
                    _dbContext.Users.Add(newUser);
                    var userRole = new UserRoles
                    {
                        User = newUser,
                        Role = _dbContext.Roles.FirstOrDefault(x => x.Name == "User")!
                    };

                    _dbContext.UserRoles.Add(userRole);
                    _dbContext.SaveChanges();
                    if (User.Identity!.IsAuthenticated)
                    {
                        return View("AllCustomers", _dbContext.Customers.ToList());
                    }
                    return Ok(customer);
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
                if (!User.Identity!.IsAuthenticated)
                {
                    return View("AllCustomers", _dbContext.Customers.ToList());
                }
                return Ok(customer);
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

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }
}
