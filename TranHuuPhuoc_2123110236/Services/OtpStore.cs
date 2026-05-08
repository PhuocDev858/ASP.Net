namespace TranHuuPhuoc_2123110236.Services
{
    public class OtpStore : IOtpStore
    {
        private readonly Dictionary<string, (string Otp, DateTime ExpireAt)> _store = new();
        private readonly object _lock = new();

        public void SaveOtp(string email, string otp)
        {
            lock (_lock)
            {
                _store[email.ToLower()] = (otp, DateTime.Now.AddMinutes(5));
            }
        }

        public bool VerifyOtp(string email, string otp)
        {
            lock (_lock)
            {
                var key = email.ToLower();
                if (!_store.ContainsKey(key)) return false;

                var (savedOtp, expireAt) = _store[key];
                if (DateTime.Now > expireAt)
                {
                    _store.Remove(key);
                    return false;
                }

                return savedOtp == otp;
            }
        }

        public void RemoveOtp(string email)
        {
            lock (_lock)
            {
                _store.Remove(email.ToLower());
            }
        }
    }
}