using System;
using System.ComponentModel.DataAnnotations;

namespace FinancialSystem.WebUI.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 2FA channel selection: "Mobile" (SMS) or "Email"
        /// </summary>
        [Required]
        public string OtpChannel { get; set; } = "Mobile";

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }

    public class VerifyOtpViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the 6-digit OTP code")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must contain only numbers")]
        public string EnteredOtp { get; set; } = string.Empty;

        public string ChannelType { get; set; } = "Mobile"; // "Mobile" or "Email"
        public string MaskedDestination { get; set; } = string.Empty;
        
        /// <summary>
        /// Displayed in demo simulation badge so tester can copy with 1 click
        /// </summary>
        public string SimulatedOtp { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
