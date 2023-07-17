using HolmesBooking.Notifications;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }

    public void SendReservationConfirmationEmail(string recipientEmail, string subject, string message)
    {

        var emailMessage = new MailMessage();

        emailMessage.From = new MailAddress("reservas@holmesbooking.com", "Reservas Holmes");
        emailMessage.To.Add(new MailAddress(recipientEmail, "Reservas"));
        emailMessage.Subject = subject;
        emailMessage.Body = message;

        SmtpClient smtp = new SmtpClient(_emailSettings.SmtpServer);
        NetworkCredential Credentials = new NetworkCredential(_emailSettings.User, _emailSettings.Pass);
        smtp.UseDefaultCredentials = false;
        smtp.Credentials = Credentials;
        smtp.Port = _emailSettings.Port;
        smtp.EnableSsl = false;
        smtp.Send(emailMessage);
    }
}


public interface IEmailService
{
    void SendReservationConfirmationEmail(string recipientEmail, string subject, string message);
}

