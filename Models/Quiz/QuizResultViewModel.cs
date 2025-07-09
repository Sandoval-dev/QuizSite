public class QuizResultViewModel
{
    public string QuizTitle { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int BlankAnswers { get; set; }
    public int Percentage { get; set; }
    public DateTime TakenAt { get; set; }
}
