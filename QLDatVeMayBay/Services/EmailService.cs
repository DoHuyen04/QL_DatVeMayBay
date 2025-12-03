using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace QLDatVeMayBay.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ==============================
        // 🔹 Gửi email cơ bản
        // ==============================
        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var email = BuildEmailMessage(toEmail, subject, htmlContent);
                using var smtp = new SmtpClient();
                await ConnectAndSendAsync(smtp, email);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi gửi email: " + ex.Message);
                return false;
            }
        }

        // ==============================
        // 🔹 Gửi email với QR code inline
        // ==============================
        public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string htmlContent, MimePart attachment)
        {
            try
            {
                var email = new MimeMessage();

                var senderName = _configuration["EmailSettings:SenderName"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];

                email.From.Add(new MailboxAddress(senderName, senderEmail));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = subject;

                // Nội dung email + QR code inline
                var builder = new BodyBuilder();
                builder.HtmlBody = htmlContent;

                // Thêm attachment (inline)
                if (attachment != null)
                {
                    builder.Attachments.Add(attachment);
                }

                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await ConnectAndSendAsync(smtp, email);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi gửi email với QR: " + ex.Message);
                return false;
            }
        }

        // ==============================
        // 🔧 Hàm tạo attachment QR code inline từ Base64
        // ==============================
        public MimePart BuildQrAttachment(string qrBase64, string contentId = "qrCodeId")
        {
            if (string.IsNullOrEmpty(qrBase64)) return null;

            var qrBytes = Convert.FromBase64String(qrBase64);
            var ms = new MemoryStream(qrBytes);

            var attachment = new MimePart("image", "png")
            {
                Content = new MimeContent(ms),
                ContentDisposition = new ContentDisposition(ContentDisposition.Inline),
                ContentId = contentId,
                FileName = "QRCode.png"
            };

            return attachment;
        }

        // ==============================
        // 🔧 Hàm kết nối & gửi SMTP
        // ==============================
        private async Task ConnectAndSendAsync(SmtpClient smtp, MimeMessage email)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            SecureSocketOptions socketOption = port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(smtpServer, port, socketOption);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        // ==============================
        // 🔧 Hàm build email cơ bản
        // ==============================
        private MimeMessage BuildEmailMessage(string toEmail, string subject, string htmlBody)
        {
            var senderName = _configuration["EmailSettings:SenderName"];
            var senderEmail = _configuration["EmailSettings:SenderEmail"];

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(senderName, senderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            email.Body = builder.ToMessageBody();
            return email;
        }
    }
}
