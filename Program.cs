using HolmesBooking;
using HolmesBooking.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Obtener la configuración de la aplicación
var configuration = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();

// Agregar servicios a contenedor
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema de Reservas API", Version = "v1" });
});
builder.Services.AddSingleton<ServiceMocks>();
builder.Services.AddSingleton<CustomerMocks>();
builder.Services.AddSingleton<ReservationMocks>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://*",
                                              "http://localhost:3000").AllowAnyHeader();
                      });
});

string connectionString = configuration.GetConnectionString("DefaultConnection");

// Configurar la conexión a la base de datos
builder.Services.AddDbContext<HolmeBookingDbContext>(options =>
    options.UseSqlServer(connectionString));
var app = builder.Build();

// ...

// Configurar el middleware de enrutamiento y otros middleware

// ...

app.Run();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
app.UseCors();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
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
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});


app.Run();


