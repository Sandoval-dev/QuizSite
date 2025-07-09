using System.ComponentModel.DataAnnotations;

public class AnswerOptionCreateModel
{
    [Required(ErrorMessage = "Option text is required.")]
    [StringLength(250, ErrorMessage = "Option text can't be longer than 250 characters.")]
    public string Text { get; set; } = string.Empty;
}