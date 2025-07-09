public class QuizSolveViewModel
{
    public int QuizId { get; set; }
    public string Title { get; set; } = null!;
    public int DurationInMinutes { get; set; }

    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalQues { get; set; }
    public List<QuestionSolveViewModel> Questions { get; set; } = new();
}
