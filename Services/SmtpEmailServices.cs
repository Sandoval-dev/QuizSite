using System.Net;
using System.Net.Mail;

namespace QuizSite.Services;


public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);

}


public class SmtpEmailService : IEmailService
{
    private IConfiguration _configuration;
    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        Console.WriteLine($"SMTP Username: {username}, Password length: {password?.Length}");

        using (var client = new SmtpClient())
        {
            client.Host = _configuration["Email:Host"]!;
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(
                _configuration["Email:Username"],
                _configuration["Email:Password"]);

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:Username"]!),
                Subject = subject,
                Body = message,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}