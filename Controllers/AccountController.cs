using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuizSite.Controllers;
using QuizSite.Models;
using QuizSite.Services;

public class AccountController : Controller
{
    private UserManager<IdentityUser> _userManager;
    private SignInManager<IdentityUser> _signInManager;
    private DataContext _context;
    private IEmailService _emailService;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IEmailService emailService, DataContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailService = emailService;
        _context = context;
    }

    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> Create(AccountCreateModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");
                return RedirectToAction("Login", "Account");
            }
            foreach (var error in result.Errors)
            {

                if (error.Code == "PasswordTooShort")
                {
                    ModelState.AddModelError("Password", "Şifreniz çok kısa.");
                    TempData["Message"] = "Şifreniz çok kısa.";
                }
                else if (error.Code == "PasswordRequiresNonAlphanumeric")
                {
                    ModelState.AddModelError("Password", "Şifre özel karakter içermelidir.");
                    TempData["Message"] = "Şifreniz özel karakter içermelidir.";
                }
                else if (error.Code == "PasswordRequiresDigit")
                {
                    ModelState.AddModelError("Password", "Şifreniz en az bir rakam içermelidir.");
                    TempData["Message"] = "Şifreniz en az bir rakam içermelidir.";
                }
                else if (error.Code == "PasswordRequiresUpper")
                {
                    ModelState.AddModelError("Password", "Şifreniz en az bir büyük harf içermelidir.");
                    TempData["Message"] = "Şifreniz en az bir büyük harf içermelidir.";
                }
                else
                {
                    ModelState.AddModelError("", error.Description); // Diğer bilinmeyen hatalar
                    TempData["Message"] = error.Description;
                }
            }
        }
        return View(model);
    }


    public ActionResult Login()
    {
        return View();
    }


    [HttpPost]
    public async Task<ActionResult> Login(AccountLoginModel model, string? returnUrl)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                await _signInManager.SignOutAsync();
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    var lockoutDate = await _userManager.GetLockoutEndDateAsync(user);
                    var timeLeft = lockoutDate - DateTimeOffset.UtcNow;
                    ModelState.AddModelError(string.Empty, $"Your account is locked out until {lockoutDate}. Time left: {timeLeft?.TotalMinutes} minutes.");
                    TempData["Message"] = $"Your account is locked out until {lockoutDate}. Time left: {timeLeft?.TotalMinutes} minutes.";
                }
                else
                {
                    TempData["Message"] = "Email ya da şifreniz hatalı. Kontrol edin.";
                    ModelState.AddModelError(string.Empty, "Email ya da şifreniz hatalı. Kontrol edin.");
                }
            }
            else
            {
                TempData["Message"] = "Kullanıcı bulunmadı. Emaili kontrol edin.";
                ModelState.AddModelError(string.Empty, "Kullanıcı bulunmadı. Emaili kontrol edin.");
            }
        }
        return View(model);
    }

    [Authorize]
    public async Task<ActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }


    public ActionResult AccessDenied()
    {
        return View();
    }

    [Authorize]
    public async Task<ActionResult> Settings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            TempData["Message"] = "User not found.";
        }
        return View(new AccountEditUserModel
        {
            Email = user?.Email!,
        });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Settings(AccountEditUserModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Index");

        user.Email = model.Email;
        user.UserName = user.Email;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Message"] = "Email başarıyla güncellendi.";
            return RedirectToAction(nameof(Settings));
        }
        else
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

    }

    [Authorize]
    public ActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> ChangePassword(AccountChangePasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                }
                foreach (var error in result.Errors)
                {
                    if (error.Code == "PasswordMismatch")
                    {
                        ModelState.AddModelError("OldPassword", "Mevcut şifreniz yanlış girildi.");
                        TempData["Message"] = "Mevcut şifreniz hatalı.";
                    }
                    else if (error.Code == "PasswordTooShort")
                    {
                        ModelState.AddModelError("Password", "Şifreniz çok kısa.");
                        TempData["Message"] = "Şifreniz çok kısa.";
                    }
                    else if (error.Code == "PasswordRequiresNonAlphanumeric")
                    {
                        ModelState.AddModelError("Password", "Şifre özel karakter içermelidir.");
                        TempData["Message"] = "Şifreniz özel karakter içermelidir.";
                    }
                    else if (error.Code == "PasswordRequiresDigit")
                    {
                        ModelState.AddModelError("Password", "Şifreniz en az bir rakam içermelidir.");
                        TempData["Message"] = "Şifreniz en az bir rakam içermelidir.";
                    }
                    else if (error.Code == "PasswordRequiresUpper")
                    {
                        ModelState.AddModelError("Password", "Şifreniz en az bir büyük harf içermelidir.");
                        TempData["Message"] = "Şifreniz en az bir büyük harf içermelidir.";
                    }
                    else
                    {
                        ModelState.AddModelError("", error.Description); // Diğer bilinmeyen hatalar
                        TempData["Message"] = error.Description;
                    }
                }

            }
        }
        return View(model);
    }


    public ActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["Message"] = "Email gerekli.";
            return View();
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            TempData["Message"] = "Geçerli bir kullanıcı bulunamadı.";
            return View();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = Url.Action("ResetPassword", "Account", new { userId = user.Id, token }, Request.Scheme);

        if (url == null)
        {
            TempData["Message"] = "Sıfırlama linki oluşturulamadı.";
            return View();
        }

        var message = $"Şifreni sıfırlamak için buraya tıkla: <a target='blank' href='{url}'>link</a>";
        await _emailService.SendEmailAsync(user.Email, "Şifre Sıfırlama", message);

        TempData["Message"] = "Şifre sıfırlama linki mail adresinize gönderildi.";
        return RedirectToAction("Login");
    }


    public async Task<ActionResult> ResetPassword(string userId, string token)
    {
        if (userId == null || token == null)
        {
            TempData["Message"] = "Geçersiz şifre sıfırlama linki.";
            return RedirectToAction("Login");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            TempData["Message"] = "Kullanıcı bulunamadı.";
            return RedirectToAction("Login");
        }

        var model = new AccountResetPasswordModel
        {
            Token = token,
            Email = user.Email!
        };
        return View(model);
    }


    [HttpPost]
    public async Task<ActionResult> ResetPassword(AccountResetPasswordModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["Message"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Login");
            }

            // ✅ Yeni şifre eskisiyle aynı mı kontrol et
            var passwordHasher = new PasswordHasher<IdentityUser>();
            var compareResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, model.Password);
            if (compareResult == PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("Password", "Yeni şifreniz eski şifrenizle aynı olamaz.");
                TempData["Message"] = "Yeni şifreniz eski şifrenizle aynı olamaz.";
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["Message"] = "Şifre başarıyla sıfırlandı.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                switch (error.Code)
                {
                    case "PasswordMismatch":
                        ModelState.AddModelError("OldPassword", "Mevcut şifreniz yanlış girildi.");
                        TempData["Message"] = "Mevcut şifreniz hatalı.";
                        break;
                    case "PasswordTooShort":
                        ModelState.AddModelError("Password", "Şifreniz çok kısa.");
                        TempData["Message"] = "Şifreniz çok kısa.";
                        break;
                    case "PasswordRequiresNonAlphanumeric":
                        ModelState.AddModelError("Password", "Şifre özel karakter içermelidir.");
                        TempData["Message"] = "Şifreniz özel karakter içermelidir.";
                        break;
                    case "PasswordRequiresDigit":
                        ModelState.AddModelError("Password", "Şifreniz en az bir rakam içermelidir.");
                        TempData["Message"] = "Şifreniz en az bir rakam içermelidir.";
                        break;
                    case "PasswordRequiresUpper":
                        ModelState.AddModelError("Password", "Şifreniz en az bir büyük harf içermelidir.");
                        TempData["Message"] = "Şifreniz en az bir büyük harf içermelidir.";
                        break;
                    default:
                        ModelState.AddModelError("", error.Description);
                        TempData["Message"] = error.Description;
                        break;
                }
            }
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            TempData["Message"] = "Kullanıcı bulunamadı.";
            return RedirectToAction("Settings");
        }

        var userId = user.Id;

        var relatedQuizResults = _context.UserQuizResults.Where(qr => qr.UserId == userId);
        _context.UserQuizResults.RemoveRange(relatedQuizResults);

        await _context.SaveChangesAsync();
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            await _signInManager.SignOutAsync();
            TempData["Message"] = "Hesabınız başarıyla silindi.";
            return RedirectToAction("Login");
        }
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
            TempData["Message"] = error.Description;
        }
        return RedirectToAction("Settings");
    }

}