using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
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
        // 🔹 Gửi email có QR code
        // ==============================
        public async Task<bool> SendEmailWithQrAsync(string toEmail, string subject, string htmlContent, string qrBase64)
        {
            try
            {
                string body = string.IsNullOrWhiteSpace(qrBase64)
                    ? htmlContent.Replace("{qrImage}", "<i>Không có mã QR</i>")
                    : htmlContent.Replace("{qrImage}", $"<img src='data:image/png;base64,{qrBase64}' width='220' />");

                var email = BuildEmailMessage(toEmail, subject, body);

                using var smtp = new SmtpClient();
                await ConnectAndSendAsync(smtp, email);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi gửi email + QR: " + ex.Message);
                return false;
            }
        }

        // ==============================
        // 🔧 Hàm tạo email chuẩn
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

        // ==============================
        // 🔧 Hàm kết nối & gửi SMTP
        // ==============================
        private async Task ConnectAndSendAsync(SmtpClient smtp, MimeMessage email)
        {
            var smtpServer = _configuration["EmailSettings:SmtpServer"];
            var port = int.Parse(_configuration["EmailSettings:Port"]);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            SecureSocketOptions socketOption =
                port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(smtpServer, port, socketOption);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
