using System.ComponentModel.DataAnnotations;

namespace QuizSite.Models;

public class CategoryEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = null!;
}