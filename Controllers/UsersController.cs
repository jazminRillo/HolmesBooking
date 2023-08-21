using HolmesBooking.DataBase;
using HolmesBooking.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
    private readonly JwtHelper jwtHelper;
    private readonly Dictionary<string, string> apiKeys = new Dictionary<string, string>
    {
        { Environment.GetEnvironmentVariable("ApiKey")!, "HolmesBrewery" },
    };

    public UsersController(HolmeBookingDbContext dbContext)
    {
        _dbContext = dbContext;
        jwtHelper = new JwtHelper(Environment.GetEnvironmentVariable("SECRET_KEY")!);
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("getToken", Name = "GetToken")]
    public IActionResult GetToken([FromForm] ApiKeyRequestModel request)
    {
        if (apiKeys.TryGetValue(request.ApiKey!, out var username))
        {
            var token = jwtHelper.GenerateToken(username);
            return Ok(new { Token = token });
        }
        else
        {
            return Unauthorized();
        }
    }

    [HttpGet]
    public IActionResult LoginUser(string? returnUrl = null)
    {
        TempData["ReturnUrl"] = returnUrl;
        return View("LoginUser", new LoginViewModel { CalledFromAdmin = true });
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    [HttpPost("login", Name = "Login")]
    public IActionResult Login([FromForm] LoginViewModel login)
    {
        var user = _dbContext.Users
            .Include(u => u.UserRoles)!
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Username == login.Username);

        if (user == null ||
            !VerifyPasswordHash(login.Password!, user.PasswordHash!, user.PasswordSalt!))
        {
            return Unauthorized();
        }
        var customer = _dbContext.Customers.Find(user.CustomerKey);
        return Ok(customer);
    }

    [EnableCors("_myAllowSpecificOrigins")]
    [HttpPost("login-admin", Name = "LoginAdmin")]
    public async Task<IActionResult> LoginAdmin([FromForm] LoginViewModel login)
    {
        var user = _dbContext.Users
            .Include(u => u.UserRoles)!
            .ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Username == login.Username);

        if (user == null ||
            !VerifyPasswordHash(login.Password!, user.PasswordHash!, user.PasswordSalt!))
        {
            return View("LoginUser", new LoginViewModel { CalledFromAdmin = true, Error = "Credenciales inválidas" });
        }

        if (user.UserRoles!.FirstOrDefault(x => x.Role!.Name == "Admin") == null)
        {
            return View("LoginUser", new LoginViewModel { CalledFromAdmin = true, Error = "El usuario no tiene permisos" });
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, login.Username!),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        string? returnUrl = TempData["ReturnUrl"] as string;

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
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
        var existingUser = _dbContext.Users.FirstOrDefault(u => u.Username!.ToUpper() == user.Username!.ToUpper());
        if (existingUser != null)
        {
            ModelState.AddModelError("", "El nombre de usuario ya está en uso");
            return View();
        }

        CreatePasswordHash(user.Password!, out byte[] passwordHash, out byte[] passwordSalt);

        var newUser = new User
        {
            Username = user.Username,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        _dbContext.Users.Add(newUser);
        var userRole = new UserRoles
        {
            User = newUser,
            Role = _dbContext.Roles.FirstOrDefault(x => x.Name == "Admin")!
        };

        _dbContext.UserRoles.Add(userRole);
        _dbContext.SaveChangesAsync();

        return RedirectToAction("LoginUser");
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
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
