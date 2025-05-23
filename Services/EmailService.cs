using GestionDesDépenses.Services.Interfaces;
using System.Net.Mail;

namespace GestionDesDépenses.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email address cannot be empty.", nameof(email));

            var smtpHost = _configuration["Email:SmtpHost"] ?? throw new InvalidOperationException("SMTP host is not configured.");
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? throw new InvalidOperationException("SMTP port is not configured."));
            var smtpUser = _configuration["Email:SmtpUser"] ?? throw new InvalidOperationException("SMTP user is not configured.");
            var smtpPass = _configuration["Email:SmtpPass"] ?? throw new InvalidOperationException("SMTP password is not configured.");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                Timeout = 10000 // 10-second timeout
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject ?? "No Subject",
                Body = message ?? string.Empty,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            try
            {
                await client.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new InvalidOperationException($"Failed to send email: SMTP error - {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send email: {ex.Message}", ex);
            }
        }
    }
}
