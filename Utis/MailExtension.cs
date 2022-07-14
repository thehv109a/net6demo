using System.Net;
using System.Net.Mail;
using System.Text;

namespace netcore6demo.Utis
{
    public static class MailExtension
    {
        public static void Send(this Mail mail)
        {
            try
            {
                var smtpClient = new SmtpClient();
                smtpClient.Host = MailConfig.Host;
                smtpClient.Port = MailConfig.Port;
                smtpClient.EnableSsl = MailConfig.EnableSsl;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential()
                {
                    UserName = MailConfig.CredentialsUserName,
                    Password = MailConfig.CredentialsPassword
                };

                var mailMessage = new MailMessage(MailConfig.CredentialsUserName, mail.To);

                var view = AlternateView.CreateAlternateViewFromString(mail.Body, Encoding.UTF8, "text/html");
                mailMessage.AlternateViews.Add(view);
                mailMessage.IsBodyHtml = true;
                mailMessage.SubjectEncoding = Encoding.UTF8;
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.Subject = mail.Subject;
                mailMessage.Body = mail.Body;
                mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;

                if (mail.Stream != null)
                {
                    var attachment = new Attachment(mail.Stream, mail.FileName, mail.FileType);
                    mailMessage.Attachments.Add(attachment);
                }

                if (mail.Bccs != null && mail.Bccs.Any())
                {
                    foreach (var bcc in mail.Bccs)
                    {
                        mailMessage.Bcc.Add(bcc);
                    }
                }

                new Thread(() => smtpClient.Send(mailMessage)).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public class Mail
        {
            public string? To { get; set; }

            public string? Subject { get; set; }

            public string? Body { get; set; }
            
            public string[]? Bccs { get; set; }

            public Stream? Stream { get; set; }

            public string? FileName { get; set; }

            public string? FileType { get; set; }

            public string? SendForm { get; set; }
        }

        public static class MailConfig
        {
            public static string Host { get; set; } = "smtp.gmail.com";

            public static int Port { get; set; } = 587;

            public static bool EnableSsl { get; set; } = true;

            public static string Help { get; set; } = "https://myaccount.google.com/lesssecureapps";

            public static string CredentialsUserName { get; set; } = "thevinhuni@gmail.com";

            public static string CredentialsPassword { get; set; } = "********";
        }
    }
}