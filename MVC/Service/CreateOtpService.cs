using System;
using System.Collections.Generic;

namespace MVC.Service
{
    public enum OtpStatus
    {
        Success,
        Invalid,
        Expired
    }

    public class OtpService
    {
        private static Dictionary<string, (string Otp, DateTime Expiry)> otpStore
            = new Dictionary<string, (string, DateTime)>();

        public string GenerateOtp(string email)
        {
            Random rnd = new Random();
            string otp = rnd.Next(1000, 9999).ToString();

            otpStore[email] = (otp, DateTime.Now.AddMinutes(1));

            return otp;
        }

        public OtpStatus VerifyOtp(string email, string otp)
        {
            if (email == null || !otpStore.ContainsKey(email))
                return OtpStatus.Invalid;

            var stored = otpStore[email];

            // ⏱ expired
            if (DateTime.Now > stored.Expiry)
            {
                otpStore.Remove(email);
                return OtpStatus.Expired;
            }

            // ❌ wrong otp
            if (stored.Otp != otp)
                return OtpStatus.Invalid;

            // ✅ correct
            return OtpStatus.Success;
        }

        public void RemoveOtp(string email)
        {
            if (otpStore.ContainsKey(email))
                otpStore.Remove(email);
        }
    }
}