using System;

#nullable enable
namespace FinancialSystem.Domain.Entities
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// Specific Business Date assigned to this user / workstation session.
        /// Shown prominently in dashboard top-right corner.
        /// </summary>
        public DateTime BusinessDate { get; set; } = DateTime.Today;

        public string Role { get; set; } = "Trader";

        // 2-Factor Authentication state
        public string? CurrentOtp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public string? PreferredOtpChannel { get; set; } // "Mobile" or "Email"
        public bool IsTwoFactorEnabled { get; set; } = true;
    }
}
