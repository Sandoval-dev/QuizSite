using System.ComponentModel.DataAnnotations;

namespace QuizSite.Models;

public class AccountEditUserModel
{
    [Required(ErrorMessage ="Email boş bırakılamaz.")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;
}