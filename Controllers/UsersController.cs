using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("users")]
public class UsersController : Controller
{
    private readonly HolmeBookingDbContext _dbContext;

    public UsersController(HolmeBookingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public IActionResult LoginUser()
    {
        return View("LoginUser", new LoginViewModel { CalledFromAdmin = true });
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("login", Name = "Login")]
    public async Task<IActionResult> Login([FromForm] LoginViewModel login)
    {
        var user = _dbContext.Users
            .Include(u => u.UserRoles)!
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Username == login.Username);

        if (user == null ||
            !VerifyPasswordHash(login.Password, user.PasswordHash, user.PasswordSalt))
        {
            return RedirectToAction("LoginUser", new { error = "Credenciales inválidas" });
        }

        if (!login.CalledFromAdmin)
        {
            var customer = _dbContext.Customers.Find(user.CustomerKey);
            return Ok(customer);
        }

        if (user.UserRoles!.FirstOrDefault(x => x.Role.Name == "Admin") == null)
        {
            return RedirectToAction("LoginUser", new { error = "El usuario no tiene permisos" });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login.Username),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("register-user", Name = "RegisterUser")]
    public IActionResult RegisterUser()
    {
        return View("RegisterUser", new RegisterViewModel());
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("/register", Name = "Register")]
    public IActionResult Register([FromForm] RegisterViewModel user)
    {
        var existingUser = _dbContext.Users.FirstOrDefault(u => u.Username == user.Username);
        if (existingUser != null)
        {
            ModelState.AddModelError("", "El nombre de usuario ya está en uso");
            return View();
        }

        byte[] passwordHash, passwordSalt;
        CreatePasswordHash(user.Password, out passwordHash, out passwordSalt);

        var newUser = new User
        {
            Username = user.Username,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        if (user.Roles != null && user.Roles.Any())
        {
            foreach (var roleAdded in user.Roles)
            {
                var role = _dbContext.Roles.FirstOrDefault(r => r.Name == roleAdded.Name);

                if (role != null)
                {
                    var userRole = new UserRoles
                    {
                        User = newUser,
                        Role = role
                    };

                    newUser.UserRoles!.Add(userRole);
                }
            }
        }
        _dbContext.Users.Add(newUser);
        _dbContext.SaveChangesAsync();

        return RedirectToAction("LoginUser");
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }
    }

    [HttpGet("/logout", Name = "Logout")]
    public IActionResult Logout()
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != passwordHash[i])
            {
                return false;
            }
        }

        return true;
    }
}
