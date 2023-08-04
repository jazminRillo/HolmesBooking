using System.Text;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Twilio;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.development.json", optional: true)
    .AddJsonFile("appsettings.production.json", optional: true)
    .Build();

builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema de Reservas API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                     policy =>
                     {
                         policy.WithOrigins(
                             "https://client.holmesbooking.com",
                             "http://client.holmesbooking.com",
                             "https://localhost:3000",
                             "http://holmessoftware-001-site4.atempurl.com",
                             "http://holmesbrewery.holmesbooking.com",
                             "https://holmesbrewery.holmesbooking.com",
                             "http://localhost:3000")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                     });
});

string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!;

string connectionString = configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<HolmeBookingDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.Cookie.Name = "booking";
    options.LoginPath = "/users";
});

builder.Services.AddAuthentication("JwtBearer")
        .AddJwtBearer("JwtBearer", options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = "holmesbooking.com",
                ValidAudience = "client.holmesbooking.com",
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET_KEY")!))
            };
        });

var accountSid = Environment.GetEnvironmentVariable("TWILIO_ACC");
var authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
if (authToken != null) { TwilioClient.Init(accountSid, authToken); }

builder.Services.AddSignalR();
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();


app.UseExceptionHandler("/Error/500"); // Redirige a la página de error para errores del servidor
app.UseStatusCodePagesWithReExecute("/Error/{0}"); // Redirige a la página de error para otros códigos de estado
app.UseHsts();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sistema de Reservas API v1");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("MyAllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationHub");
    endpoints.MapControllerRoute(
        name: "services",
        pattern: "services/{action=Index}/{id?}",
        defaults: new { controller = "Services" });

    endpoints.MapControllerRoute(
        name: "customers",
        pattern: "customers/{action=Index}/{id?}",
        defaults: new { controller = "Customers" });

    endpoints.MapControllerRoute(
        name: "reservations",
        pattern: "reservations/{action=Index}/{id?}",
        defaults: new { controller = "Reservations" });

    endpoints.MapControllerRoute(
        name: "users",
        pattern: "users/{action=Index}/{id?}",
        defaults: new { controller = "Users" });

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}"
    );
});


app.Run();


