using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class QuizEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Süre gereklidir.")]
    [Range(1, 180, ErrorMessage = "Süre 1 ile 180 dakika arasında olmalıdır.")]
    [Display(Name = "Quiz Süresi (dakika)")]
    public int DurationInMinutes { get; set; }


    public List<SelectListItem> Categories { get; set; } = new();

    public List<int> SelectedQuestionIds { get; set; } = new();

    public List<SelectListItem> AllQuestions { get; set; } = new();
}
