
public class QuizViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CategoryName { get; set; } = null!;
    public int DurationInMinutes { get; set; }
    public int QuestionCount { get; set; }
}