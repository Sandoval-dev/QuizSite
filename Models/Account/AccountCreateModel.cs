using System.ComponentModel.DataAnnotations;

namespace QuizSite.Models;

public class AccountCreateModel
{
    [Required(ErrorMessage ="Kaydolmak için email gerekli.")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage ="Kaydolmak için şifre gerekli.")]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage ="Şifre tekrarı gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "Şifreler birbirleriyle eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = null!;
}