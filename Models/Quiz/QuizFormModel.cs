

using System.ComponentModel.DataAnnotations;

public class QuizFormModel
{
    [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description text is required.")]
    [StringLength(500, ErrorMessage = "Description text can't be longer than 500 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Quiz süresi gereklidir.")]
    [Range(1, 180, ErrorMessage = "Süre 1 ile 180 dakika arasında olmalıdır.")]
    [Display(Name = "Quiz Süresi (dakika)")]
    public int DurationInMinutes { get; set; }

    public List<int> SelectedQuestionIds { get; set; } = new();
}