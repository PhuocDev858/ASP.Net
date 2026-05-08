namespace TranHuuPhuoc_2123110236.Services
{
    public interface IOtpStore
    {
        void SaveOtp(string email, string otp);
        bool VerifyOtp(string email, string otp);
        void RemoveOtp(string email);
    }
}