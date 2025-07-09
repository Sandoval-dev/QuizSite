

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

public class QuestionCreateModel
{
    [Required(ErrorMessage = "Question text is required.")]
    [StringLength(500, ErrorMessage = "Question text can't be longer than 500 characters.")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide 4 options.")]
    [MinLength(4, ErrorMessage = "At least 4 options are required.")]
    [MaxLength(4, ErrorMessage = "Maximum 4 options are allowed.")]
    public List<AnswerOptionCreateModel> Options { get; set; } = new List<AnswerOptionCreateModel>()
    {
        new AnswerOptionCreateModel(),
        new AnswerOptionCreateModel(),
        new AnswerOptionCreateModel(),
        new AnswerOptionCreateModel()
    };

    [Range(0, 3, ErrorMessage = "Correct option index must be between 0 and 3.")]
    public int CorrectOptionIndex { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = new();
}