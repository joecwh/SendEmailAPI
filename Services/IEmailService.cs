using SendEmail.Models;

namespace SendEmail.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(Email email);
        Task<bool> SendEmailConfirmedAsync(string name, string email);
        Task<bool> SendAttachmentEmailAsync(string name, string email, IFormFileCollection files);
    }
}
