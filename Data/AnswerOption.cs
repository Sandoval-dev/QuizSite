namespace QuizSite.Data;

public class AnswerOption
{
    public int Id { get; set; }
    public string Text { get; set; } = null!;
    public bool IsCorrect { get; set; }
}