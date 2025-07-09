using System.ComponentModel.DataAnnotations;

namespace QuizSite.Models;

public class AccountLoginModel
{
    [Required(ErrorMessage ="Giriş yapmak için email gerekli.")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage ="Giriş yapmak için şifre gerekli.")]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
}