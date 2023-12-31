using System.Data;
using System.Security.Cryptography;
using System.Text;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolmesBooking.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController : Controller
{
    private readonly HolmeBookingDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public CustomersController(IConfiguration configuration, IEmailService emailService, HolmeBookingDbContext dbContext)
    {
        _emailService = emailService;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [Authorize]
    [HttpGet("create-new-customer", Name = "CreateCustomer")]
    public IActionResult CreateCustomer()
    {
        return View("CreateCustomer", new Customer());
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpPost("/external-login", Name = "ExternalLogin")]
    public IActionResult ExternalLogin([FromForm] Customer customer)
    {
        try
        {
            var existingCustomer = _dbContext.Customers.FirstOrDefault(x => x.Email!.ToLower() == customer.Email!.ToLower());
            if (existingCustomer == null)
            {
                customer.Password = "google";
                _dbContext.Customers.Add(customer);
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(customer.Password!, out passwordHash, out passwordSalt);
                var newUser = new User
                {
                    Username = customer.Email!.ToLower(),
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
                return Ok(_dbContext.Customers.FirstOrDefault(x => x.Email == customer.Email));
            }
            else
            {
                return Ok(existingCustomer);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpPost("/forgot-password", Name = "ForgotPassword")]
    public IActionResult ForgotPassword([FromForm] string email)
    {
        try
        {
            var existingCustomer = _dbContext.Customers.FirstOrDefault(x => x.Email == email);
            var link = _configuration["AdminUrl"] + "/reset-password/" + existingCustomer!.Id;
            var message = $"<html>" +
                      $"<body>" +
                      $"<h2>{existingCustomer!.Name} {existingCustomer.Lastname}</h2>" +
                      $"<p>¿No recuerdas tu contraseña? Haciendo clic en el botón debajo lo dirigiremos a nuestro sitio web para crear una nueva contraseña.</p>" +
                      $"<p><a href='{link}'>CREAR NUEVA CONTRASEÑA</a></p>" +
                      $"<p>Si no olvidó su contraseña, o no solicitó iniciar este proceso, por favor ignore este correo electrónico.</p>" +
                      $"</body>" +
                      $"</html>";
            _emailService.SendEmail(email, "¿Olvidó su contraseña? Elige una nueva contraseña para acceder a las reservas.", message);
            return Ok("Email Enviado");
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/reset-password/{id}", Name = "ResetPassword")]
    public IActionResult ResetPasswrord(Guid id)
    {
        try
        {
            var existingCustomer = _dbContext.Customers.Find(id);
            if (existingCustomer != null)
            {
                var model = new { id = id, url = _configuration["FrontUrl"]!.ToString() };
                return View("ResetPassword", model);
            }
            else
            {
                return BadRequest("Cliente no encontrado");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/update-passwrord/{id}", Name = "UpdatePasswrord")]
    public IActionResult UpdatePasswrord(Guid id, [FromForm] string password)
    {
        try
        {
            var existingCustomer = _dbContext.Customers.Find(id);
            if (existingCustomer != null)
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);
                var existingUser = _dbContext.Users.FirstOrDefault(x => x.CustomerKey == existingCustomer!.Id);
                existingUser!.PasswordHash = passwordHash;
                existingUser.PasswordSalt = passwordSalt;
                _dbContext.Users.Update(existingUser);
                _dbContext.SaveChanges();
                return Ok("");
            }
            else
            {
                return BadRequest("Cliente no encontrado");
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpPost("/save-customer", Name = "SaveCustomer")]
    public IActionResult SaveCustomer([FromForm] Customer customer)
    {
        SaveCustomers(customer);
        return Ok(_dbContext.Customers.FirstOrDefault(x => x.Email == customer.Email));
    }

    [HttpPost("/save-customer-admin", Name = "SaveCustomerAdmin")]
    public IActionResult SaveCustomerAdmin([FromForm] Customer customer)
    {
        SaveCustomers(customer);
        if (User.Identity!.IsAuthenticated && !customer.CalledFromReservation.GetValueOrDefault())
        {
            return GetAllCustomers();
        }
        return Ok(_dbContext.Customers.FirstOrDefault(x => x.Email == customer.Email));
    }

    private IActionResult SaveCustomers(Customer customer)
    {
        try
        {
            if (CustomerValidations.IsNull(customer))
            {
                return BadRequest("Cliente NULO.");
            }

            if (CustomerValidations.IsNewCustomer(customer))
            {
                if (_dbContext.Customers.FirstOrDefault(x => x.Email == customer.Email) != null)
                {
                    return BadRequest("Cliente con ese email ya registrado");
                }

                if (CustomerValidations.IsValid(_dbContext.Customers.ToList(), customer))
                {
                    _dbContext.Customers.Add(customer);
                    if (string.IsNullOrEmpty(customer.Password))
                    {
                        customer.Password = "1234";
                    }
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(customer.Password!, out passwordHash, out passwordSalt);
                    var newUser = new User
                    {
                        Username = customer.Email!.ToLower(),
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
                    if (User.Identity!.IsAuthenticated && !customer.CalledFromReservation.GetValueOrDefault())
                    {
                        return GetAllCustomers();
                    }
                    return Ok(_dbContext.Customers.FirstOrDefault(x => x.Email == customer.Email));
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
                    existingCustomer.PhoneNumber = customer.PhoneNumber;
                    var existingUser = _dbContext.Users.FirstOrDefault(x => x.CustomerKey == existingCustomer!.Id);
                    if (existingCustomer.Password != customer.Password)
                    {
                        byte[] passwordHash, passwordSalt;
                        CreatePasswordHash(customer.Password!, out passwordHash, out passwordSalt);
                        existingUser!.PasswordHash = passwordHash;
                        existingUser.PasswordSalt = passwordSalt;
                        _dbContext.Users.Update(existingUser);
                    }
                    if (existingCustomer.Email!.ToLower() != customer.Email!.ToLower())
                    {
                        existingUser!.Username = customer.Email!.ToLower();
                        _dbContext.Users.Update(existingUser);
                        existingCustomer.Email = customer.Email!.ToLower();
                    }
                    _dbContext.Customers.Update(existingCustomer);
                    _dbContext.SaveChanges();
                }
                if (!User.Identity!.IsAuthenticated)
                {
                    return View("AllCustomers", _dbContext.Customers.ToList());
                }
                return Ok(existingCustomer);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpGet("/all-customers", Name = "GetAllCustomers")]
    [Authorize]
    public IActionResult GetAllCustomers(int page = 1, int pageSize = 8, [FromQuery] string? search = "")
    {
        try
        {
            var allCustomers = _dbContext.Customers.ToList();
            var reservationsByState = new Dictionary<Guid, Dictionary<State, int>>();

            if (!string.IsNullOrEmpty(search))
            {
                allCustomers = allCustomers
                .Where(c => c.Name!.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                            c.Lastname!.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
            }

            var totalCustomers = allCustomers.Count;

            var customersToDisplay = allCustomers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var customer in customersToDisplay)
            {
                customer.Reservations = _dbContext.Reservations
                    .Where(r => r.Customer!.Id == customer.Id)
                    .OrderByDescending(r => r.Time)
                    .Include(r => r.Service)
                    .ToList();

                var statusCount = customer.Reservations
                                    .Where(r => r.State != null)
                                    .GroupBy(r => r.State)
                                    .ToDictionary(g => g.Key ?? State.CONFIRMADA, g => g.Count());
                                        reservationsByState[customer.Id!.Value] = statusCount;
            }

            var totalPages = (int)Math.Ceiling((double)totalCustomers / pageSize);

            var viewModel = new AllCustomersViewModel
            {
                Customers = customersToDisplay,
                Page = page,
                TotalPages = totalPages,
                ReservationsByState = reservationsByState
            };

            return View("AllCustomers", viewModel);
        }
        catch (Exception error)
        {
            var a = error;
            return StatusCode(500);
        }
    }

    [HttpGet("edit-customer/{id}", Name = "EditCustomer")]
    [Authorize]
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
            var customer = _dbContext.Customers.ToList().Find(x => x.Id == customerId)!;
            customer.Reservations = _dbContext.Reservations
                    .Where(r => r.Customer!.Id == customer.Id)
                    .OrderByDescending(r => r.Time)
                    .Include(r => r.Service)
                    .ToList();
            return customer;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [HttpPost("update-customer", Name = "UpdateCustomer")]
    [Authorize]
    public IActionResult UpdateService(Customer customer)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Index");
        }

        return View("EditCustomer", customer);
    }

    [HttpGet("delete-customer", Name = "DeleteCustomer")]
    public IActionResult DeleteCustomer(Guid id)
    {
        var customerToDelete = _dbContext.Customers.FirstOrDefault(x => x.Id == id);
        if (customerToDelete != null)
        {
            _dbContext.Customers.Remove(customerToDelete);
            _dbContext.SaveChanges();
        }
        return GetAllCustomers();
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
