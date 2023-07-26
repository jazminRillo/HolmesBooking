using System;
using System.Globalization;
using System.Text;
using HolmesBooking.DataBase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolmesBooking.Controllers
{
    [ApiController]
    [Route("search")]
    public class SearchController : Controller
    {
        private readonly HolmeBookingDbContext _dbContext;

        public SearchController(HolmeBookingDbContext dbContext)
		{
            _dbContext = dbContext;

        }

        [Authorize]
        [HttpGet("/search", Name = "Search")]
        public IActionResult Search(string search)
        {
            return View("Search", new { query = search });
        }

        public static string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        [EnableCors("_myAllowSpecificOrigins")]
        [HttpGet("/search-results", Name = "SearchResults")]
        public IActionResult SearchResults(string search, bool? showAll = false)
        {
            try
            {
                var customers = new List<Customer>();
                var reservations = new List<Reservation>();
                string searchNormalized = RemoveDiacritics(search);

                if (!showAll.GetValueOrDefault())
                {
                    customers = _dbContext.Customers.ToList()
                    .Where(c =>
                        (c.Name != null && RemoveDiacritics(c.Name).Contains(searchNormalized, StringComparison.OrdinalIgnoreCase)) ||
                        (c.Lastname != null && RemoveDiacritics(c.Lastname).Contains(searchNormalized, StringComparison.OrdinalIgnoreCase)) ||
                        (c.PhoneNumber != null && c.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (c.Email != null && c.Email.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .Take(3).ToList();

                } else {
                    customers = _dbContext.Customers.ToList()
                    .Where(c =>
                        (c.Name != null && RemoveDiacritics(c.Name).Contains(searchNormalized, StringComparison.OrdinalIgnoreCase)) ||
                        (c.Lastname != null && RemoveDiacritics(c.Lastname).Contains(searchNormalized, StringComparison.OrdinalIgnoreCase)) ||
                        (c.PhoneNumber != null && c.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (c.Email != null && c.Email.Contains(search, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                }
                

                var customerIds = customers.Select(c => c.Id).ToList();
                if (!showAll.GetValueOrDefault())
                {
                    reservations = _dbContext.Reservations
                    .Include(r => r.Service)
                    .Where(c => customerIds.Contains(c.Customer!.Id))
                    .Take(3).ToList();
                } else {
                    reservations = _dbContext.Reservations
                    .Include(r => r.Service)
                    .Where(c => customerIds.Contains(c.Customer!.Id))
                    .ToList();
                }
                var reservationsByState = new Dictionary<Guid, Dictionary<State, int>>();
                foreach (var customer in customers)
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
                var searchResults = new
                {
                    Customers = customers.Select(c => new
                    {
                        Id = c.Id!,
                        Name = c.Name!,
                        Lastname = c.Lastname!,
                        Email = c.Email ?? "Sin datos de email",
                        Phone = c.PhoneNumber ?? "Sin datos de teléfono",
                        Classification = c.Classification!
                    }),
                    Reservations = reservations.Select(r => new
                    {
                        Id = r.Id!,
                        Date = GetDate(r.Time!.Value),
                        Time = r.Time!.Value.ToString("HH:mm"),
                        status = GetStateText(r.State!.Value),
                        customerName = r.Customer!.Name,
                        customerLastName = r.Customer!.Lastname,
                        people = r.NumberDiners,
                        service = r.Service!.Name
                    }),
                    ReservationsByState = reservationsByState
                };

                return Json(searchResults);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetDate(DateTime dateTime)
        {
            var culture = new CultureInfo("es-ES");
            return dateTime.ToString("D", culture);
        }

        private string GetStateText(State state)
        {
            switch (state)
            {
                case State.CONFIRMADA:
                    return "Confirmada";
                case State.CANCELADA:
                    return "Cancelada";
                case State.DEMORADA:
                    return "Demorada";
                case State.NOSHOW:
                    return "No Show";
                default:
                    return "Sin confirmar";
            }
        }
    }
}

