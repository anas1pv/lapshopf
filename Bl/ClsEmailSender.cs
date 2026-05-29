using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace lapshop.Bl
{
    public class ClsEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly LapShopContext _context;

        public ClsEmailSender(IConfiguration configuration, LapShopContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                string host = null;
                int port = 587;
                string username = null;
                string password = null;
                bool enableSsl = true;
                string fromEmail = "noreply@lapshop.com";

                try
                {
                    // Try reading SMTP settings from database
                    var dbSettings = _context.TbSettings.FirstOrDefault();
                    if (dbSettings != null)
                    {
                        host = dbSettings.SmtpHost;
                        port = dbSettings.SmtpPort ?? 587;
                        username = dbSettings.SmtpUsername;
                        password = dbSettings.SmtpPassword;
                        enableSsl = dbSettings.SmtpEnableSsl ?? true;
                        fromEmail = dbSettings.FromEmail ?? "noreply@lapshop.com";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load SMTP settings from DB, falling back. Error: {ex.Message}");
                }

                // Fallback to appsettings.json if database configuration is missing
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
                {
                    var smtpSettings = _configuration.GetSection("SmtpSettings");
                    host = smtpSettings["Host"];
                    port = int.Parse(smtpSettings["Port"] ?? "587");
                    username = smtpSettings["Username"];
                    password = smtpSettings["Password"];
                    enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                    fromEmail = smtpSettings["FromEmail"] ?? "noreply@lapshop.com";
                }

                // If SMTP username is still not configured, fallback to simulation
                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
                {
                    System.Diagnostics.Debug.WriteLine($"[EMAIL SIMULATION] To: {email}, Subject: {subject}, Body: {htmlMessage}");
                    return;
                }

                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, "LapShop");
                    mail.To.Add(new MailAddress(email));
                    mail.Subject = subject;
                    mail.Body = htmlMessage;
                    mail.IsBodyHtml = true;

                    using (var smtp = new SmtpClient(host, port))
                    {
                        smtp.Credentials = new NetworkCredential(username, password);
                        smtp.EnableSsl = enableSsl;
                        await smtp.SendMailAsync(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SMTP Send Error: {ex.Message}");
            }
        }
    }
}
