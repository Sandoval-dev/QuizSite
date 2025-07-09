using Microsoft.AspNetCore.Mvc.Rendering;

public class QuestionViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<AnswerOptionViewModel> Options { get; set; } = new()
    {
        new AnswerOptionViewModel(),
        new AnswerOptionViewModel(),
        new AnswerOptionViewModel(),
        new AnswerOptionViewModel()
    };

    public int CorrectOptionIndex { get; set; }
    public string CategoryName { get; set; } = string.Empty; // ✅ Ekledik

}