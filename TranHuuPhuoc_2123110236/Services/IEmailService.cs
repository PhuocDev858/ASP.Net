using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace TranHuuPhuoc_2123110236.Services
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string fullName, string otp);
    }
}