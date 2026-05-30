using lapshop.Bl;
using lapshop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Authorization;
using System.Text;

namespace lapshop.Controllers
{
    public class UsersController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IConfiguration _configuration;
        private IEmailSender _emailSender;

        public UsersController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public IActionResult Login(string returnUrl)
        {
            UserModel model = new UserModel() { ReturnUrl = returnUrl };
            return View(model);
        }

        public async Task<IActionResult> LoginOut()
        {
            await _signInManager.SignOutAsync();
            return Redirect("~/");
        }

        public IActionResult Register()
        {
            return View(new UserModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(UserModel model)
        {
            if (!ModelState.IsValid)
                return View("Register", model);

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
            {
                ModelState.AddModelError("Email", "This email is already in use.");
                return View(model);
            }

            ApplicationUser user = new ApplicationUser()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                UserName = model.Email
            };

            try
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var validCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Action("ConfirmEmail", "Users", new { userId = user.Id }, protocol: HttpContext.Request.Scheme);
                    var emailBody = $"<h3>Welcome to LapShop!</h3><p>Your email verification code is: <strong>{validCode}</strong></p><p>You can verify your email on this page: <a href='{callbackUrl}'>Verify Email</a></p>";
                    await _emailSender.SendEmailAsync(user.Email, "Email Verification - LapShop", emailBody);
                    var Myuser = await _userManager.FindByEmailAsync(model.Email);
                    await _userManager.AddToRoleAsync(Myuser, "Customer");
                    return RedirectToAction("ConfirmEmail", new { userId = user.Id });
                }

                // 4. إضافة أخطاء الـ Identity (مثل قوة الباسورد) للـ ModelState ليراها المستخدم
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string userId)
        {
            return View(new ConfirmEmailModel { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ConfirmEmailAsync(user, decodedCode);

            if (result.Succeeded)
                return RedirectToAction("Login");

            ModelState.AddModelError("", "Invalid Code!");
            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserModel model)
        {
            try
            {

                var loginResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, true);
                if (loginResult.Succeeded)
                {
                    if (string.IsNullOrEmpty(model.ReturnUrl))
                        return Redirect("~/");
                    return Redirect(model.ReturnUrl);
                }

                ModelState.AddModelError("", "Invalid Login Attempt.");
            }
            catch (Exception)
            {

            }
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AccountDetails()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new AccountDetailsViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(AccountDetailsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                TempData["ProfileError"] = "Invalid profile data.";
                return RedirectToAction(nameof(AccountDetails));
            }

            if (user.Email == "admin@lapshop.com" && user.Email != model.Email)
            {
                TempData["ProfileError"] = "The email address of the root admin account cannot be changed.";
                return RedirectToAction(nameof(AccountDetails));
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            if (user.Email != model.Email)
            {
                // Check if email is already taken
                var emailExists = await _userManager.FindByEmailAsync(model.Email);
                if (emailExists != null && emailExists.Id != user.Id)
                {
                    TempData["ProfileError"] = "Email is already in use by another account.";
                    return RedirectToAction(nameof(AccountDetails));
                }

                user.Email = model.Email;
                user.UserName = model.Email;
                user.EmailConfirmed = false; // Need re-verification in a real scenario
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["ProfileSuccess"] = "Profile updated successfully.";
            }
            else
            {
                TempData["ProfileError"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(AccountDetails));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (user.Email == "admin@lapshop.com")
            {
                TempData["PasswordError"] = "The password of the root admin account cannot be changed on this demo site.";
                return RedirectToAction(nameof(AccountDetails));
            }

            if (!ModelState.IsValid)
            {
                TempData["PasswordError"] = "Invalid password data.";
                return RedirectToAction(nameof(AccountDetails));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["PasswordSuccess"] = "Password changed successfully.";
            }
            else
            {
                TempData["PasswordError"] = string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(AccountDetails));
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.Email.Trim().ToLower() == "admin@lapshop.com")
            {
                ModelState.AddModelError("", "Password reset is disabled for the root admin account.");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "If the email is registered, a password reset link has been sent.");
                return View(model);
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var validCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action("ResetPassword", "Users", new { code = validCode, email = user.Email }, protocol: HttpContext.Request.Scheme);
            
            var emailBody = $"<h3>Reset Your Password - LapShop</h3><p>Please reset your password by <a href='{callbackUrl}'>clicking here</a>.</p>";
            await _emailSender.SendEmailAsync(user.Email, "Reset Password - LapShop", emailBody);

            ViewBag.Message = "A password reset link has been sent to your email. Please check your inbox.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code, string email)
        {
            if (code == null || email == null)
            {
                return RedirectToAction("Login");
            }
            var model = new ResetPasswordViewModel { Code = code, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (user.Email == "admin@lapshop.com")
            {
                ModelState.AddModelError("", "The password of the root admin account cannot be changed on this demo site.");
                return View(model);
            }

            var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            var result = await _userManager.ResetPasswordAsync(user, decodedCode, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }
    }
}