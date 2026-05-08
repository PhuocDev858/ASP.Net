using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TranHuuPhuoc_2123110236.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendOtpEmailAsync(string toEmail, string fullName, string otp)
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"] ?? "";
            var smtpPass = _configuration["Email:SmtpPass"] ?? "";
            var fromName = _configuration["Email:FromName"] ?? "PhuocOtakuShop";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, smtpUser));
            message.To.Add(new MailboxAddress(fullName, toEmail));
            message.Subject = "Mã OTP đặt lại mật khẩu - PhuocOtakuShop";
            message.Body = new TextPart("html")
            {
                Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: 0 auto;'>
                    <h2 style='color: #1890ff;'>Đặt lại mật khẩu</h2>
                    <p>Xin chào <strong>{fullName}</strong>,</p>
                    <p>Mã OTP của bạn là:</p>
                    <div style='background: #f0f0f0; padding: 20px; text-align: center; border-radius: 8px;'>
                        <h1 style='color: #1890ff; letter-spacing: 8px; margin: 0;'>{otp}</h1>
                    </div>
                    <p style='color: #888;'>Mã OTP có hiệu lực trong <strong>5 phút</strong>.</p>
                    <p style='color: #888;'>Nếu bạn không yêu cầu đặt lại mật khẩu, hãy bỏ qua email này.</p>
                    <hr/>
                    <p style='color: #aaa; font-size: 12px;'>© 2026 PhuocOtakuShop</p>
                </div>"
            };

            using var client = new SmtpClient();
            // ❌ Port 587 bị Render chặn
            // await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

            // ✅ Dùng port 465 với SSL
            await client.ConnectAsync(smtpHost, 465, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"OTP email sent to {toEmail}");
        }
    }
}