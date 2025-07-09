using System.ComponentModel.DataAnnotations;

namespace QuizSite.Models;

public class AccountChangePasswordModel
{
    [Required(ErrorMessage ="Şuanki şifreniz gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Old Password")]
    public string OldPassword { get; set; } = null!;

    [Required(ErrorMessage ="Yeni şifreniz gerekli.")]
    [Display(Name = "New Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage ="Yeni şifrenin tekrarı gerekli.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    public string ConfirmPassword { get; set; } = null!;
}