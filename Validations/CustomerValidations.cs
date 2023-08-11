using System.Text.RegularExpressions;
using HolmesBooking;

namespace HolmesBooking;

public class CustomerValidations
{
    public static bool IsNull(Customer? c)
    {
        return c == null;
    }

    public static bool IsNewCustomer(Customer c)
    {
        return c.Id == null;
    }

    public static bool IsPresent(List<Customer> customers, Guid Id)
    {
        Customer? c = customers.Find(x => x.Id == Id);
        return c != null;
    }

    public static Customer GetCustomer(List<Customer> customers, Guid Id)
    {
        return customers.Find(x => x.Id == Id)!;
    }

    public static bool
        IsValid(List<Customer> customers, Customer c)
    {
        if (string.IsNullOrEmpty(c.Name) || string.IsNullOrEmpty(c.Email))
        {
            return false;
        }

        if (customers.Any(x => x.Email!.Equals(c.Email)))
        {
            return false;
        }

        if (!Regex.IsMatch(c.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(c.PhoneNumber) &&!IsPhoneNumberValid(c.PhoneNumber!))
        {
            return false;
        }

        return true;
    }

    public static bool IsPhoneNumberValid(string phoneNumber)
    {
        string cleanedPhoneNumber = Regex.Replace(phoneNumber, @"[\s-]", "");
        return Regex.IsMatch(cleanedPhoneNumber, @"^(\+\d{1,3})?\d{10}$");
    }
}