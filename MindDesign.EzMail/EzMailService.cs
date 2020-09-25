using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MindDesign.EzMail
{
    public class EzMailService : IEzMailService
    {
        private readonly EzMailConfig ezMailConfig;

        public IRazorViewToStringRenderer Renderer { get; private set; }

        public EzMailService(IRazorViewToStringRenderer renderer, IConfiguration configuration)
        {
            ezMailConfig = configuration
                .GetSection(EzMailConfig.EzMail)
                .Get<EzMailConfig>();

            this.Renderer = renderer;
        }

        public void SendMail(string subject, string body, string fromAddress, IList<string> toAddresses, IList<string> replyAddresses = null, IList<string> ccAddresses = null, IList<string> bccAddresses = null, IList<string> attachments = null)
        {
            Task.Run(async () =>
            {
                await SendMailAsync(subject, body, fromAddress, toAddresses, replyAddresses, ccAddresses, bccAddresses, attachments);
            })
                .GetAwaiter()
                .GetResult();
        }

        public async Task SendMailAsync(string subject, string body, string fromAddress, IList<string> toAddresses, IList<string> replyAddresses = null, IList<string> ccAddresses = null, IList<string> bccAddresses = null, IList<string> attachments = null)
        {
            var message = new MimeMessage();
            message.Subject = subject;

            message.From.Add(MailboxAddress.Parse(fromAddress));

            foreach (var toAddress in toAddresses)
            {
                if (MailboxAddress.TryParse(toAddress, out MailboxAddress parsedAddress))
                    message.To.Add(parsedAddress);
            }
            foreach (var replyAddress in replyAddresses ?? Enumerable.Empty<string>())
            {
                if (MailboxAddress.TryParse(replyAddress, out MailboxAddress parsedAddress))
                    message.ReplyTo.Add(parsedAddress);
            }
            foreach (var ccAddress in ccAddresses ?? Enumerable.Empty<string>())
            {
                if (MailboxAddress.TryParse(ccAddress, out MailboxAddress parsedAddress))
                    message.Cc.Add(parsedAddress);
            }
            foreach (var bccAddress in bccAddresses ?? Enumerable.Empty<string>())
            {
                if (MailboxAddress.TryParse(bccAddress, out MailboxAddress parsedAddress))
                    message.Bcc.Add(parsedAddress);
            }

            var builder = new BodyBuilder();

            builder.HtmlBody = body;

            foreach (var attachment in attachments ?? Enumerable.Empty<string>())
            {
                builder.Attachments.Add(attachment);
            }
            
            if (ezMailConfig.DebugData.Active)
            {
                message.Subject = $"[DEBUG] - {subject}";

                var debugText = $"To: {string.Join(", ", message.To.Mailboxes.Select(x => $"{x.Name} {x.Address}"))}<br/>";
                debugText += $"ReplyTo: {string.Join(", ", message.ReplyTo.Mailboxes.Select(x => $"{x.Name} {x.Address}"))}<br/>";
                debugText += $"Cc: {string.Join(", ", message.Cc.Mailboxes.Select(x => $"{x.Name} {x.Address}"))}<br/>";
                debugText += $"Bcc: {string.Join(", ", message.Bcc.Mailboxes.Select(x => $"{x.Name} {x.Address}"))}";

                builder.HtmlBody = $"{builder.HtmlBody}<hr/><div>{debugText}</div>";

                message.To.Clear();
                message.ReplyTo.Clear();
                message.Cc.Clear();
                message.Bcc.Clear();

                message.To.Add(MailboxAddress.Parse(ezMailConfig.DebugData.Email));
            }

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(ezMailConfig.SmtpParameters.Host, ezMailConfig.SmtpParameters.Port, ezMailConfig.SmtpParameters.EnableSSL);
                await client.AuthenticateAsync(ezMailConfig.SmtpParameters.Username, ezMailConfig.SmtpParameters.Password);

                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
        }
    }

    public interface IEzMailService
    {
        IRazorViewToStringRenderer Renderer { get; }

        Task SendMailAsync(
            string subject, string body,
            string fromAddress,
            IList<string> toAddresses,
            IList<string> replyAddresses = null,
            IList<string> ccAddresses = null,
            IList<string> bccAddresses = null,
            IList<string> attachments = null);

        void SendMail(
            string subject, string body,
            string fromAddress,
            IList<string> toAddresses,
            IList<string> replyAddresses = null,
            IList<string> ccAddresses = null,
            IList<string> bccAddresses = null,
            IList<string> attachments = null);
    }
}
