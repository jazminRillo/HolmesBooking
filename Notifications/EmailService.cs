
using System.Net;
using System.Net.Mail;
public class EmailService : IEmailService
{
    public EmailService()
    {
    }

    public async Task SendEmail(string recipientEmail, string subject, string message)
    {

        var emailMessage = new MailMessage();

        emailMessage.From = new MailAddress("reservas@holmesbooking.com", "Reservas Holmes");
        emailMessage.To.Add(new MailAddress(recipientEmail, "Reservas"));
        emailMessage.Subject = subject;
        emailMessage.IsBodyHtml = true;
        emailMessage.Body = message;
        var smtpClient = new SmtpClient("mail.holmesbooking.com");
        smtpClient.Credentials = new NetworkCredential("reservas@holmesbooking.com", Environment.GetEnvironmentVariable("SmtpPass"));
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Port = 8889;
        smtpClient.EnableSsl = false;
        await smtpClient.SendMailAsync(emailMessage);
    }
}


public interface IEmailService
{
    Task SendEmail(string recipientEmail, string subject, string message);
}

