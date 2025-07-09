using System.ComponentModel.DataAnnotations;

namespace QuizSite.Models;

public class AccountResetPasswordModel
{

    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;

    [Required(ErrorMessage ="Şifre gerekli.")]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage ="Şifre tekrarı boş bırakılamaz.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("Password", ErrorMessage = "Şifreler birbirleriyle eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = null!;
}