using System.Threading.Tasks;

namespace lapshop.Bl
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
