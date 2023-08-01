using HolmesBooking.DataBase;
using HolmesBooking.Notifications;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Twilio;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
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
                             "http://localhost:3000")
                               .AllowAnyHeader()
                               .AllowAnyMethod();
                     });
});

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

var accountSid = "ACc63085fc41002a8338df0209550437cb";
var authToken = configuration["Twilio:AuthToken"];
TwilioClient.Init(accountSid, authToken);

builder.Services.AddSignalR();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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


