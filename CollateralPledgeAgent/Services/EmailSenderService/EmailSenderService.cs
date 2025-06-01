using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CollateralPledgeAgent.Services
{
    public class SmtpSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class EmailSenderService : IEmailSenderService
    {
        private readonly SmtpSettings _smtp;

        public EmailSenderService(IOptions<SmtpSettings> options)
        {
            _smtp = options.Value;
        }

        public async Task SendConfirmationAsync(string toEmail, string subject, string bodyHtml, IList<string>? cc = null)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_smtp.Username));
            message.To.Add(MailboxAddress.Parse(toEmail));
            if (cc != null && cc.Count > 0)
            {
                foreach (var c in cc)
                {
                    message.Cc.Add(MailboxAddress.Parse(c));
                }
            }
            message.Subject = subject;
            var builder = new BodyBuilder { HtmlBody = bodyHtml };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
