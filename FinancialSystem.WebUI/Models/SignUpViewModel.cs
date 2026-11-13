using System.ComponentModel.DataAnnotations;

namespace FinancialSystem.WebUI.Models
{
    public class SignUpViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2–100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3–50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^[+]?[0-9\s\-]{8,15}$", ErrorMessage = "Enter a valid mobile number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string PreferredOtpChannel { get; set; } = "Mobile";

        public string Role { get; set; } = "Trader";
    }
}
