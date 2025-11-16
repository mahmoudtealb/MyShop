using Microsoft.AspNetCore.Identity.UI.Services;

namespace MyShop.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // TODO: Implement email sending logic
            return Task.CompletedTask;
        }
    }
} 