using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SendEmail.Models;
using System.Net;
using System.Net.Mail;

namespace SendEmail.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        public EmailService(IOptions<EmailSettings> option)
        {
            _settings = option.Value;
        }


        /// <summary>
        /// using MimeMessage
        /// simple send email method 
        /// only receive text message
        /// no template is used
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> SendEmailAsync(Email email)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(new MailboxAddress(email.ReceiverName, email.ReceiverEmail));
                message.Subject = email.Subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = email.Body;
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new MailKit.Net.Smtp.SmtpClient();
                await client.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// using MimeMessage
        /// embedded sendEmailAsync method
        /// use html template
        /// name params as the placeholder in html 
        /// tested embedded image into html
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> SendEmailConfirmedAsync(string name, string email)
        {
            try
            {
                string emailTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "Models/EmailHtmlTemplates/ConfirmEmailTemplate.html");
                string emailTemplate = await File.ReadAllTextAsync(emailTemplatePath);

                emailTemplate = emailTemplate.Replace("{{link}}", Guid.NewGuid().ToString());
                emailTemplate = emailTemplate.Replace("{{SenderName}}", _settings.SenderName);

                Email emailData = new Email
                {
                    ReceiverName = name,
                    ReceiverEmail = email,
                    Subject = "Confirm Email",
                    Body = emailTemplate
                };

                if(await SendEmailAsync(emailData))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// use MailMessage
        /// receive name as placeholder in html
        /// receive files as attachment
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public async Task<bool> SendAttachmentEmailAsync(string name, string email, IFormFileCollection files)
        {
            try
            {
                string emailTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "Models/EmailHtmlTemplates/AttachmentEmailTemplate.html");
                string emailTemplate = await File.ReadAllTextAsync(emailTemplatePath);

                emailTemplate = emailTemplate.Replace("{{ReceiverName}}", name);
                string description = "";
                foreach(var file in files)
                {
                    description += file.FileName + "<br>";
                }
                emailTemplate = emailTemplate.Replace("{{FileDescription}}", description);
                emailTemplate = emailTemplate.Replace("{{SenderName}}", _settings.SenderName);
                emailTemplate = emailTemplate.Replace("{{SenderEmail}}", _settings.SenderEmail);

                Email emailData = new Email
                {
                    ReceiverName = name,
                    ReceiverEmail = email,
                    Subject = "Attachment Email",
                    Body = emailTemplate
                };

                var message = new MailMessage(new MailAddress(_settings.SenderEmail, "Testing send Attachment Email"), new MailAddress(email));
                message.Subject = emailData.Subject;
                message.Body = emailData.Body;
                message.IsBodyHtml = true;
                foreach (var file in files)
                {
                    message.Attachments.Add(new Attachment(file.OpenReadStream(), file.FileName));
                }

                var smtp = new System.Net.Mail.SmtpClient();
                smtp.Host = _settings.SmtpServer;
                smtp.Port = _settings.Port;
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                NetworkCredential credential = new NetworkCredential();
                credential.UserName = _settings.SenderEmail;
                credential.Password = _settings.SenderPassword;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = credential;

                await smtp.SendMailAsync(message);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
