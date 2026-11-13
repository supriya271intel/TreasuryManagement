using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinancialSystem.Infrastructure.Data;
using FinancialSystem.Domain.Entities;
using FinancialSystem.WebUI.Models;

namespace FinancialSystem.WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var vm = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                OtpChannel = "Mobile"
            };

            // Read Remember Me cookies if present
            if (Request.Cookies.TryGetValue("RememberUser", out var remUser) && !string.IsNullOrEmpty(remUser))
            {
                vm.Username = remUser;
                vm.RememberMe = true;
                if (Request.Cookies.TryGetValue("RememberPass", out var remPass) && !string.IsNullOrEmpty(remPass))
                {
                    vm.Password = remPass;
                }
            }

            return View(vm);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());

            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password. Please try again.");
                return View(model);
            }

            // Handle Remember Me cookies
            if (model.RememberMe)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append("RememberUser", model.Username.Trim(), cookieOptions);
                Response.Cookies.Append("RememberPass", model.Password, cookieOptions);
            }
            else
            {
                Response.Cookies.Delete("RememberUser");
                Response.Cookies.Delete("RememberPass");
            }

            // Generate 6-digit OTP for 2-Factor Authentication
            var rng = new Random();
            var otp = rng.Next(100000, 999999).ToString();

            user.CurrentOtp = otp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.PreferredOtpChannel = model.OtpChannel;
            await _db.SaveChangesAsync();

            // Store pending user in session
            HttpContext.Session.SetString("Pending2FA_Username", user.Username);
            HttpContext.Session.SetString("Pending2FA_Channel", model.OtpChannel);
            HttpContext.Session.SetString("Pending2FA_SimulatedOtp", otp);

            return RedirectToAction(nameof(VerifyOtp), new { returnUrl = model.ReturnUrl });
        }

        // GET: /Account/VerifyOtp
        [HttpGet]
        public async Task<IActionResult> VerifyOtp(string? returnUrl = null)
        {
            var username = HttpContext.Session.GetString("Pending2FA_Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var channel = HttpContext.Session.GetString("Pending2FA_Channel") ?? user.PreferredOtpChannel ?? "Mobile";
            var masked = channel == "Email" ? MaskEmail(user.Email) : MaskMobile(user.PhoneNumber);
            var simOtp = HttpContext.Session.GetString("Pending2FA_SimulatedOtp") ?? user.CurrentOtp ?? "123456";

            var vm = new VerifyOtpViewModel
            {
                Username = user.Username,
                ChannelType = channel,
                MaskedDestination = masked,
                SimulatedOtp = simOtp,
                ReturnUrl = returnUrl
            };

            return View(vm);
        }

        // POST: /Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Verify OTP code and expiry
            if (string.IsNullOrWhiteSpace(user.CurrentOtp) ||
                user.CurrentOtp != model.EnteredOtp.Trim() ||
                !user.OtpExpiry.HasValue ||
                user.OtpExpiry.Value < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(model.EnteredOtp), "Invalid or expired OTP. Please enter the correct 6-digit verification code.");
                model.MaskedDestination = model.ChannelType == "Email" ? MaskEmail(user.Email) : MaskMobile(user.PhoneNumber);
                model.SimulatedOtp = user.CurrentOtp ?? "";
                return View(model);
            }

            // 2FA Verified! Clear OTP from database
            user.CurrentOtp = null;
            user.OtpExpiry = null;
            await _db.SaveChangesAsync();

            // Clear session 2FA temp values
            HttpContext.Session.Remove("Pending2FA_Username");
            HttpContext.Session.Remove("Pending2FA_Channel");
            HttpContext.Session.Remove("Pending2FA_SimulatedOtp");

            // Sign in user with Cookie Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Email", user.Email),
                new Claim("PhoneNumber", user.PhoneNumber),
                // Store User-Wise Business Date
                new Claim("BusinessDate", user.BusinessDate.ToString("yyyy-MM-dd")),
                new Claim("BusinessDateDisplay", user.BusinessDate.ToString("dd MMM yyyy"))
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Also keep user-wise business date in session for fast lookup
            HttpContext.Session.SetString("UserBusinessDate", user.BusinessDate.ToString("dd MMM yyyy"));
            HttpContext.Session.SetString("UserFullName", user.FullName);
            HttpContext.Session.SetString("Username", user.Username);

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}! Logged in successfully. Current Business Date: {user.BusinessDate:dd MMM yyyy}.";

            // Always redirect to Dashboard after successful 2FA verification
            return RedirectToAction("Index", "Home");


        }

        // POST: /Account/ResendOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string username, string channel)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return RedirectToAction(nameof(Login));

            var rng = new Random();
            var newOtp = rng.Next(100000, 999999).ToString();

            user.CurrentOtp = newOtp;
            user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
            user.PreferredOtpChannel = channel;
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("Pending2FA_SimulatedOtp", newOtp);
            TempData["SuccessMessage"] = $"A new 6-digit OTP has been dispatched to your registered {channel}.";

            return RedirectToAction(nameof(VerifyOtp));
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out safely.";
            return RedirectToAction(nameof(Login));
        }

        // POST: /Account/UpdateBusinessDate
        // Allows user to update their user-wise business date from the top-right widget
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBusinessDate(DateTime newBusinessDate)
        {
            var username = User.Identity?.Name ?? HttpContext.Session.GetString("Username");
            if (!string.IsNullOrEmpty(username))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user != null)
                {
                    user.BusinessDate = newBusinessDate.Date;
                    await _db.SaveChangesAsync();

                    HttpContext.Session.SetString("UserBusinessDate", user.BusinessDate.ToString("dd MMM yyyy"));

                    // Refresh claims identity with updated business date
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim("FullName", user.FullName),
                        new Claim(ClaimTypes.Role, user.Role),
                        new Claim("Email", user.Email),
                        new Claim("PhoneNumber", user.PhoneNumber),
                        new Claim("BusinessDate", user.BusinessDate.ToString("yyyy-MM-dd")),
                        new Claim("BusinessDateDisplay", user.BusinessDate.ToString("dd MMM yyyy"))
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    TempData["SuccessMessage"] = $"Business Date updated to {user.BusinessDate:dd MMM yyyy} for user {user.FullName}.";
                }
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index", "Home");
        }

        // Helpers
        private static string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@")) return email;
            var parts = email.Split('@');
            var name = parts[0];
            var domain = parts[1];
            if (name.Length <= 2) return $"{name[0]}*@{domain}";
            return $"{name.Substring(0, 2)}***@{domain}";
        }

        private static string MaskMobile(string mobile)
        {
            if (string.IsNullOrEmpty(mobile) || mobile.Length < 6) return mobile;
            var trimmed = mobile.Trim();
            var last4 = trimmed.Substring(trimmed.Length - 4);
            var prefix = trimmed.Substring(0, Math.Min(3, trimmed.Length - 4));
            return $"{prefix} ****** {last4}";
        }

        // GET: /Account/SignUp
        [HttpGet]
        public IActionResult SignUp()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View(new SignUpViewModel());
        }

        // POST: /Account/SignUp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check duplicate username
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.Trim().ToLower());
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Username), "This username is already taken. Please choose another.");
                return View(model);
            }

            // Check duplicate email
            var existingEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
                return View(model);
            }

            var newUser = new FinancialSystem.Domain.Entities.AppUser
            {
                FullName = model.FullName.Trim(),
                Username = model.Username.Trim(),
                Password = model.Password,
                Email = model.Email.Trim().ToLower(),
                PhoneNumber = model.PhoneNumber.Trim(),
                Role = "Trader",
                BusinessDate = System.DateTime.Today,
                PreferredOtpChannel = model.PreferredOtpChannel,
                IsTwoFactorEnabled = true
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Account created successfully for {newUser.FullName}! Please sign in with your new credentials.";
            return RedirectToAction(nameof(Login));
        }
    }
}
